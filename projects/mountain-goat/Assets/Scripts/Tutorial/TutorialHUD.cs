using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tutorial UI overlay — programmatically created canvas with instruction panel,
/// key hints display, and skip button.
/// </summary>
public class TutorialHUD : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private float panelHeight = 0.28f;
    [SerializeField] private Color panelColor = new Color(0.08f, 0.08f, 0.10f, 0.88f);

    [Header("Text")]
    [SerializeField] private float titleFontSize = 22f;
    [SerializeField] private float messageFontSize = 17f;
    [SerializeField] private float keyFontSize = 30f;
    [SerializeField] private Color textColor = new Color(1f, 0.95f, 0.7f, 1f);
    [SerializeField] private Color keyHighlightColor = new Color(1f, 0.85f, 0.2f, 1f);

    private Canvas canvas;
    private RectTransform panelRect;
    private TMP_Text titleText;
    private TMP_Text messageText;
    private TMP_Text keyHintsText;
    private Button skipButton;

    // Key display elements for highlighting
    private TMP_Text keyQ, keyE, keyA, keyD;

    private void Awake()
    {
        CreateCanvas();
        CreatePanel();
        CreateTitle();
        CreateMessage();
        CreateKeyDisplay();
        CreateSkipButton();
    }

    // ── Canvas ────────────────────────────────────────────

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("TutorialHUDCanvas");
        canvasObj.transform.SetParent(transform, false);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    // ── Bottom Panel ──────────────────────────────────────

    private void CreatePanel()
    {
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvas.transform, false);

        panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, panelHeight);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = panelColor;
    }

    // ── Title ─────────────────────────────────────────────

    private void CreateTitle()
    {
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelRect, false);

        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.72f);
        rect.anchorMax = new Vector2(1f, 0.98f);
        rect.offsetMin = new Vector2(30f, 0f);
        rect.offsetMax = new Vector2(-30f, 0f);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = titleFontSize;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.text = "";
    }

    // ── Message ───────────────────────────────────────────

    private void CreateMessage()
    {
        GameObject msgObj = new GameObject("MessageText");
        msgObj.transform.SetParent(panelRect, false);

        RectTransform rect = msgObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.40f);
        rect.anchorMax = new Vector2(1f, 0.74f);
        rect.offsetMin = new Vector2(40f, 0f);
        rect.offsetMax = new Vector2(-40f, 0f);

        messageText = msgObj.AddComponent<TextMeshProUGUI>();
        messageText.fontSize = messageFontSize;
        messageText.color = Color.white;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.text = "";
    }

    // ── Key Display (Q E A D layout) ──────────────────────

    private void CreateKeyDisplay()
    {
        // Container for key labels — placed below message
        GameObject keysObj = new GameObject("KeyHints");
        keysObj.transform.SetParent(panelRect, false);

        RectTransform keysRect = keysObj.AddComponent<RectTransform>();
        keysRect.anchorMin = new Vector2(0f, 0.04f);
        keysRect.anchorMax = new Vector2(1f, 0.42f);
        keysRect.offsetMin = Vector2.zero;
        keysRect.offsetMax = Vector2.zero;

        // Create 4 key labels in a diamond layout using absolute positioning within the container
        //   Q     E
        //   A     D
        float centerX = 0.5f;
        float centerY = 0.5f;
        float spacingX = 0.12f;
        float spacingY = 0.28f;

        keyQ = CreateKeyLabel(keysRect, "Q", "左前", centerX - spacingX, centerY + spacingY);
        keyE = CreateKeyLabel(keysRect, "E", "右前", centerX + spacingX, centerY + spacingY);
        keyA = CreateKeyLabel(keysRect, "A", "左后", centerX - spacingX, centerY - spacingY);
        keyD = CreateKeyLabel(keysRect, "D", "右后", centerX + spacingX, centerY - spacingY);
    }

    private TMP_Text CreateKeyLabel(RectTransform parent, string keyName, string direction, float anchorX, float anchorY)
    {
        GameObject keyObj = new GameObject($"Key_{keyName}");
        keyObj.transform.SetParent(parent, false);

        RectTransform rect = keyObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorX, anchorY);
        rect.anchorMax = new Vector2(anchorX, anchorY);
        rect.sizeDelta = new Vector2(160f, 70f);
        rect.anchoredPosition = Vector2.zero;

        TMP_Text text = keyObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = keyFontSize;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.text = $"[{keyName}]\n<size=60%>{direction}</size>";

        return text;
    }

    // ── Skip Button ───────────────────────────────────────

    private void CreateSkipButton()
    {
        GameObject btnObj = new GameObject("SkipButton");
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-80f, -30f);
        rect.sizeDelta = new Vector2(120f, 40f);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

        skipButton = btnObj.AddComponent<Button>();
        skipButton.onClick.AddListener(OnSkipClicked);

        GameObject txtObj = new GameObject("SkipText");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TMP_Text txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = "Skip >>";
        txt.fontSize = 16f;
        txt.color = new Color(0.75f, 0.75f, 0.75f, 1f);
        txt.alignment = TextAlignmentOptions.Center;
    }

    private void OnSkipClicked()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.SkipTutorial();
        }
    }

    // ── Public API ────────────────────────────────────────

    public void ShowMessage(string title, string message)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;
    }

    public void HighlightKeys(bool q, bool e, bool a, bool d)
    {
        SetKeyHighlight(keyQ, q);
        SetKeyHighlight(keyE, e);
        SetKeyHighlight(keyA, a);
        SetKeyHighlight(keyD, d);
    }

    public void ShowAllKeys()
    {
        HighlightKeys(true, true, true, true);
    }

    public void ClearKeyHighlights()
    {
        HighlightKeys(false, false, false, false);
    }

    private void SetKeyHighlight(TMP_Text keyText, bool highlight)
    {
        if (keyText == null) return;
        keyText.color = highlight ? keyHighlightColor : textColor;
        keyText.transform.localScale = highlight ? Vector3.one * 1.12f : Vector3.one;
    }

    public void SetSkipVisible(bool visible)
    {
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(visible);
        }
    }
}
