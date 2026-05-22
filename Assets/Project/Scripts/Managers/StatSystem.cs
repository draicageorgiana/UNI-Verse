using UnityEngine;
using System;

public class StatSystem : MonoBehaviour
{
    public static StatSystem instance;
    
    [SerializeField] private int socialStat = 50;
    [SerializeField] private int academicStat = 50;
    [SerializeField] private int healthStat = 50;
    
    private const int MIN_STAT = 0;
    private const int MAX_STAT = 100;
    
    // Event fired when stats change
    public static event Action<string, int> OnStatChanged;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadStats();
    }
    
    public void ModifyStat(string statName, int amount)
    {
        switch (statName.ToLower())
        {
            case "social":
                socialStat = Mathf.Clamp(socialStat + amount, MIN_STAT, MAX_STAT);
                OnStatChanged?.Invoke("Social", socialStat);
                break;
                
            case "academic":
                academicStat = Mathf.Clamp(academicStat + amount, MIN_STAT, MAX_STAT);
                OnStatChanged?.Invoke("Academic", academicStat);
                break;
                
            case "health":
                healthStat = Mathf.Clamp(healthStat + amount, MIN_STAT, MAX_STAT);
                OnStatChanged?.Invoke("Health", healthStat);
                break;
                
            default:
                Debug.LogWarning($"StatSystem: Unknown stat '{statName}'");
                break;
        }
        
        SaveStats();
    }
    
    public int GetStat(string statName)
    {
        return statName.ToLower() switch
        {
            "social" => socialStat,
            "academic" => academicStat,
            "health" => healthStat,
            _ => -1
        };
    }
    
    public void SetStat(string statName, int value)
    {
        int clampedValue = Mathf.Clamp(value, MIN_STAT, MAX_STAT);
        
        switch (statName.ToLower())
        {
            case "social":
                socialStat = clampedValue;
                OnStatChanged?.Invoke("Social", socialStat);
                break;
                
            case "academic":
                academicStat = clampedValue;
                OnStatChanged?.Invoke("Academic", academicStat);
                break;
                
            case "health":
                healthStat = clampedValue;
                OnStatChanged?.Invoke("Health", healthStat);
                break;
        }
        
        SaveStats();
    }
    
    public void ResetAllStats()
    {
        socialStat = 50;
        academicStat = 50;
        healthStat = 50;
        SaveStats();
    }
    
    private void SaveStats()
    {
        PlayerPrefs.SetInt("StatSystem_Social", socialStat);
        PlayerPrefs.SetInt("StatSystem_Academic", academicStat);
        PlayerPrefs.SetInt("StatSystem_Health", healthStat);
        PlayerPrefs.Save();
    }
    
    private void LoadStats()
    {
        if (PlayerPrefs.HasKey("StatSystem_Social"))
        {
            socialStat = PlayerPrefs.GetInt("StatSystem_Social");
            academicStat = PlayerPrefs.GetInt("StatSystem_Academic");
            healthStat = PlayerPrefs.GetInt("StatSystem_Health");
        }
    }
}
