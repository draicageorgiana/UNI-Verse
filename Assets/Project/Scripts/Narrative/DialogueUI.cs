using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The passive presentation layer for the narrative engine (§3.2): it
/// intercepts text strings and choice sets broadcast by the StoryManager over
/// the static event channels and visually renders them. It never reads story
/// state directly and never references the StoryManager (§3.6).
///
/// Implements the Dialogue Selection use case (§3.5): Ink choices become
/// buttons; the picked index is raised back over OnChoiceSelected.
///
/// Dialogue pacing (§4.4.3): lines print character by character and the
/// progression button is held inactive until the printed length matches the
/// source buffer, preventing accidental text skipping. When a TextCreator
/// component sits on the text object the existing pipeline is reused;
/// otherwise an internal coroutine applies the same pacing technique.
///
/// Wire the references in the Inspector, or leave them empty and the component
/// builds a functional dialogue layout at runtime (zero-wiring chapter scenes).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Optional UI (auto-built if left empty)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerLabel;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;
    [SerializeField] private RectTransform choicesContainer;
    [SerializeField] private Button choiceButtonTemplate;
    [SerializeField] private bool autoBuildUi = true;

    [Header("Pacing (§4.4.3)")]
    [SerializeField] private float charDelay = 0.03f;

    private readonly List<Button> spawnedChoiceButtons = new List<Button>();
    private Coroutine printRoutine;
    private bool uiReady;

    void OnEnable()
    {
        GameEvents.OnDialogueLine += HandleDialogueLine;
        GameEvents.OnChoicesPresented += HandleChoicesPresented;
        GameEvents.OnStoryEnded += HandleStoryEnded;
    }

    void OnDisable()
    {
        GameEvents.OnDialogueLine -= HandleDialogueLine;
        GameEvents.OnChoicesPresented -= HandleChoicesPresented;
        GameEvents.OnStoryEnded -= HandleStoryEnded;
    }

    void Awake()
    {
        EnsureUi();
    }

    private void EnsureUi()
    {
        if (uiReady) return;

        if (bodyText == null && autoBuildUi)
        {
            BuildRuntimeUi();
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        uiReady = bodyText != null;

        if (!uiReady)
        {
            Debug.LogWarning("DialogueUI: no body text reference and auto-build is disabled — dialogue cannot render.");
        }
    }

    // ---- Event handlers (§3.6 subscribe in OnEnable / unsubscribe in OnDisable) ----

    private void HandleDialogueLine(string speaker, string text)
    {
        EnsureUi();
        if (!uiReady) return;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        ClearChoices();
        HideContinueButton();

        if (speakerLabel != null)
        {
            speakerLabel.text = speaker;
            speakerLabel.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }

        if (printRoutine != null)
        {
            StopCoroutine(printRoutine);
        }

        TextCreator textCreator = bodyText.GetComponent<TextCreator>();
        if (textCreator != null)
        {
            // Reuse the existing §4.4.3 TextCreator pipeline: hand it the full
            // buffer and gate progression on the printed character count.
            bodyText.text = text;
            TextCreator.runTextPrint = true;
            printRoutine = StartCoroutine(WaitForTextCreator(text.Length));
        }
        else
        {
            printRoutine = StartCoroutine(PrintLine(text));
        }
    }

    private void HandleChoicesPresented(List<string> choices)
    {
        EnsureUi();
        if (!uiReady || choices == null || choices.Count == 0) return;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        HideContinueButton();
        ClearChoices();

        for (int i = 0; i < choices.Count; i++)
        {
            int choiceIndex = i; // capture per-iteration copy for the closure
            Button button = CreateChoiceButton(choices[i]);
            if (button == null) continue;

            button.onClick.AddListener(() =>
            {
                ClearChoices();
                GameEvents.RaiseChoiceSelected(choiceIndex);
            });

            spawnedChoiceButtons.Add(button);
        }
    }

    private void HandleStoryEnded()
    {
        ClearChoices();
        HideContinueButton();

        if (speakerLabel != null)
        {
            speakerLabel.gameObject.SetActive(false);
        }
    }

    // ---- Typewriter pacing (§4.4.3) ------------------------------------------------

    private IEnumerator WaitForTextCreator(int targetLength)
    {
        // Give TextCreator one Update cycle to capture and clear the buffer.
        yield return new WaitForSeconds(0.1f);

        float timeout = targetLength * charDelay + 5f;
        float elapsed = 0f;

        while (bodyText.text.Length < targetLength && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        ShowContinueButton();
    }

    private IEnumerator PrintLine(string text)
    {
        bodyText.text = "";
        WaitForSeconds pace = new WaitForSeconds(charDelay);

        foreach (char c in text)
        {
            bodyText.text += c;
            yield return pace;
        }

        ShowContinueButton();
    }

    private void ShowContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void HideContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    private void OnContinueClicked()
    {
        GameEvents.RaiseContinueRequested();
    }

    // ---- Choice buttons ---------------------------------------------------------------

    private Button CreateChoiceButton(string label)
    {
        if (choicesContainer == null) return null;

        if (choiceButtonTemplate != null)
        {
            Button instance = Instantiate(choiceButtonTemplate, choicesContainer);
            instance.gameObject.SetActive(true);

            TMP_Text labelText = instance.GetComponentInChildren<TMP_Text>();
            if (labelText != null) labelText.text = label;

            return instance;
        }

        return RuntimeUiFactory.CreateButton(choicesContainer, "Choice", label,
            new Color(0.16f, 0.22f, 0.34f, 0.95f), 46f);
    }

    private void ClearChoices()
    {
        foreach (Button button in spawnedChoiceButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        spawnedChoiceButtons.Clear();
    }

    // ---- Fallback runtime layout ---------------------------------------------------------

    private void BuildRuntimeUi()
    {
        Canvas canvas = RuntimeUiFactory.FindOrCreateHudCanvas();

        // Dialogue panel (bottom text canvas, §4.2 layout)
        RectTransform panel = RuntimeUiFactory.CreatePanel(canvas.transform, "Dialogue Panel (auto)",
            anchorMin: new Vector2(0.05f, 0f), anchorMax: new Vector2(0.95f, 0f), pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(0f, 24f), size: new Vector2(0f, 230f),
            backgroundColor: new Color(0.05f, 0.06f, 0.10f, 0.85f));
        dialoguePanel = panel.gameObject;

        // Character name container flag
        GameObject speakerGo = new GameObject("SpeakerLabel");
        speakerGo.transform.SetParent(panel, false);
        RectTransform speakerRect = speakerGo.AddComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0f, 1f);
        speakerRect.anchorMax = new Vector2(1f, 1f);
        speakerRect.pivot = new Vector2(0.5f, 1f);
        speakerRect.anchoredPosition = new Vector2(0f, -10f);
        speakerRect.sizeDelta = new Vector2(-48f, 34f);
        TextMeshProUGUI speakerTmp = speakerGo.AddComponent<TextMeshProUGUI>();
        speakerTmp.fontSize = 24f;
        speakerTmp.fontStyle = FontStyles.Bold;
        speakerTmp.alignment = TextAlignmentOptions.Left;
        speakerTmp.color = new Color(1f, 0.85f, 0.55f);
        speakerLabel = speakerTmp;

        // Organized text canvas (body)
        GameObject bodyGo = new GameObject("BodyText");
        bodyGo.transform.SetParent(panel, false);
        RectTransform bodyRect = bodyGo.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(24f, 64f);
        bodyRect.offsetMax = new Vector2(-24f, -50f);
        TextMeshProUGUI bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
        bodyTmp.fontSize = 24f;
        bodyTmp.alignment = TextAlignmentOptions.TopLeft;
        bodyTmp.color = Color.white;
        bodyText = bodyTmp;

        // Progression button (held inactive during printing, §4.4.3)
        GameObject buttonGo = new GameObject("ContinueButton");
        buttonGo.transform.SetParent(panel, false);
        RectTransform buttonRect = buttonGo.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-24f, 12f);
        buttonRect.sizeDelta = new Vector2(180f, 42f);
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.16f, 0.22f, 0.34f, 0.95f);
        continueButton = buttonGo.AddComponent<Button>();
        continueButton.targetGraphic = buttonImage;
        RuntimeUiFactory.CreateText(buttonGo.transform, "Label", "Continue  ▸", 20f,
            TextAlignmentOptions.Center, new Vector2(8f, 4f));

        // Choice container (vertical stack above the dialogue panel)
        RectTransform choicesPanel = RuntimeUiFactory.CreatePanel(canvas.transform, "Choices (auto)",
            anchorMin: new Vector2(0.5f, 0f), anchorMax: new Vector2(0.5f, 0f), pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(0f, 270f), size: new Vector2(640f, 0f),
            backgroundColor: new Color(0f, 0f, 0f, 0f));
        VerticalLayoutGroup layout = choicesPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = choicesPanel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        choicesContainer = choicesPanel;
    }
}
