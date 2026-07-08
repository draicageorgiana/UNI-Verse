using UnityEngine;

/// <summary>
/// Scene-local compatibility facade over the player identity data.
///
/// The actual state now lives inside the immutable SaveData container owned by
/// the single persistent GameStateManager (§3.3 single-singleton rule) and is
/// persisted through the JSON serialization layer (§3.4.2). This component no
/// longer calls DontDestroyOnLoad and holds no state of its own.
///
/// Immutability (§3.4.3): PlayerData.PlayerName is a getter-only property —
/// assigning to it is a compiler-level violation. The only write path is
/// SetPlayerName(), which routes through the GameManager to produce a brand
/// new SaveData instance.
/// </summary>
public class PlayerData : MonoBehaviour
{
    private static PlayerData _instance;

    /// <summary>Kept for backwards compatibility with existing scenes; may be null.</summary>
    public static PlayerData instance => _instance;

    /// <summary>Read-only view of the player's chosen character name (§3.4.3).</summary>
    public static string PlayerName => GameStateManager.instance != null
        ? GameStateManager.instance.CurrentSave.GetPlayerName()
        : "Player";

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

    /// <summary>Character Customization use case (§3.5): stores the identity attribute immutably.</summary>
    public static void SetPlayerName(string name)
    {
        if (GameStateManager.instance == null) return;
        GameStateManager.instance.ApplyPlayerName(name);
    }

    public static string GetPlayerName()
    {
        return PlayerName;
    }

    public static void ClearData()
    {
        SetPlayerName("Player");
    }

    public static void SaveAllData()
    {
        if (GameStateManager.instance == null) return;
        GameStateManager.instance.SaveGameState();
        Debug.Log("PlayerData saved successfully");
    }

    public static void ResetAllData()
    {
        SetPlayerName("Player");
        Debug.Log("All player data has been reset");
    }
}
