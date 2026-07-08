using UnityEngine;

/// <summary>
/// The single, plain (non-MonoBehaviour) C# data container for the global game
/// state, marked [System.Serializable] so it stays separated from Unity's
/// object tracking systems.
///
/// Thesis references:
///   §3.4.2  JSON Serialization Layer  — this object is what JsonUtility.ToJson()
///           converts into the gamesave.json text written to local storage.
///   §3.4.3  Immutability Policy       — no public setters exist anywhere on this
///           class. Writing "PlayerData.TotalCredits += 10" (or mutating any field
///           from outside) is a compiler-level violation. The only way to change
///           state is to obtain a NEW instance through the constructor or one of
///           the With*() factory methods below.
///   §2.7    Chapter rollback          — because instances can never be mutated,
///           a chapter snapshot is simply a reference to the SaveData that was
///           active when the chapter began. Rolling back = restoring that reference.
///   Appendix D — core schema: { "PlayerName", "CompletedChapters", "TotalCredits" }.
///
/// Implementation note on field naming: Unity's JsonUtility writes JSON keys using
/// the exact C# field names, and it cannot populate fields declared with the
/// 'readonly' keyword when deserializing. The backing fields are therefore
/// private + PascalCase (matching Appendix D key-for-key) and immutability is
/// enforced structurally through access level instead of 'readonly'.
/// </summary>
[System.Serializable]
public class SaveData
{
    // ---- Core schema (Appendix D) ------------------------------------------
    [SerializeField] private string PlayerName;
    [SerializeField] private int CompletedChapters;
    [SerializeField] private int TotalCredits;

    // ---- Simulation metrics (§2.5 will-I-passometer inputs, §4.5.2) --------
    [SerializeField] private int SocialStat;
    [SerializeField] private int AcademicStat;
    [SerializeField] private int HealthStat;
    [SerializeField] private int Stress;             // hidden variable (§2.5)
    [SerializeField] private int Motivation;         // hidden variable (§2.5)
    [SerializeField] private int LecturesAttended;
    [SerializeField] private int LecturesMissed;
    [SerializeField] private bool SupportUnlocked;   // §4.5.2 support options

    // ---- Session bookkeeping (GameManager) ---------------------------------
    [SerializeField] private int CurrentSceneIndex;
    [SerializeField] private float TotalPlayTime;

    public const int StatMin = 0;
    public const int StatMax = 100;

    public const int DefaultCoreStat = 50;
    public const int DefaultStress = 25;
    public const int DefaultMotivation = 75;

    // ---- Full constructor: the ONLY write path (§3.4.3) --------------------
    public SaveData(
        string playerName,
        int completedChapters,
        int totalCredits,
        int socialStat,
        int academicStat,
        int healthStat,
        int stress,
        int motivation,
        int lecturesAttended,
        int lecturesMissed,
        bool supportUnlocked,
        int currentSceneIndex,
        float totalPlayTime)
    {
        // Defensive normalisation: even a hand-edited save file can never load
        // into an out-of-range state (part of the §3.8 security posture).
        PlayerName        = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
        CompletedChapters = Mathf.Max(0, completedChapters);
        TotalCredits      = Mathf.Max(0, totalCredits);
        SocialStat        = Mathf.Clamp(socialStat, StatMin, StatMax);
        AcademicStat      = Mathf.Clamp(academicStat, StatMin, StatMax);
        HealthStat        = Mathf.Clamp(healthStat, StatMin, StatMax);
        Stress            = Mathf.Clamp(stress, StatMin, StatMax);
        Motivation        = Mathf.Clamp(motivation, StatMin, StatMax);
        LecturesAttended  = Mathf.Max(0, lecturesAttended);
        LecturesMissed    = Mathf.Max(0, lecturesMissed);
        SupportUnlocked   = supportUnlocked;
        CurrentSceneIndex = Mathf.Max(0, currentSceneIndex);
        TotalPlayTime     = Mathf.Max(0f, totalPlayTime);
    }

    /// <summary>Clean default profile (§3.8: generated when a save is corrupt or absent).</summary>
    public static SaveData CreateDefault()
    {
        return new SaveData(
            playerName:        "Player",
            completedChapters: 0,
            totalCredits:      0,
            socialStat:        DefaultCoreStat,
            academicStat:      DefaultCoreStat,
            healthStat:        DefaultCoreStat,
            stress:            DefaultStress,
            motivation:        DefaultMotivation,
            lecturesAttended:  0,
            lecturesMissed:    0,
            supportUnlocked:   false,
            currentSceneIndex: 0,
            totalPlayTime:     0f);
    }

