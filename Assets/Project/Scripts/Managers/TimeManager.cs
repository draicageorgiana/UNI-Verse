using UnityEngine;
using System;

/// <summary>
/// Scene-local time-of-day service (§4.2): supplies the parameters that shift
/// environmental lighting and audio backdrops.
///
/// Refactored for the single-singleton rule (§3.3): the component no longer
/// calls DontDestroyOnLoad. The session time offset survives scene loads in a
/// plain static field, and the component self-creates on first access, so
/// TimeManager.IsDay() is always safe to call from any scene.
/// </summary>
public class TimeManager : MonoBehaviour
{
    private static TimeManager _instance;

    /// <summary>Hours added by AdvanceTime/SetTime this session; survives scene reloads.</summary>
    private static float sessionOffsetHours = 0f;
    private static bool useFixedTime = false;
    private static float fixedTime = 0f;

    public static TimeManager instance
    {
        get
        {
            if (_instance == null && Application.isPlaying && !GameStateManager.IsQuitting)
            {
                _instance = FindFirstObjectByType<TimeManager>();
                if (_instance == null)
                {
                    _instance = new GameObject("TimeManager (scene-local)").AddComponent<TimeManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private float currentTime;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        RefreshCurrentTime();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void RefreshCurrentTime()
    {
        currentTime = useFixedTime
            ? fixedTime % 24f
            : (DateTime.Now.Hour + sessionOffsetHours) % 24f;
    }

    public static bool IsDay()
    {
        float time = GetCurrentTime();
        return time >= 6f && time < 18f;
    }

    // Check if it's currently night
    public static bool IsNight()
    {
        return !IsDay();
    }

    // Advance time by hours
    public static void AdvanceTime(float hours)
    {
        sessionOffsetHours = (sessionOffsetHours + hours) % 24f;
        if (instance != null) instance.RefreshCurrentTime();
    }

    // Set time directly
    public static void SetTime(float time)
    {
        useFixedTime = true;
        fixedTime = time % 24f;
        if (instance != null) instance.RefreshCurrentTime();
    }

    // Get current time
    public static float GetCurrentTime()
    {
        TimeManager tm = instance;
        if (tm == null) return DateTime.Now.Hour;
        tm.RefreshCurrentTime();
        return tm.currentTime;
    }
}
