using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Static utility for creating styled UI elements and animations.
/// Reduces duplication across UI scripts and ensures visual consistency.
/// </summary>
public static class UIHelper
{
    // ── Cached rounded-rect sprite (white, 9-sliced) ──
    private static Sprite _cachedRoundedRect;
    private const int SpriteSize = 64;
    private const int SpriteRadius = 12;

    // ── Cached materials ──
    private static Material _cachedOutlineMat;
    private static Material _cachedShadowMat;

    // ── Cached TMP font asset ──
    private static TMP_FontAsset _cachedFontAsset;
    private static bool _fontLookupAttempted;

    /// <summary>
    /// Returns the manaspc SDF font asset from Resources.
    /// Cached after first load. Returns null if not found.
    /// </summary>
    public static TMP_FontAsset GetDefaultFontAsset()
    {
        if (_cachedFontAsset == null && !_fontLookupAttempted)
        {
            _fontLookupAttempted = true;

            // Priority 1: Built-in LiberationSans (always works, guaranteed glyph coverage)
            _cachedFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (_cachedFontAsset != null)
            {
                Debug.Log("[UIHelper] Using font: LiberationSans SDF (built-in)");
            }

            // Priority 2: User's custom manaspc font (overrides if LiberationSans unavailable)
            if (_cachedFontAsset == null)
            {
                _cachedFontAsset = Resources.Load<TMP_FontAsset>("Fonts/manaspc SDF");
                if (_cachedFontAsset != null)
                {
                    Debug.Log("[UIHelper] Using font: manaspc SDF (custom)");
                }
            }

            // Priority 3: TMP Settings default
            if (_cachedFontAsset == null)
            {
                _cachedFontAsset = TMP_Settings.defaultFontAsset;
                if (_cachedFontAsset != null)
                {
                    Debug.Log($"[UIHelper] Using TMP Settings default font: {_cachedFontAsset.name}");
                }
                else
                {
                    Debug.LogError("[UIHelper] CRITICAL: No TMP font asset found! All UI text will be invisible.");
                }
            }
        }
        return _cachedFontAsset;
    }

    // ── EventSystem ─────────────────────────────────

