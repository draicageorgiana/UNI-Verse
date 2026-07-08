using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Real-time UI gauge for the will-I-passometer (§2.5): turns the hidden
/// weighted variables into a clear, always-visible element so the player sees
/// how small daily habits accumulate into major academic outcomes.
///
/// Purely presentational and event-driven (§4.2): it subscribes to the static
/// channel in OnEnable, unsubscribes in OnDisable (§3.6), and repaints only
/// when OnPassometerChanged fires — never in a per-frame update loop.
/// Wire the references in the Inspector, or leave them empty to auto-build a
/// minimal top-right HUD gauge at runtime.
/// </summary>
public class PassometerDisplay : MonoBehaviour
{
    [Header("Optional UI (auto-built if left empty)")]
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text valueLabel;
    [SerializeField] private bool autoBuildUi = true;

    [Header("State colours")]
    [SerializeField] private Color onTrackColor = new Color(0.30f, 0.78f, 0.38f);
    [SerializeField] private Color atRiskColor = new Color(0.95f, 0.75f, 0.20f);
    [SerializeField] private Color criticalColor = new Color(0.90f, 0.25f, 0.25f);

    void OnEnable()
    {
        GameEvents.OnPassometerChanged += HandlePassometerChanged;
        GameEvents.OnSupportUnlocked += HandleSupportUnlocked;
    }

    void OnDisable()
    {
        GameEvents.OnPassometerChanged -= HandlePassometerChanged;
        GameEvents.OnSupportUnlocked -= HandleSupportUnlocked;
    }

    void Start()
    {
        if (autoBuildUi && fillRect == null && fillImage == null)
        {
            BuildRuntimeUi();
        }

        // Paint the initial value if the passometer already computed one.
        WillIPassometer passometer = FindFirstObjectByType<WillIPassometer>();
        if (passometer != null && passometer.CurrentValue >= 0f)
        {
            HandlePassometerChanged(passometer.CurrentValue, passometer.CurrentState);
        }
    }

    private void HandlePassometerChanged(float value, PassometerState state)
    {
        Color stateColor = state == PassometerState.OnTrack ? onTrackColor
                         : state == PassometerState.AtRisk ? atRiskColor
                         : criticalColor;

        if (fillRect != null)
        {
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
            Image barImage = fillRect.GetComponent<Image>();
            if (barImage != null) barImage.color = stateColor;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(value);
            fillImage.color = stateColor;
        }

        if (valueLabel != null)
        {
            valueLabel.text = $"Will-I-Passometer  {Mathf.RoundToInt(value * 100f)}%  —  {StateText(state)}";
            valueLabel.color = stateColor;
        }
    }

    private void HandleSupportUnlocked()
    {
        if (valueLabel != null)
        {
            valueLabel.text += "\n<size=70%>Student support options unlocked</size>";
        }
    }

    private static string StateText(PassometerState state)
    {
        switch (state)
        {
            case PassometerState.OnTrack: return "On track";
            case PassometerState.AtRisk: return "At risk";
            default: return "Critical";
        }
    }

    private void BuildRuntimeUi()
    {
        Canvas canvas = RuntimeUiFactory.FindOrCreateHudCanvas();

        RectTransform panel = RuntimeUiFactory.CreatePanel(canvas.transform, "Passometer (auto)",
            anchorMin: new Vector2(1f, 1f), anchorMax: new Vector2(1f, 1f), pivot: new Vector2(1f, 1f),
            anchoredPosition: new Vector2(-16f, -16f), size: new Vector2(320f, 64f),
            backgroundColor: new Color(0f, 0f, 0f, 0.55f));

        // Label (top part of the panel)
        GameObject labelGo = new GameObject("PassometerLabel");
        labelGo.transform.SetParent(panel, false);
        RectTransform labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.45f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(12f, 0f);
        labelRect.offsetMax = new Vector2(-12f, -4f);
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.fontSize = 16f;
        label.alignment = TextAlignmentOptions.Left;
        label.text = "Will-I-Passometer";
        valueLabel = label;

        // Bar background (bottom part of the panel)
        GameObject barGo = new GameObject("BarBackground");
        barGo.transform.SetParent(panel, false);
        RectTransform barRect = barGo.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0.45f);
        barRect.offsetMin = new Vector2(12f, 10f);
        barRect.offsetMax = new Vector2(-12f, -4f);
        Image barBg = barGo.AddComponent<Image>();
        barBg.color = new Color(1f, 1f, 1f, 0.15f);

        // Fill (anchor-driven so no sprite is required)
        fillRect = RuntimeUiFactory.CreateStretchedChild(barRect, "Fill",
            atRiskColor, Vector2.zero, new Vector2(0.5f, 1f));
    }
}
