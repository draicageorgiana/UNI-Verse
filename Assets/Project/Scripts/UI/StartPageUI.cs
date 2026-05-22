using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartPageUI : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image gameLogoImage;
    [SerializeField] private Text gameNameText;
    [SerializeField] private string loadingScreenSceneName = "LoadingScreen";
    [SerializeField] private string scene01Name = "Scene01";
    
    [SerializeField] private Sprite[] logoSprites;
    
    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        DisplayRandomLogo();
        CheckSaveData();
    }
    
    private void InitializeUI()
    {
        if (gameNameText != null)
        {
            gameNameText.text = "UNI-Verse";
        }
    }
    
    private void SetupButtonListeners()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }
    
    private void DisplayRandomLogo()
    {
        if (gameLogoImage != null && logoSprites != null && logoSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, logoSprites.Length);
            gameLogoImage.sprite = logoSprites[randomIndex];
        }
    }
    
    private void CheckSaveData()
    {
        bool hasSave = GameStateManager.instance != null && GameStateManager.instance.HasSaveData();
        
        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
            
            // Optionally change button appearance when disabled
            Button buttonComponent = continueButton.GetComponent<Button>();
            if (buttonComponent != null && !hasSave)
            {
                Image buttonImage = continueButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    // Reduce alpha to show it's disabled
                    Color disabledColor = buttonImage.color;
                    disabledColor.a = 0.5f;
                    buttonImage.color = disabledColor;
                }
            }
        }
    }
    
    private void OnNewGameClicked()
    {
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.StartNewGame();
        }
        
        if (StatSystem.instance != null)
        {
            StatSystem.instance.ResetAllStats();
        }
        
        PlayerData.ResetAllData();
        
        StartCoroutine(LoadSceneWithLoading(scene01Name));
    }
    
    private void OnContinueClicked()
    {
        if (GameStateManager.instance != null && GameStateManager.instance.HasSaveData())
        {
            GameStateManager.instance.ContinueGame();
            
            int sceneIndex = GameStateManager.instance.GetCurrentSceneIndex();
            string sceneName = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            
            StartCoroutine(LoadSceneWithLoading(sceneName));
        }
        else
        {
            Debug.LogWarning("No save data found");
        }
    }
    
    private IEnumerator LoadSceneWithLoading(string sceneName)
    {
        // Load loading screen first
        SceneManager.LoadScene(loadingScreenSceneName);
        yield return new WaitForSeconds(0.5f);
        
        // Then load actual scene
        SceneManager.LoadScene(sceneName);
    }
}