    /// <summary>
    /// Ensure an EventSystem exists in the scene.
    /// Required for ScrollRect, Button, TMP_InputField, and other UI interactivity.
    /// </summary>
    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
    }

    /// <summary>
    /// Safely assign the default font to a TMP_Text component.
    /// Does nothing if the font asset is unavailable (logs a warning).
    /// </summary>
    public static void AssignDefaultFont(TMP_Text textComponent)
    {
        if (textComponent == null) return;
        TMP_FontAsset font = GetDefaultFontAsset();
        if (font != null)
        {
            textComponent.font = font;
        }
        else
        {
            Debug.LogWarning($"[UIHelper] Cannot assign font to '{textComponent.name}' — no font asset available.");
        }
    }

    // ── Sprite Generation ───────────────────────────

    /// <summary>
    /// Returns a white 64x64 rounded-rect sprite with 9-slice borders set to the corner radius.
    /// Cached after first creation. Use Image.Type.Sliced for best results.
    /// </summary>
    public static Sprite GetRoundedRectSprite()
    {
        if (_cachedRoundedRect != null)
            return _cachedRoundedRect;

        int size = SpriteSize;
        int radius = SpriteRadius;
        float radiusSq = radius * radius;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = 0f, dy = 0f;

                // Determine distance from the nearest corner
                if (x < radius)
                    dx = radius - x - 0.5f;
                else if (x >= size - radius)
                    dx = x - (size - radius) + 0.5f;

                if (y < radius)
                    dy = radius - y - 0.5f;
                else if (y >= size - radius)
                    dy = y - (size - radius) + 0.5f;

                // Fill pixel if inside rounded rectangle
                if (dx <= 0f && dy <= 0f)
                {
                    pixels[y * size + x] = Color.white;
                }
                else if (dx > 0f || dy > 0f)
                {
                    // In a corner region — check circle
                    if (dx * dx + dy * dy <= radiusSq)
                        pixels[y * size + x] = Color.white;
                    else
                        pixels[y * size + x] = Color.clear;
                }
                else
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Vector4 border = new Vector4(radius, radius, radius, radius);
        _cachedRoundedRect = Sprite.Create(tex,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0u,
            SpriteMeshType.FullRect,
            border);

        _cachedRoundedRect.name = "RoundedRect_9Slice";
        return _cachedRoundedRect;
    }

    // ── Material Helpers ──────────────────────────

    /// <summary>Load the LiberationSans Outline material from Resources.</summary>
    public static Material GetOutlineMaterial()
    {
        if (_cachedOutlineMat == null)
        {
            _cachedOutlineMat = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
        }
        return _cachedOutlineMat;
    }

    /// <summary>Load the LiberationSans Drop Shadow material from Resources.</summary>
    public static Material GetShadowMaterial()
    {
        if (_cachedShadowMat == null)
        {
            _cachedShadowMat = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Drop Shadow");
        }
        return _cachedShadowMat;
    }

    // ── Styled Button Creation ────────────────────

    /// <summary>Button visual weight for sizing and prominence.</summary>
    public enum ButtonRole
    {
        Primary,   // Large, prominent (Play, Confirm)
        Secondary, // Medium (Back, Cancel, Tutorial)
        Danger,    // Medium-small (Delete, Quit)
        Icon       // Small square (arrows, close)
    }

    /// <summary>Get the recommended size for a button role.</summary>
    public static Vector2 GetButtonSize(ButtonRole role)
    {
        switch (role)
        {
            case ButtonRole.Primary:   return new Vector2(280f, 62f);
            case ButtonRole.Secondary: return new Vector2(220f, 52f);
            case ButtonRole.Danger:    return new Vector2(180f, 46f);
            case ButtonRole.Icon:      return new Vector2(56f, 56f);
            default:                   return new Vector2(240f, 54f);
        }
    }

    /// <summary>Get the recommended font size for a button role.</summary>
    public static float GetButtonFontSize(ButtonRole role)
    {
        switch (role)
        {
            case ButtonRole.Primary:   return 28f;
            case ButtonRole.Secondary: return 24f;
            case ButtonRole.Danger:    return 22f;
            case ButtonRole.Icon:      return 30f;
            default:                   return 26f;
        }
    }

    /// <summary>
    /// Create a fully styled button with rounded corners, hover/press transitions,
    /// and a TMP text label.
    /// </summary>
    public static GameObject CreateStyledButton(
        Transform parent, string name, string label,
        Vector2 anchoredPosition, Color buttonColor,
        UnityEngine.Events.UnityAction onClick,
        ButtonRole role = ButtonRole.Secondary)
    {
        Vector2 size = GetButtonSize(role);
        float fontSize = GetButtonFontSize(role);
        Sprite sprite = GetRoundedRectSprite();

        // ── Button root ──
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        // ── Image (9-sliced rounded rect) ──
        Image image = btnObj.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = buttonColor;
        image.pixelsPerUnitMultiplier = 1f;

        // ── Shadow effect (child image slightly offset and darker) ──
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(btnObj.transform, false);
        shadowObj.transform.SetAsFirstSibling(); // behind the button content

        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(3f, -3f);
        shadowRect.offsetMax = new Vector2(3f, -3f);

        Image shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = sprite;
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.25f);
        shadowImg.raycastTarget = false;
        shadowImg.pixelsPerUnitMultiplier = 1f;

        // ── Button component with transitions ──
        Button button = btnObj.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = UIColorPalette.HoverVariant(buttonColor);
        colors.pressedColor = UIColorPalette.PressVariant(buttonColor);
        colors.selectedColor = buttonColor;
        colors.disabledColor = new Color(buttonColor.r * 0.5f, buttonColor.g * 0.5f, buttonColor.b * 0.5f, 0.5f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        // ── Text label ──
        GameObject labelObj = new GameObject("Text");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        AssignDefaultFont(tmp);
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        return btnObj;
    }

    /// <summary>
    /// Create a TMP text with optional outline or shadow material.
    /// </summary>
    public static TextMeshProUGUI CreateStyledText(
        Transform parent, string name, string content,
        Vector2 anchoredPosition, Vector2 size,
        float fontSize, TextAlignmentOptions alignment, Color color,
        bool bold = false, bool outline = false, bool shadow = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;

        // Assign the TMP font asset — text won't render without this
        AssignDefaultFont(tmp);

        if (bold) tmp.fontStyle |= FontStyles.Bold;
        if (outline && GetOutlineMaterial() != null)
            tmp.fontMaterial = GetOutlineMaterial();
        if (shadow && GetShadowMaterial() != null)
            tmp.fontMaterial = GetShadowMaterial();

        return tmp;
    }

    /// <summary>
    /// Create a full-screen dim overlay panel with a centered card.
    /// Returns the center panel transform for further customization.
    /// </summary>
    public static GameObject CreateOverlayPanel(
        Transform parent, string name,
        Vector2 panelSize,
        out GameObject centerPanel,
        UnityEngine.Events.UnityAction onDismiss = null)
    {
        // Full-screen root
        GameObject root = new GameObject(name + "Root");
        root.transform.SetParent(parent, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Dim background — click to close
        Image rootBg = root.AddComponent<Image>();
        rootBg.color = UIColorPalette.OverlayDim;
        rootBg.raycastTarget = true;

        Button dismissBtn = root.AddComponent<Button>();
        if (onDismiss != null)
            dismissBtn.onClick.AddListener(onDismiss);
        ColorBlock cb = dismissBtn.colors;
        cb.normalColor = Color.clear;
        cb.highlightedColor = Color.clear;
        cb.pressedColor = Color.clear;
        cb.selectedColor = Color.clear;
        dismissBtn.colors = cb;
        dismissBtn.transition = Selectable.Transition.None;

        // Center card panel
        centerPanel = new GameObject("Panel");
        centerPanel.transform.SetParent(root.transform, false);

        RectTransform panelRect = centerPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = centerPanel.AddComponent<Image>();
        panelBg.sprite = GetRoundedRectSprite();
        panelBg.type = Image.Type.Sliced;
        panelBg.color = UIColorPalette.PanelBg;

        return root;
    }

    // ── Animation Coroutines ──────────────────────

    /// <summary>Scale a transform from 0 to 1 with an elastic ease-out.</summary>
    public static IEnumerator PopIn(Transform target, float duration = 0.30f)
    {
        target.localScale = Vector3.zero;
        yield return null; // wait one frame so scale=0 is applied

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out back: slight overshoot
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float val = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            target.localScale = Vector3.one * Mathf.Clamp(val, 0f, 1.15f);
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    /// <summary>Scale a transform from 1 to 0 with acceleration.</summary>
    public static IEnumerator PopOut(Transform target, float duration = 0.15f)
    {
        target.localScale = Vector3.one;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-in
            target.localScale = Vector3.one * (1f - t * t);
            yield return null;
        }

        target.localScale = Vector3.zero;
    }

    /// <summary>Pulse a transform (scale up and back) for attention.</summary>
    public static IEnumerator Pulse(Transform target, float peakScale = 1.15f, float duration = 0.50f)
    {
        Vector3 baseScale = target.localScale;

        // Scale up
        float half = duration * 0.5f;
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            target.localScale = Vector3.Lerp(baseScale, baseScale * peakScale, t);
            yield return null;
        }

        // Scale back
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            target.localScale = Vector3.Lerp(baseScale * peakScale, baseScale, t);
            yield return null;
        }

        target.localScale = baseScale;
    }

    /// <summary>Repeatedly pulse for emphasis (e.g. "New Record!" text).</summary>
    public static IEnumerator PulseLoop(Transform target, float peakScale = 1.10f, float interval = 0.8f)
    {
        while (target != null && target.gameObject.activeInHierarchy)
        {
            yield return Pulse(target, peakScale, interval);
            yield return new WaitForSecondsRealtime(interval * 0.5f);
        }
    }

    // ── Button Color Transition Setup ─────────────

    /// <summary>
    /// Apply ColorTint transition to an existing Button.
    /// Call this if you already have a Button and just want proper hover/press colors.
    /// </summary>
    public static void SetupButtonTransitions(Button button, Color baseColor, bool addShadow = true)
    {
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = UIColorPalette.HoverVariant(baseColor);
        colors.pressedColor = UIColorPalette.PressVariant(baseColor);
        colors.selectedColor = baseColor;
        colors.disabledColor = new Color(baseColor.r * 0.5f, baseColor.g * 0.5f, baseColor.b * 0.5f, 0.5f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        // Optionally add a subtle shadow under the button
        if (addShadow && button.targetGraphic is Image img && img.sprite != null)
        {
            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(button.transform, false);
            shadow.transform.SetAsFirstSibling();

            RectTransform shadowRect = shadow.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(3f, -3f);
            shadowRect.offsetMax = new Vector2(3f, -3f);

            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.sprite = img.sprite;
            shadowImg.type = img.type;
            shadowImg.color = new Color(0f, 0f, 0f, 0.25f);
            shadowImg.raycastTarget = false;
        }
    }
}
