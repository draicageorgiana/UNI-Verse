using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    [SerializeField] private float currentTime;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            currentTime = DateTime.Now.Hour;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool IsDay()
    {
        return instance.currentTime >= 6f && instance.currentTime < 18f;
    }

    // Check if it's currently night
    public static bool IsNight()
    {
        return !IsDay();
    }

    // Advance time by hours
    public static void AdvanceTime(float hours)
    {
        instance.currentTime += hours;
        if (instance.currentTime >= 24f)
            instance.currentTime -= 24f;
    }

    // Set time directly
    public static void SetTime(float time)
    {
        instance.currentTime = time % 24f;
    }

    // Get current time
    public static float GetCurrentTime()
    {
        return instance.currentTime;
    }
}
