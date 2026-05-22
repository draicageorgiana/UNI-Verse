using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitGameButton;
    
    [SerializeField] private Text socialStatText;
    [SerializeField] private Image socialStatBar;
    [SerializeField] private Text academicStatText;
    [SerializeField] private Image academicStatBar;
    [SerializeField] private Text healthStatText;
    [SerializeField] private Image healthStatBar;
    
    [SerializeField] private Text playTimeText;
    
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private bool isPaused = false;
    
    void Start()
    {
        SetupButtonListeners();
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    private void SetupButtonListeners()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (exitGameButton != null)
        {
            exitGameButton.onClick.AddListener(ExitGame);
        }
    }
    
    private void Pause()
    {
        isPaused = true;
        
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.PauseGame();
        }
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            UpdateMenuDisplay();
        }
    }
    
    private void Resume()
    {
        isPaused = false;
        
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ResumeGame();
        }
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }
    
    private void UpdateMenuDisplay()
    {
        if (StatSystem.instance == null)
        {
            Debug.LogWarning("PauseMenuUI: StatSystem instance not found");
            return;
        }
        
        // Update Social Stat
        int socialStat = StatSystem.instance.GetStat("Social");
        if (socialStatText != null)
        {
            socialStatText.text = $"Social: {socialStat}/100";
        }
        if (socialStatBar != null)
        {
            socialStatBar.fillAmount = socialStat / 100f;
        }
        
        // Update Academic Stat
        int academicStat = StatSystem.instance.GetStat("Academic");
        if (academicStatText != null)
        {
            academicStatText.text = $"Academic: {academicStat}/100";
        }
        if (academicStatBar != null)
        {
            academicStatBar.fillAmount = academicStat / 100f;
        }
        
        // Update Health Stat
        int healthStat = StatSystem.instance.GetStat("Health");
        if (healthStatText != null)
        {
            healthStatText.text = $"Health: {healthStat}/100";
        }
        if (healthStatBar != null)
        {
            healthStatBar.fillAmount = healthStat / 100f;
        }
        
        // Update Play Time
        if (playTimeText != null && GameStateManager.instance != null)
        {
            string playTime = GameStateManager.instance.GetPlayTimeFormatted();
            playTimeText.text = $"Play Time: {playTime}";
        }
    }
    
    private void GoToMainMenu()
    {
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ResumeGame();
            GameStateManager.instance.SaveGameState();
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    private void ExitGame()
    {
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ResumeGame();
            GameStateManager.instance.SaveGameState();
            GameStateManager.instance.ExitGame();
        }
        else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
}
