using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    
    [SerializeField] private int currentSceneIndex = 0;
    [SerializeField] private float totalPlayTime = 0f;
    
    private bool isGamePaused = false;
    private float pauseStartTime = 0f;
    
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
        LoadGameState();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Update()
    {
        if (!isGamePaused)
        {
            totalPlayTime += Time.deltaTime;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneIndex = scene.buildIndex;
        SaveGameState();
    }
    
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        pauseStartTime = Time.realtimeSinceStartup;
    }
    
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
    }
    
    public bool IsGamePaused()
    {
        return isGamePaused;
    }
    
    public void SaveGameState()
    {
        PlayerPrefs.SetInt("GameState_CurrentScene", currentSceneIndex);
        PlayerPrefs.SetFloat("GameState_PlayTime", totalPlayTime);
        PlayerPrefs.SetInt("GameState_HasSave", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"Game saved: Scene {currentSceneIndex}, PlayTime: {totalPlayTime}s");
    }
    
    public void LoadGameState()
    {
        if (PlayerPrefs.HasKey("GameState_CurrentScene"))
        {
            currentSceneIndex = PlayerPrefs.GetInt("GameState_CurrentScene");
            totalPlayTime = PlayerPrefs.GetFloat("GameState_PlayTime");
        }
    }
    
    public void StartNewGame()
    {
        // Clear all save data
        PlayerPrefs.DeleteKey("GameState_CurrentScene");
        PlayerPrefs.DeleteKey("GameState_PlayTime");
        PlayerPrefs.DeleteKey("GameState_HasSave");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("StatSystem_Social");
        PlayerPrefs.DeleteKey("StatSystem_Academic");
        PlayerPrefs.DeleteKey("StatSystem_Health");
        PlayerPrefs.Save();
        
        currentSceneIndex = 1; // Scene01
        totalPlayTime = 0f;
        isGamePaused = false;
        Time.timeScale = 1f;
        
        Debug.Log("New game started - all data cleared");
    }
    
    public void ContinueGame()
    {
        if (HasSaveData())
        {
            LoadGameState();
            Debug.Log($"Continuing from scene {currentSceneIndex}");
        }
        else
        {
            Debug.LogWarning("No save data found!");
        }
    }
    
    public bool HasSaveData()
    {
        return PlayerPrefs.GetInt("GameState_HasSave", 0) == 1;
    }
    
    public int GetCurrentSceneIndex()
    {
        return currentSceneIndex;
    }
    
    public float GetTotalPlayTime()
    {
        return totalPlayTime;
    }
    
    public string GetPlayTimeFormatted()
    {
        int hours = (int)(totalPlayTime / 3600f);
        int minutes = (int)((totalPlayTime % 3600f) / 60f);
        int seconds = (int)(totalPlayTime % 60f);
        
        return $"{hours}h {minutes}m {seconds}s";
    }
    
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void ExitGame()
    {
        SaveGameState();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
