using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

/// <summary>
/// The scene-local narrative controller (§3.3, §3.6).
///
/// Owns the isolated Ink virtual machine: it initializes the narrative engine
/// from scratch on every scene load, reads the data values injected by the
/// global GameManager, pulls story text line by line, extracts the specialized
/// metadata tags embedded by the narrative writer, and fires the corresponding
/// static C# events. Unity stays in a passive presentation role (§3.2) — no
/// component ever touches the Story object directly.
///
/// Double-entry sync (§3.6): C#-side state changes arrive back over the
/// OnInkSyncRequested channel and are written into the Ink runtime variable
/// memory, keeping both engines perfectly aligned without cross-references.
///
/// Metadata tag glossary (written by the narrative author in the .ink file):
///   #speaker: Name          → dialogue name flag
///   #credits: +10           → award/deduct credits (fires OnCreditsAwarded)
///   #stat: academic +8      → adjust a stat (social/academic/health/stress/motivation)
///   #stress: +5             → shorthand for #stat: stress +5
///   #motivation: -10        → shorthand for #stat: motivation -10
///   #attend / #skip         → register lecture attendance for the passometer
///   #background: id         → environmental cue (OnBackgroundCue)
///   #audio: id              → audio cue (OnAudioCue)
///   #chapter_end            → run the credit-threshold gate (§2.7) on the next advance
/// </summary>
public class StoryManager : MonoBehaviour
{
    [Header("Compiled Ink story (the .json asset produced from the .ink file)")]
    [SerializeField] private TextAsset inkJsonAsset;

    [Header("Chapter integration")]
    [Tooltip("Take the §2.7 rollback snapshot when this scene starts.")]
    [SerializeField] private bool beginChapterOnStart = true;
    [Tooltip("If the story finishes without a #chapter_end tag, evaluate the credit gate anyway.")]
    [SerializeField] private bool evaluateChapterOnStoryEnd = true;
    [SerializeField] private bool autoStartStory = true;

    private Story story;
    private HashSet<string> declaredGlobals;
    private string currentSpeaker = "";
    private bool pendingChapterEvaluation;
    private bool chapterEvaluated;

    void OnEnable()
    {
        GameEvents.OnContinueRequested += HandleContinueRequested;
        GameEvents.OnChoiceSelected += HandleChoiceSelected;
        GameEvents.OnInkSyncRequested += HandleInkSyncRequested;
    }

    void OnDisable()
    {
        GameEvents.OnContinueRequested -= HandleContinueRequested;
        GameEvents.OnChoiceSelected -= HandleChoiceSelected;
        GameEvents.OnInkSyncRequested -= HandleInkSyncRequested;
    }

    void Start()
    {
        if (inkJsonAsset == null)
        {
            Debug.LogError("StoryManager: no compiled Ink story assigned. " +
                           "Assign the .json asset generated from your .ink file (e.g. Chapter1.json).");
            return;
        }

        InitializeStory();

        if (beginChapterOnStart && GameStateManager.instance != null)
        {
            GameStateManager.instance.BeginChapter();
        }

        if (autoStartStory)
        {
            ContinueStory();
        }
    }

    /// <summary>Builds a fresh Ink story instance from scratch (§2.7) and injects global state.</summary>
    private void InitializeStory()
    {
        story = new Story(inkJsonAsset.text);

        // Cache the globals actually declared by the narrative writer so we
        // only ever sync variables the story knows about.
        declaredGlobals = new HashSet<string>();
        foreach (string variableName in story.variablesState)
        {
            declaredGlobals.Add(variableName);
        }

        InjectGlobalState();
    }

    /// <summary>
    /// §3.3: the scene-local StoryManager reads the data values injected by the
    /// global GameManager and mirrors them into the Ink variable memory.
    /// </summary>
    private void InjectGlobalState()
    {
        if (GameStateManager.instance == null) return;
        SaveData save = GameStateManager.instance.CurrentSave;

        SetInkVariable("player_name", save.GetPlayerName());
        SetInkVariable("total_credits", save.GetTotalCredits());
        SetInkVariable("credits_required", GameStateManager.instance.GetRequiredCreditsForCurrentChapter());
        SetInkVariable("chapter", GameStateManager.instance.GetCurrentChapterNumber());
        SetInkVariable("academic", save.GetAcademicStat());
        SetInkVariable("social", save.GetSocialStat());
        SetInkVariable("health", save.GetHealthStat());
        SetInkVariable("stress", save.GetStress());
        SetInkVariable("motivation", save.GetMotivation());
        SetInkVariable("support_unlocked", save.GetSupportUnlocked());
    }

