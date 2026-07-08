using UnityEngine;
using TMPro;

/// <summary>
/// Scene-local credit tracker (§2.3, §3.6) — strictly local to the scene, with
/// no direct references to neighbouring scripts.
///
/// Double-entry sync pattern (§3.6): when OnCreditsAwarded fires, this system
/// catches the message and adds the amount to its local C# counter for UI
/// tracking; when the GameManager broadcasts the authoritative new total, the
/// CreditSystem instructs the StoryManager (over the decoupled event channel)
/// to update the runtime variable memory inside the Ink engine, keeping both
/// engines perfectly aligned without script cross-references.
/// </summary>
public class CreditSystem : MonoBehaviour
{
    [Header("Optional UI (auto-built if left empty)")]
    [SerializeField] private TMP_Text creditsLabel;
    [SerializeField] private string labelFormat = "Credits: {0} / {1}";
    [SerializeField] private bool autoBuildLabel = true;

    /// <summary>Scene-local counter used purely for UI tracking (§3.6).</summary>
    private int localCreditCounter;

    void OnEnable()
    {
        GameEvents.OnCreditsAwarded += HandleCreditsAwarded;
        GameEvents.OnCreditsChanged += HandleCreditsChanged;
        GameEvents.OnChapterFailed += HandleChapterFailed;
    }

    void OnDisable()
    {
        GameEvents.OnCreditsAwarded -= HandleCreditsAwarded;
        GameEvents.OnCreditsChanged -= HandleCreditsChanged;
        GameEvents.OnChapterFailed -= HandleChapterFailed;
    }

    void Start()
    {
        if (creditsLabel == null && autoBuildLabel)
        {
            BuildRuntimeLabel();
        }

        RefreshFromGlobalState();
    }

    private void HandleCreditsAwarded(int amount)
    {
        localCreditCounter += amount;
        Debug.Log($"CreditSystem: {(amount >= 0 ? "+" : "")}{amount} credits (scene counter: {localCreditCounter}).");
    }

    private void HandleCreditsChanged(int newTotal)
    {
        UpdateLabel(newTotal);

        // Double-entry sync: push the authoritative total into Ink's variable memory.
        GameEvents.RaiseInkSyncRequested("total_credits", newTotal);
    }

    private void HandleChapterFailed(int chapter, int earned, int required)
    {
        // The rollback restored the snapshot; refresh the display from global state.
        RefreshFromGlobalState();
    }

    private void RefreshFromGlobalState()
    {
        if (GameStateManager.instance == null) return;

        localCreditCounter = GameStateManager.instance.CurrentSave.GetTotalCredits();
        UpdateLabel(localCreditCounter);
    }

    private void UpdateLabel(int total)
    {
        if (creditsLabel == null) return;

        int required = GameStateManager.instance != null
            ? GameStateManager.instance.GetRequiredCreditsForCurrentChapter()
            : 0;
        creditsLabel.text = string.Format(labelFormat, total, required);
    }

    /// <summary>Builds a minimal top-left HUD label so the scene works with zero editor wiring.</summary>
    private void BuildRuntimeLabel()
    {
        Canvas canvas = RuntimeUiFactory.FindOrCreateHudCanvas();

        RectTransform panel = RuntimeUiFactory.CreatePanel(canvas.transform, "CreditSystem Label (auto)",
            anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f), pivot: new Vector2(0f, 1f),
            anchoredPosition: new Vector2(16f, -16f), size: new Vector2(240f, 36f),
            backgroundColor: new Color(0f, 0f, 0f, 0.55f));

        creditsLabel = RuntimeUiFactory.CreateText(panel, "CreditsText", "Credits: 0 / 0", 18f,
            TextAlignmentOptions.Left, new Vector2(12f, 0f));
    }
}
