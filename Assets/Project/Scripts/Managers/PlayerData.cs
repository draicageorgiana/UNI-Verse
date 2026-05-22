using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    private string playerName = "Player";

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
            return;
        }

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
    }

    public static void SetPlayerName(string name)
    {
        if (instance != null)
        {
            instance.playerName = name;
            PlayerPrefs.SetString("PlayerName", name);
            PlayerPrefs.Save();
        }
    }

    public static string GetPlayerName()
    {
        return instance != null ? instance.playerName : null;
    }

    public static void ClearData()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.Save();
        if (instance != null)
        {
            instance.playerName = "Player";
        }
    }
    
    public static void SaveAllData()
    {
        if (instance != null)
        {
            PlayerPrefs.SetString("PlayerName", instance.playerName);
            PlayerPrefs.Save();
            Debug.Log("PlayerData saved successfully");
        }
    }
    
    public static void ResetAllData()
    {
        ClearData();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        if (instance != null)
        {
            instance.playerName = "Player";
        }
        Debug.Log("All player data has been reset");
    }
}

