using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The global persistent controller — the "GameManager" of the thesis (§3.3).
///
/// Single-singleton rule: this is the sole DontDestroyOnLoad entity allowed in
/// active memory. It holds the high-level data model (the immutable SaveData
/// container), manages scene transitions, and owns the credit-gated chapter
/// progression loop with its rollback mechanism (§1.3.2, §2.7).
///
/// All other managers (PlayerData, StatSystem, TimeManager) are scene-local
/// compatibility facades that read from and write through this controller.
///
/// Persistence is routed through the JSON serialization layer (SaveSystem,
/// §3.4.2) instead of PlayerPrefs; legacy PlayerPrefs saves are migrated once.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    private static GameStateManager _instance;

    /// <summary>True while the application is quitting — guards lazy creation.</summary>
    public static bool IsQuitting { get; private set; }

    /// <summary>
    /// Kept property-shaped so every existing "GameStateManager.instance" call
    /// site keeps working. Lazily bootstraps the singleton so any scene can be
    /// entered directly (e.g. pressing Play inside Scene02 in the editor).
    /// </summary>
    public static GameStateManager instance
    {
        get
        {
            if (_instance == null && !IsQuitting && Application.isPlaying)
            {
                GameObject host = new GameObject("GameStateManager (auto)");
                host.AddComponent<GameStateManager>();
            }
            return _instance;
        }
    }

    /// <summary>Guarantees the GameManager exists (and is subscribed) before any scene loads.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        IsQuitting = false;
        GameStateManager _ = instance;
    }

    [Header("Credit-gated chapter progression (§1.3.2)")]
    [Tooltip("Credits required per academic year: chapter N gate = N × this value.")]
    [SerializeField] private int creditsPerChapter = 60;
    [Tooltip("On a failed chapter, reload the active scene so the Ink story is rebuilt from scratch (§2.7).")]
    [SerializeField] private bool reloadSceneOnChapterFail = true;
    [Tooltip("Optional scene loaded after a chapter is passed. Leave empty to let the story/scene decide.")]
    [SerializeField] private string sceneToLoadOnChapterPass = "";

    [SerializeField] private int currentSceneIndex = 0;
    [SerializeField] private float totalPlayTime = 0f;

    private bool isGamePaused = false;

    /// <summary>The active immutable state container (§3.4.3). Never null after Awake.</summary>
    public SaveData CurrentSave { get; private set; }

    /// <summary>
    /// Initialization snapshot for the chapter rollback mechanism (§2.7).
    /// Because SaveData is immutable, holding the reference IS the snapshot —
    /// no deep copy is required and no later mutation can corrupt it.
    /// </summary>
    private SaveData chapterStartSnapshot;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameState();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        if (_instance != null && _instance != this) return; // never subscribe a doomed duplicate

        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnCreditsAwarded += HandleCreditsAwarded;
        GameEvents.OnLectureAttended += HandleLectureAttended;
        GameEvents.OnLectureMissed += HandleLectureMissed;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnCreditsAwarded -= HandleCreditsAwarded;
        GameEvents.OnLectureAttended -= HandleLectureAttended;
        GameEvents.OnLectureMissed -= HandleLectureMissed;
    }

    void OnApplicationQuit()
    {
        IsQuitting = true;
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

    // ---- Event handlers: the only writers of the global state (§3.6) --------

    private void HandleCreditsAwarded(int amount)
    {
        int newTotal = Mathf.Max(0, CurrentSave.GetTotalCredits() + amount);
        CurrentSave = CurrentSave.WithTotalCredits(newTotal);
        GameEvents.RaiseCreditsChanged(newTotal);
    }

    private void HandleLectureAttended()
    {
        CurrentSave = CurrentSave.WithLecture(true);
    }

    private void HandleLectureMissed()
    {
        CurrentSave = CurrentSave.WithLecture(false);
    }

    // ---- State mutation API used by the scene-local facades ------------------

    /// <summary>Replaces one stat value; the clamped result is broadcast on the event hub.</summary>
    public void ApplyStatChange(string statName, int newValue)
    {
        string key = SaveData.NormaliseStatName(statName);
        if (CurrentSave.GetStat(key) == -1)
        {
            Debug.LogWarning($"GameStateManager: unknown stat '{statName}'");
            return;
        }

        int clamped = Mathf.Clamp(newValue, SaveData.StatMin, SaveData.StatMax);
        CurrentSave = CurrentSave.WithStat(key, clamped);
        GameEvents.RaiseStatChanged(key, clamped);
        GameEvents.RaiseInkSyncRequested(key, clamped);
    }

    /// <summary>Converts the customization input into an immutable string field in the data layer (§3.5).</summary>
    public void ApplyPlayerName(string newName)
    {
        CurrentSave = CurrentSave.WithPlayerName(newName);
        SaveGameState();
        GameEvents.RaiseInkSyncRequested("player_name", CurrentSave.GetPlayerName());
    }

    /// <summary>Marks the §4.5.2 support options as unlocked (idempotent).</summary>
    public void MarkSupportUnlocked()
    {
        if (CurrentSave.GetSupportUnlocked()) return;

        CurrentSave = CurrentSave.WithSupportUnlocked(true);
        GameEvents.RaiseSupportUnlocked();
        GameEvents.RaiseInkSyncRequested("support_unlocked", true);
        Debug.Log("GameStateManager: motivation reached the critical level — support options unlocked (§4.5.2).");
    }

    // ---- Credit-gated chapter progression (§1.3.2, §2.3, §2.7) ---------------

    public int GetCurrentChapterNumber() => CurrentSave.GetCompletedChapters() + 1;

    public int GetRequiredCreditsForCurrentChapter() => creditsPerChapter * GetCurrentChapterNumber();

    /// <summary>
    /// Called when a chapter scene initializes. Captures the immutable
    /// initialization snapshot that a later rollback will restore (§2.7).
    /// </summary>
    public void BeginChapter()
    {
        chapterStartSnapshot = CurrentSave;
        Debug.Log($"GameStateManager: Chapter {GetCurrentChapterNumber()} started — " +
                  $"credits {CurrentSave.GetTotalCredits()}/{GetRequiredCreditsForCurrentChapter()}.");
    }

    /// <summary>
    /// The progression trigger (§3.5): validates the mandatory credit threshold.
    /// Pass  → CompletedChapters advances and the state is saved. Returns true.
    /// Fail  → the initialization snapshot is restored (rolling back total
    ///         credits), and the chapter retry sequence reloads the scene so a
    ///         fresh Ink story instance is built from scratch (§2.7). Returns false.
    /// </summary>
    public bool EvaluateChapterEnd()
    {
        int chapter = GetCurrentChapterNumber();
        int earned = CurrentSave.GetTotalCredits();
        int required = GetRequiredCreditsForCurrentChapter();

        if (earned >= required)
        {
            CurrentSave = CurrentSave.WithCompletedChapters(chapter);
            SaveGameState();
            Debug.Log($"GameStateManager: Chapter {chapter} PASSED ({earned}/{required} credits).");
            GameEvents.RaiseChapterPassed(chapter);

            if (!string.IsNullOrEmpty(sceneToLoadOnChapterPass))
            {
                LoadSceneByName(sceneToLoadOnChapterPass);
            }

            return true;
        }
        else
        {
            if (chapterStartSnapshot != null)
            {
                CurrentSave = chapterStartSnapshot; // §2.7 rollback of total credits
            }
            SaveGameState();
            Debug.Log($"GameStateManager: Chapter {chapter} FAILED ({earned}/{required} credits) — retry sequence initiated.");
            GameEvents.RaiseChapterFailed(chapter, earned, required);

            if (reloadSceneOnChapterFail)
            {
                ReloadCurrentScene();
            }

            return false;
        }
    }

    /// <summary>Chapter retry (§3.5): clears scene memory; scene-local subsystems rebuild from scratch.</summary>
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---- Pause / resume --------------------------------------------------------

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
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

    // ---- Persistence (JSON serialization layer, §3.4.2) ------------------------

    public void SaveGameState()
    {
        CurrentSave = CurrentSave.WithSessionProgress(currentSceneIndex, totalPlayTime);

        if (SaveSystem.Save(CurrentSave))
        {
            Debug.Log($"Game saved to {SaveSystem.SaveFileName}: Scene {currentSceneIndex}, PlayTime: {totalPlayTime:0}s, Credits: {CurrentSave.GetTotalCredits()}");
        }
    }

    public void LoadGameState()
    {
        MigrateLegacyPrefsIfNeeded();

        CurrentSave = SaveSystem.LoadOrDefault();
        chapterStartSnapshot = CurrentSave;
        currentSceneIndex = CurrentSave.GetCurrentSceneIndex();
        totalPlayTime = CurrentSave.GetTotalPlayTime();
    }

    /// <summary>One-time migration from the old PlayerPrefs keys to gamesave.json.</summary>
    private void MigrateLegacyPrefsIfNeeded()
    {
        if (SaveSystem.SaveFileExists()) return;
        if (!PlayerPrefs.HasKey("GameState_HasSave") && !PlayerPrefs.HasKey("PlayerName")) return;

        SaveData migrated = SaveData.CreateDefault()
            .WithPlayerName(PlayerPrefs.GetString("PlayerName", "Player"))
            .WithStat("social", PlayerPrefs.GetInt("StatSystem_Social", SaveData.DefaultCoreStat))
            .WithStat("academic", PlayerPrefs.GetInt("StatSystem_Academic", SaveData.DefaultCoreStat))
            .WithStat("health", PlayerPrefs.GetInt("StatSystem_Health", SaveData.DefaultCoreStat))
            .WithSessionProgress(
                PlayerPrefs.GetInt("GameState_CurrentScene", 0),
                PlayerPrefs.GetFloat("GameState_PlayTime", 0f));

        if (SaveSystem.Save(migrated))
        {
            ClearLegacyPrefs();
            Debug.Log("GameStateManager: legacy PlayerPrefs save migrated to gamesave.json (§3.4.2).");
        }
    }

    private static void ClearLegacyPrefs()
    {
        PlayerPrefs.DeleteKey("GameState_CurrentScene");
        PlayerPrefs.DeleteKey("GameState_PlayTime");
        PlayerPrefs.DeleteKey("GameState_HasSave");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("StatSystem_Social");
        PlayerPrefs.DeleteKey("StatSystem_Academic");
        PlayerPrefs.DeleteKey("StatSystem_Health");
        PlayerPrefs.Save();
    }

    public void StartNewGame()
    {
        SaveSystem.Delete();
        ClearLegacyPrefs();

        CurrentSave = SaveData.CreateDefault();
        chapterStartSnapshot = CurrentSave;
        currentSceneIndex = 2; // Scene01 (build index: SplashScreen 0, MainMenu 1, Scene01 2)
        totalPlayTime = 0f;
        isGamePaused = false;
        Time.timeScale = 1f;

        SaveGameState();
        Debug.Log("New game started — clean default profile written to gamesave.json");
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
        return SaveSystem.SaveFileExists();
    }

    // ---- Read helpers ------------------------------------------------------------

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

    // ---- Scene routing -------------------------------------------------------------

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