    // ---- Read-only surface ---------------------------------------------------
    public string GetPlayerName()       => PlayerName;
    public int  GetCompletedChapters()  => CompletedChapters;
    public int  GetTotalCredits()       => TotalCredits;
    public int  GetSocialStat()         => SocialStat;
    public int  GetAcademicStat()       => AcademicStat;
    public int  GetHealthStat()         => HealthStat;
    public int  GetStress()             => Stress;
    public int  GetMotivation()         => Motivation;
    public int  GetLecturesAttended()   => LecturesAttended;
    public int  GetLecturesMissed()     => LecturesMissed;
    public bool GetSupportUnlocked()    => SupportUnlocked;
    public int  GetCurrentSceneIndex()  => CurrentSceneIndex;
    public float GetTotalPlayTime()     => TotalPlayTime;

    /// <summary>Reads a simulation stat by name (social, academic, health, stress, motivation).</summary>
    public int GetStat(string statName)
    {
        switch (NormaliseStatName(statName))
        {
            case "social":     return SocialStat;
            case "academic":   return AcademicStat;
            case "health":     return HealthStat;
            case "stress":     return Stress;
            case "motivation": return Motivation;
            default:           return -1;
        }
    }

    public static string NormaliseStatName(string statName)
    {
        return string.IsNullOrEmpty(statName) ? "" : statName.Trim().ToLowerInvariant();
    }

    /// <summary>Structural sanity check used by the §3.8 loading pipeline.</summary>
    public bool IsValid()
    {
        return PlayerName != null
            && CompletedChapters >= 0
            && TotalCredits >= 0
            && SocialStat  >= StatMin && SocialStat  <= StatMax
            && AcademicStat >= StatMin && AcademicStat <= StatMax
            && HealthStat  >= StatMin && HealthStat  <= StatMax
            && Stress      >= StatMin && Stress      <= StatMax
            && Motivation  >= StatMin && Motivation  <= StatMax
            && LecturesAttended >= 0
            && LecturesMissed   >= 0;
    }

    // ---- Immutable update factories (§3.4.3): each discards the current -----
    // ---- instance and returns a completely new, fully populated container ---

    public SaveData WithPlayerName(string newName) => new SaveData(
        newName, CompletedChapters, TotalCredits,
        SocialStat, AcademicStat, HealthStat, Stress, Motivation,
        LecturesAttended, LecturesMissed, SupportUnlocked,
        CurrentSceneIndex, TotalPlayTime);

    public SaveData WithTotalCredits(int newTotal) => new SaveData(
        PlayerName, CompletedChapters, newTotal,
        SocialStat, AcademicStat, HealthStat, Stress, Motivation,
        LecturesAttended, LecturesMissed, SupportUnlocked,
        CurrentSceneIndex, TotalPlayTime);

    public SaveData WithCompletedChapters(int newCompleted) => new SaveData(
        PlayerName, newCompleted, TotalCredits,
        SocialStat, AcademicStat, HealthStat, Stress, Motivation,
        LecturesAttended, LecturesMissed, SupportUnlocked,
        CurrentSceneIndex, TotalPlayTime);

    /// <summary>Returns a new instance with one stat replaced (name: social, academic, health, stress, motivation).</summary>
    public SaveData WithStat(string statName, int newValue)
    {
        string key = NormaliseStatName(statName);
        return new SaveData(
            PlayerName, CompletedChapters, TotalCredits,
            key == "social"     ? newValue : SocialStat,
            key == "academic"   ? newValue : AcademicStat,
            key == "health"     ? newValue : HealthStat,
            key == "stress"     ? newValue : Stress,
            key == "motivation" ? newValue : Motivation,
            LecturesAttended, LecturesMissed, SupportUnlocked,
            CurrentSceneIndex, TotalPlayTime);
    }

    /// <summary>Registers one lecture as attended (true) or skipped (false).</summary>
    public SaveData WithLecture(bool attended) => new SaveData(
        PlayerName, CompletedChapters, TotalCredits,
        SocialStat, AcademicStat, HealthStat, Stress, Motivation,
        attended ? LecturesAttended + 1 : LecturesAttended,
        attended ? LecturesMissed : LecturesMissed + 1,
        SupportUnlocked, CurrentSceneIndex, TotalPlayTime);

    public SaveData WithSupportUnlocked(bool unlocked) => new SaveData(
        PlayerName, CompletedChapters, TotalCredits,
        SocialStat, AcademicStat, HealthStat, Stress, Motivation,
        LecturesAttended, LecturesMissed, unlocked,
        CurrentSceneIndex, TotalPlayTime);

    public SaveData WithSessionProgress(int sceneIndex, float playTime) => new SaveData(
        PlayerName, CompletedChapters, TotalCredits,
        SocialStat, AcademicStat, HealthStat, Stress, Motivation,
        LecturesAttended, LecturesMissed, SupportUnlocked,
        sceneIndex, playTime);
}