    private void SetInkVariable(string variableName, object value)
    {
        if (story == null || declaredGlobals == null) return;
        if (!declaredGlobals.Contains(variableName)) return;

        try
        {
            story.variablesState[variableName] = value;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"StoryManager: could not sync Ink variable '{variableName}' — {e.Message}");
        }
    }

    // ---- Event handlers (presentation layer → narrative logic, §3.6) --------

    private void HandleContinueRequested()
    {
        // A #chapter_end tag defers the credit gate until the player has read
        // the line, so the fail/retry reload never cuts a sentence short.
        if (pendingChapterEvaluation && !chapterEvaluated)
        {
            pendingChapterEvaluation = false;
            chapterEvaluated = true;

            bool passed = GameStateManager.instance != null && GameStateManager.instance.EvaluateChapterEnd();
            if (!passed)
            {
                return; // §2.7: retry sequence is reloading the scene
            }

            InjectGlobalState(); // chapter number / thresholds moved forward
        }

        ContinueStory();
    }

    private void HandleChoiceSelected(int index)
    {
        if (story == null) return;
        if (index < 0 || index >= story.currentChoices.Count) return;

        story.ChooseChoiceIndex(index);
        ContinueStory();
    }

    private void HandleInkSyncRequested(string variableName, object value)
    {
        SetInkVariable(variableName, value);
    }

    // ---- Core narrative loop ---------------------------------------------------

    /// <summary>
    /// Advances the story by exactly one displayable line (§4.4.3 pacing) —
    /// tag-only lines are consumed silently — then hands the text (or the
    /// choice set) to the presentation layer over the static event channel.
    /// </summary>
    public void ContinueStory()
    {
        if (story == null) return;

        while (story.canContinue)
        {
            string line = story.Continue();
            ProcessTags(story.currentTags);

            line = line != null ? line.Trim() : "";
            if (line.Length > 0)
            {
                GameEvents.RaiseDialogueLine(currentSpeaker, line);
                return;
            }
        }

        if (story.currentChoices.Count > 0)
        {
            List<string> choiceTexts = new List<string>(story.currentChoices.Count);
            foreach (Choice choice in story.currentChoices)
            {
                choiceTexts.Add(choice.text);
            }
            GameEvents.RaiseChoicesPresented(choiceTexts);
            return;
        }

        // Story exhausted.
        GameEvents.RaiseStoryEnded();

        if (!chapterEvaluated && evaluateChapterOnStoryEnd && GameStateManager.instance != null)
        {
            chapterEvaluated = true;
            GameStateManager.instance.EvaluateChapterEnd();
        }
    }

    /// <summary>
    /// §2.7: completely reloads the Ink story instance from scratch. The default
    /// retry path reloads the whole scene (which rebuilds this component), but
    /// this method also supports an in-place restart.
    /// </summary>
    public void RestartStory()
    {
        if (story == null) return;

        story.ResetState();
        currentSpeaker = "";
        pendingChapterEvaluation = false;
        chapterEvaluated = false;
        InjectGlobalState();
        ContinueStory();
    }

    // ---- Metadata tag extraction (§3.6) -----------------------------------------

    private void ProcessTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0) return;

        foreach (string rawTag in tags)
        {
            if (string.IsNullOrWhiteSpace(rawTag)) continue;

            string tag = rawTag.Trim();
            string key;
            string value;

            int colonIndex = tag.IndexOf(':');
            if (colonIndex >= 0)
            {
                key = tag.Substring(0, colonIndex).Trim().ToLowerInvariant();
                value = tag.Substring(colonIndex + 1).Trim();
            }
            else
            {
                key = tag.ToLowerInvariant();
                value = "";
            }

            switch (key)
            {
                case "speaker":
                    currentSpeaker = value;
                    break;

                case "credits":
                    if (TryParseDelta(value, out int creditDelta))
                    {
                        GameEvents.RaiseCreditsAwarded(creditDelta);
                    }
                    break;

                case "stat":
                    ApplyStatTag(value);
                    break;

                case "stress":
                    ApplyStatDelta("stress", value);
                    break;

                case "motivation":
                    ApplyStatDelta("motivation", value);
                    break;

                case "attend":
                    GameEvents.RaiseLectureAttended();
                    break;

                case "skip":
                    GameEvents.RaiseLectureMissed();
                    break;

                case "background":
                    GameEvents.RaiseBackgroundCue(value);
                    break;

                case "audio":
                    GameEvents.RaiseAudioCue(value);
                    break;

                case "chapter_end":
                    pendingChapterEvaluation = true;
                    break;

                default:
                    // Unknown tags are ignored on purpose: the narrative writer
                    // may annotate freely without breaking the engine (§3.2).
                    break;
            }
        }
    }

    /// <summary>Parses "academic +8" style payloads.</summary>
    private void ApplyStatTag(string value)
    {
        string[] parts = value.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            Debug.LogWarning($"StoryManager: malformed #stat tag '{value}' (expected: name delta).");
            return;
        }

        ApplyStatDelta(parts[0], parts[1]);
    }

    private void ApplyStatDelta(string statName, string deltaText)
    {
        if (!TryParseDelta(deltaText, out int delta))
        {
            Debug.LogWarning($"StoryManager: malformed stat delta '{deltaText}' for '{statName}'.");
            return;
        }

        if (StatSystem.instance != null)
        {
            StatSystem.instance.ModifyStat(statName, delta);
        }
    }

    private static bool TryParseDelta(string text, out int delta)
    {
        return int.TryParse(text, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out delta);
    }
}
