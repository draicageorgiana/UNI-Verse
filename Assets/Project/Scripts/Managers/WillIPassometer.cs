using UnityEngine;

/// <summary>The finite states of the will-I-passometer gauge (§2.5).</summary>
public enum PassometerState
{
    OnTrack,
    AtRisk,
    Critical
}

/// <summary>
/// The "will-I-passometer" (§2.5, §4.5.2): a dynamic status gauge running on an
/// underlying state machine that evaluates mathematical weights tied to daily
/// choices — attending a lecture raises the value, skipping one lowers it —
/// while balancing the hidden stress and motivation variables.
///
/// Fully event-driven (§4.2): the index is recalculated only when a relevant
/// event boundary is crossed (stat change, credit change, attendance change),
/// never inside a per-frame update loop. The result is broadcast on the
/// GameEvents hub, mirrored into Ink's variable memory, and — when the
/// motivation index falls below the critical level — the special support
/// options are unlocked, simulating real-world university support rules.
/// </summary>
public class WillIPassometer : MonoBehaviour
{
    [Header("Mathematical weights (§2.5) — should sum to 1")]
    [SerializeField] private float academicWeight = 0.40f;
    [SerializeField] private float attendanceWeight = 0.25f;
    [SerializeField] private float motivationWeight = 0.20f;
    [SerializeField] private float calmWeight = 0.15f; // rewards LOW stress

    [Header("State machine thresholds")]
    [SerializeField] private float onTrackThreshold = 0.65f;
    [SerializeField] private float atRiskThreshold = 0.40f;

    [Header("Support options (§4.5.2)")]
    [Tooltip("When motivation falls to or below this level, support options unlock.")]
    [SerializeField] private int criticalMotivationLevel = 25;

    /// <summary>Latest computed passing probability in [0,1]; -1 until first calculation.</summary>
    public float CurrentValue { get; private set; } = -1f;

    public PassometerState CurrentState { get; private set; } = PassometerState.AtRisk;

    void OnEnable()
    {
        GameEvents.OnStatChanged += HandleStatChanged;
        GameEvents.OnCreditsChanged += HandleCreditsChanged;
        GameEvents.OnLectureAttended += Recalculate;
        GameEvents.OnLectureMissed += Recalculate;
        GameEvents.OnChapterFailed += HandleChapterFailed;
    }

    void OnDisable()
    {
        GameEvents.OnStatChanged -= HandleStatChanged;
        GameEvents.OnCreditsChanged -= HandleCreditsChanged;
        GameEvents.OnLectureAttended -= Recalculate;
        GameEvents.OnLectureMissed -= Recalculate;
        GameEvents.OnChapterFailed -= HandleChapterFailed;
    }

    void Start()
    {
        Recalculate();
    }

    private void HandleStatChanged(string statName, int newValue) => Recalculate();
    private void HandleCreditsChanged(int newTotal) => Recalculate();
    private void HandleChapterFailed(int chapter, int earned, int required) => Recalculate();

    /// <summary>
    /// Pure weighted model of the passing probability (§2.5). Kept static and
    /// side-effect free so it can be validated in isolation (§3.10 evaluation).
    /// </summary>
    public static float Calculate(SaveData save,
        float wAcademic, float wAttendance, float wMotivation, float wCalm)
    {
        if (save == null) return 0f;

        int held = save.GetLecturesAttended() + save.GetLecturesMissed();
        float attendanceRatio = held == 0
            ? 0.5f // neutral before the first lecture decision
            : (float)save.GetLecturesAttended() / held;

        float score =
              wAcademic   * (save.GetAcademicStat() / 100f)
            + wAttendance * attendanceRatio
            + wMotivation * (save.GetMotivation() / 100f)
            + wCalm       * (1f - save.GetStress() / 100f);

        return Mathf.Clamp01(score);
    }

    private void Recalculate()
    {
        if (GameStateManager.instance == null) return;
        SaveData save = GameStateManager.instance.CurrentSave;

        float value = Calculate(save, academicWeight, attendanceWeight, motivationWeight, calmWeight);
        PassometerState state = ResolveState(value);

        bool changed = !Mathf.Approximately(value, CurrentValue) || state != CurrentState;
        CurrentValue = value;
        CurrentState = state;

        if (changed)
        {
            GameEvents.RaisePassometerChanged(value, state);
            GameEvents.RaiseInkSyncRequested("passometer", Mathf.RoundToInt(value * 100f));
        }

        // §4.5.2: if the motivation index falls below critical, unlock support options.
        if (save.GetMotivation() <= criticalMotivationLevel)
        {
            GameStateManager.instance.MarkSupportUnlocked();
        }
    }

    private PassometerState ResolveState(float value)
    {
        if (value >= onTrackThreshold) return PassometerState.OnTrack;
        if (value >= atRiskThreshold) return PassometerState.AtRisk;
        return PassometerState.Critical;
    }
}
