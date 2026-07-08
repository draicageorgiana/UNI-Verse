using UnityEngine;

/// <summary>
/// Scene-local facade over the simulation metrics (§2.5).
///
/// Tracks the visible stats (social, academic, health) plus the hidden
/// balancing variables of the will-I-passometer model: stress and motivation.
/// The authoritative values live in the immutable SaveData container owned by
/// the persistent GameStateManager (§3.3 single-singleton rule) and every
/// change is broadcast on the decoupled GameEvents hub (§3.6) instead of a
/// component-owned delegate.
///
/// The component is scene-local (no DontDestroyOnLoad) and self-creates on
/// first access, so any scene can run standalone without manual wiring.
/// </summary>
public class StatSystem : MonoBehaviour
{
    private static StatSystem _instance;

    public static StatSystem instance
    {
        get
        {
            if (_instance == null && Application.isPlaying && !GameStateManager.IsQuitting)
            {
                _instance = FindFirstObjectByType<StatSystem>();
                if (_instance == null)
                {
                    _instance = new GameObject("StatSystem (scene-local)").AddComponent<StatSystem>();
                }
            }
            return _instance;
        }
    }

    public const int MIN_STAT = SaveData.StatMin;
    public const int MAX_STAT = SaveData.StatMax;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>Applies a delta to a stat (social, academic, health, stress, motivation).</summary>
    public void ModifyStat(string statName, int amount)
    {
        int current = GetStat(statName);
        if (current == -1)
        {
            Debug.LogWarning($"StatSystem: Unknown stat '{statName}'");
            return;
        }

        SetStat(statName, current + amount);
    }

    public int GetStat(string statName)
    {
        if (GameStateManager.instance == null) return -1;
        return GameStateManager.instance.CurrentSave.GetStat(statName);
    }

    public void SetStat(string statName, int value)
    {
        if (GameStateManager.instance == null) return;

        int clampedValue = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        GameStateManager.instance.ApplyStatChange(statName, clampedValue);
    }

    public void ResetAllStats()
    {
        SetStat("social", SaveData.DefaultCoreStat);
        SetStat("academic", SaveData.DefaultCoreStat);
        SetStat("health", SaveData.DefaultCoreStat);
        SetStat("stress", SaveData.DefaultStress);
        SetStat("motivation", SaveData.DefaultMotivation);
    }
}
