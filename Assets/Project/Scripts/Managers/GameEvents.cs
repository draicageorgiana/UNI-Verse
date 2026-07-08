using System;
using System.Collections.Generic;

/// <summary>
/// The asynchronous, decoupled event-driven communication model (§3.6).
///
/// Subsystems are structurally blocked from searching for or reading state from
/// neighbouring scripts via direct references: communication happens exclusively
/// through these static C# delegate channels. Independent components subscribe
/// during their initialization phase (OnEnable) and unsubscribe during
/// destruction (OnDisable), so any subsystem can be rewritten or replaced
/// without requiring changes to adjacent classes (§3.2 Strict Component Isolation).
/// </summary>
public static class GameEvents
{
    // ---- Academic economy (§2.3 credit and criterion system) ---------------
    /// <summary>A choice/tag awarded (or deducted) credits. Payload: the delta.</summary>
    public static event Action<int> OnCreditsAwarded;
    /// <summary>The authoritative credit total changed. Payload: the new total.</summary>
    public static event Action<int> OnCreditsChanged;
    /// <summary>A simulation stat changed. Payload: canonical stat name, new value.</summary>
    public static event Action<string, int> OnStatChanged;
    public static event Action OnLectureAttended;
    public static event Action OnLectureMissed;

    // ---- Will-I-passometer (§2.5, §4.5.2) -----------------------------------
    /// <summary>Payload: passing probability in [0,1] and the FSM state bucket.</summary>
    public static event Action<float, PassometerState> OnPassometerChanged;
    /// <summary>Motivation fell below the critical level: support options unlocked (§4.5.2).</summary>
    public static event Action OnSupportUnlocked;

    // ---- Chapter progression (§1.3.2, §2.7) ---------------------------------
    /// <summary>Payload: the chapter number that was passed.</summary>
    public static event Action<int> OnChapterPassed;
    /// <summary>Payload: chapter number, credits earned, credits required.</summary>
    public static event Action<int, int, int> OnChapterFailed;

    // ---- Narrative presentation channel (StoryManager → DialogueUI, §3.6) ---
    /// <summary>Payload: speaker display name, dialogue text.</summary>
    public static event Action<string, string> OnDialogueLine;
    /// <summary>Payload: the choice texts, in Ink order.</summary>
    public static event Action<List<string>> OnChoicesPresented;
    /// <summary>Raised by the presentation layer when the player picks a choice.</summary>
    public static event Action<int> OnChoiceSelected;
    /// <summary>Raised by the presentation layer when the player advances dialogue.</summary>
    public static event Action OnContinueRequested;
    public static event Action OnStoryEnded;
    /// <summary>Environmental cues extracted from Ink metadata tags (§3.6).</summary>
    public static event Action<string> OnBackgroundCue;
    public static event Action<string> OnAudioCue;

    // ---- Double-entry sync channel (C# state → Ink variable memory, §3.6) ---
    /// <summary>Payload: Ink global variable name, new value.</summary>
    public static event Action<string, object> OnInkSyncRequested;

    // ---- Raise helpers -------------------------------------------------------
    public static void RaiseCreditsAwarded(int amount)                 => OnCreditsAwarded?.Invoke(amount);
    public static void RaiseCreditsChanged(int newTotal)               => OnCreditsChanged?.Invoke(newTotal);
    public static void RaiseStatChanged(string statName, int newValue) => OnStatChanged?.Invoke(statName, newValue);
    public static void RaiseLectureAttended()                          => OnLectureAttended?.Invoke();
    public static void RaiseLectureMissed()                            => OnLectureMissed?.Invoke();

    public static void RaisePassometerChanged(float value, PassometerState state) => OnPassometerChanged?.Invoke(value, state);
    public static void RaiseSupportUnlocked()                          => OnSupportUnlocked?.Invoke();

    public static void RaiseChapterPassed(int chapter)                 => OnChapterPassed?.Invoke(chapter);
    public static void RaiseChapterFailed(int chapter, int earned, int required) => OnChapterFailed?.Invoke(chapter, earned, required);

    public static void RaiseDialogueLine(string speaker, string text)  => OnDialogueLine?.Invoke(speaker, text);
    public static void RaiseChoicesPresented(List<string> choices)     => OnChoicesPresented?.Invoke(choices);
    public static void RaiseChoiceSelected(int index)                  => OnChoiceSelected?.Invoke(index);
    public static void RaiseContinueRequested()                        => OnContinueRequested?.Invoke();
    public static void RaiseStoryEnded()                               => OnStoryEnded?.Invoke();
    public static void RaiseBackgroundCue(string cue)                  => OnBackgroundCue?.Invoke(cue);
    public static void RaiseAudioCue(string cue)                       => OnAudioCue?.Invoke(cue);

    public static void RaiseInkSyncRequested(string variableName, object value) => OnInkSyncRequested?.Invoke(variableName, value);
}
