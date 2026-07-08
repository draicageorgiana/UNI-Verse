using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu lifecycle and scene initialization (§4.4.1 —
/// Assets/Project/Scripts/MainMenu.cs).
///
/// Handles user onboarding, establishes the configuration canvas overlay
/// (settings panel hosting the VolumeSettings sliders), reads persistent
/// storage variables through the JSON serialization layer, and routes
/// execution triggers to begin a new chapter, continue a saved run, or exit
/// the player loop safely.
///
/// Button listeners are wired in code, so dropping this component onto the
/// menu scene and assigning references is all the setup required. It can be
/// used alongside (or instead of) the earlier StartPageUI component.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Primary interface")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text saveInfoText;

    [Header("Configuration canvas overlay (§4.4.1)")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;

    [Header("Routing")]
    [SerializeField] private string firstSceneName = "Scene01";

    void Start()
    {
        WireButtons();
        InitializeInterface();
        ReadPersistentStorage();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void WireButtons()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(() => ToggleSettings(true));
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(() => ToggleSettings(false));
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void InitializeInterface()
    {
        if (titleText != null)
        {
            titleText.text = "UNI-Verse";
        }
    }

    /// <summary>
    /// §4.4.1: reads persistent storage variables (via the validated JSON
    /// loading pipeline) to decide which routes are available to the player.
    /// </summary>
    private void ReadPersistentStorage()
    {
        bool hasSave = SaveSystem.SaveFileExists();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
        }

        if (saveInfoText != null)
        {
            if (hasSave && GameStateManager.instance != null)
            {
                SaveData save = GameStateManager.instance.CurrentSave;
                saveInfoText.text = $"{save.GetPlayerName()} — Year {save.GetCompletedChapters() + 1}, " +
                                    $"{save.GetTotalCredits()} credits";
            }
            else
            {
                saveInfoText.text = "No saved journey yet";
            }
        }
    }

    private void OnNewGameClicked()
    {
        if (GameStateManager.instance == null) return;

        // Immutable baseline entries are written to the global profile layer
        // (a clean default SaveData serialized to gamesave.json), then the
        // primary curriculum tracking layout for Chapter 1 is loaded (§4.4.1).
        GameStateManager.instance.StartNewGame();
        GameStateManager.instance.LoadSceneByName(firstSceneName);
    }

    private void OnContinueClicked()
    {
        if (GameStateManager.instance == null || !GameStateManager.instance.HasSaveData())
        {
            Debug.LogWarning("MainMenu: no save data found");
            return;
        }

        GameStateManager.instance.ContinueGame();

        int savedSceneIndex = GameStateManager.instance.GetCurrentSceneIndex();

        // Indices 0-1 are SplashScreen/MainMenu: resuming there means the run
        // never reached a chapter, so route to the first chapter instead.
        if (savedSceneIndex > 1)
        {
            GameStateManager.instance.LoadScene(savedSceneIndex);
        }
        else
        {
            GameStateManager.instance.LoadSceneByName(firstSceneName);
        }
    }

    private void ToggleSettings(bool show)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(show);
        }
    }

    private void OnQuitClicked()
    {
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ExitGame();
        }
        else
        {
            Application.Quit();
        }
    }
}
