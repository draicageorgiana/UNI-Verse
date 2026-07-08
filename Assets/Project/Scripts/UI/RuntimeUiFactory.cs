using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Small factory for building fallback runtime UI.
///
/// The presentation components (DialogueUI, PassometerDisplay, CreditSystem)
/// accept hand-wired scene references in the Inspector; when none are assigned
/// they construct a functional default layout through this factory so a chapter
/// scene runs with zero editor wiring. Designers can replace any of it later —
/// components stay isolated and communicate only via GameEvents (§3.2, §3.6).
/// </summary>
public static class RuntimeUiFactory
{
    public const string HudCanvasName = "HUD Canvas (auto)";

    public static Canvas FindOrCreateHudCanvas()
    {
        GameObject existing = GameObject.Find(HudCanvasName);
        if (existing != null)
        {
            Canvas found = existing.GetComponent<Canvas>();
            if (found != null) return found;
        }

        Canvas sceneCanvas = Object.FindFirstObjectByType<Canvas>();
        if (sceneCanvas != null && sceneCanvas.renderMode != RenderMode.WorldSpace)
        {
            return sceneCanvas;
        }

        GameObject go = new GameObject(HudCanvasName);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        go.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();
        return canvas;
    }

    /// <summary>UI buttons need an EventSystem; create one if the scene has none.</summary>
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem (auto)");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();

        // This project runs with activeInputHandler = Input System Package (new),
        // so the legacy StandaloneInputModule would disable itself. Pick the
        // module that matches the active input backend at compile time.
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
    }

    public static RectTransform CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 size, Color backgroundColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = go.AddComponent<Image>();
        image.color = backgroundColor;

        return rect;
    }

    /// <summary>Creates a stretched child rect (used for fill bars and containers).</summary>
    public static RectTransform CreateStretchedChild(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        if (color.a > 0f)
        {
            Image image = go.AddComponent<Image>();
            image.color = color;
        }

        return rect;
    }

    public static TMP_Text CreateText(Transform parent, string name, string content,
        float fontSize, TextAlignmentOptions alignment, Vector2 padding)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding.x, padding.y);
        rect.offsetMax = new Vector2(-padding.x, -padding.y);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;

        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label,
        Color backgroundColor, float height)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, height);

        Image image = go.AddComponent<Image>();
        image.color = backgroundColor;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.colors = colors;

        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;

        CreateText(go.transform, "Label", label, 20f, TextAlignmentOptions.Center, new Vector2(12f, 4f));

        return button;
    }
}
