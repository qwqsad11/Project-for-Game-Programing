using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileCreatePanel : MonoBehaviour
{
    [Header("Panel Styling")]
    [SerializeField] private Color panelBackgroundColor = new Color(0.10f, 0.12f, 0.08f, 0.92f);
    [SerializeField] private Color inputFieldColor = new Color(0.25f, 0.28f, 0.22f, 0.95f);
    [SerializeField] private Color confirmColor = new Color(0.35f, 0.65f, 0.30f, 0.95f);
    [SerializeField] private Color cancelColor = new Color(0.60f, 0.35f, 0.30f, 0.95f);

    public System.Action<string> OnConfirm;
    public System.Action OnCancel;

    private TMP_InputField inputField;
    private TMP_Text errorText;
    private GameObject panelRoot;
    private GameObject centerPanel;
    private bool isShown;

    private void Awake()
    {
        BuildPanel();
    }

    private void Update()
    {
        if (isShown && Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancel?.Invoke();
        }

        // Auto-focus the input field
        if (isShown && inputField != null && !inputField.isFocused)
        {
            inputField.ActivateInputField();
        }
    }

    private void BuildPanel()
    {
        UIHelper.EnsureEventSystem();

        // Ensure our own GameObject has a RectTransform for proper UI nesting
        RectTransform selfRect = GetComponent<RectTransform>();
        if (selfRect == null)
        {
            selfRect = gameObject.AddComponent<RectTransform>();
        }
        selfRect.anchorMin = Vector2.zero;
        selfRect.anchorMax = Vector2.one;
        selfRect.offsetMin = Vector2.zero;
        selfRect.offsetMax = Vector2.zero;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            transform.SetParent(canvas.transform, false);
        }

        // Full-screen overlay with centered card
        panelRoot = UIHelper.CreateOverlayPanel(transform, "CreateProfile",
            new Vector2(420f, 280f), out centerPanel, () => OnCancel?.Invoke());

        // Override panel background
        Image panelBg = centerPanel.GetComponent<Image>();
        if (panelBg != null) panelBg.color = panelBackgroundColor;

        isShown = true;

        // ── Title ──
        UIHelper.CreateStyledText(centerPanel.transform, "Title", "Create Profile",
            new Vector2(0f, 95f), new Vector2(380f, 46f), 30f,
            TextAlignmentOptions.Center, Color.white, bold: true);

        // ── Input field container with border ──
        GameObject inputContainer = new GameObject("InputContainer");
        inputContainer.transform.SetParent(centerPanel.transform, false);

        RectTransform containerRect = inputContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(328f, 56f);
        containerRect.anchoredPosition = new Vector2(0f, 28f);

        // Border background (slightly larger, darker)
        Image borderImg = inputContainer.AddComponent<Image>();
        borderImg.sprite = UIHelper.GetRoundedRectSprite();
        borderImg.type = Image.Type.Sliced;
        borderImg.color = new Color(0.40f, 0.44f, 0.36f, 0.70f);

        // Input field object
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(inputContainer.transform, false);

        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = Vector2.one;
        inputRect.offsetMin = new Vector2(3f, 3f);
        inputRect.offsetMax = new Vector2(-3f, -3f);

        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.sprite = UIHelper.GetRoundedRectSprite();
        inputBg.type = Image.Type.Sliced;
        inputBg.color = inputFieldColor;

        // Text Area
        GameObject textAreaObj = new GameObject("Text Area");
        textAreaObj.transform.SetParent(inputObj.transform, false);

        RectTransform textAreaRect = textAreaObj.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(12f, 6f);
        textAreaRect.offsetMax = new Vector2(-12f, -6f);

        // Placeholder
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textAreaObj.transform, false);

        RectTransform phRect = placeholderObj.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;

        TMP_Text placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        UIHelper.AssignDefaultFont(placeholder);
        placeholder.text = "Enter nickname (3-16 chars)";
        placeholder.fontSize = 19;
        placeholder.alignment = TextAlignmentOptions.Center;
        placeholder.color = new Color(0.55f, 0.55f, 0.55f, 0.65f);

        // Input text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textAreaObj.transform, false);

        RectTransform textR = textObj.AddComponent<RectTransform>();
        textR.anchorMin = Vector2.zero;
        textR.anchorMax = Vector2.one;
        textR.offsetMin = Vector2.zero;
        textR.offsetMax = Vector2.zero;

        TMP_Text inputText = textObj.AddComponent<TextMeshProUGUI>();
        UIHelper.AssignDefaultFont(inputText);
        inputText.fontSize = 22;
        inputText.alignment = TextAlignmentOptions.Center;
        inputText.color = Color.white;

        inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.textViewport = textAreaRect;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.characterLimit = 16;
        inputField.onValueChanged.AddListener(OnInputChanged);

        // ── Error text ──
        GameObject errorObj = new GameObject("ErrorText");
        errorObj.transform.SetParent(centerPanel.transform, false);

        RectTransform errorRect = errorObj.AddComponent<RectTransform>();
        errorRect.anchorMin = new Vector2(0.5f, 0.5f);
        errorRect.anchorMax = new Vector2(0.5f, 0.5f);
        errorRect.sizeDelta = new Vector2(360f, 28f);
        errorRect.anchoredPosition = new Vector2(0f, -20f);

        errorText = errorObj.AddComponent<TextMeshProUGUI>();
        UIHelper.AssignDefaultFont(errorText);
        errorText.fontSize = 15;
        errorText.alignment = TextAlignmentOptions.Center;
        errorText.color = UIColorPalette.TextError;
        errorText.gameObject.SetActive(false);

        // ── Confirm button (primary) ──
        UIHelper.CreateStyledButton(centerPanel.transform, "ConfirmButton", "Confirm",
            new Vector2(-105f, -85f), confirmColor,
            () => OnConfirm?.Invoke(inputField?.text ?? ""),
            UIHelper.ButtonRole.Primary);

        // ── Cancel button (secondary) ──
        UIHelper.CreateStyledButton(centerPanel.transform, "CancelButton", "Cancel",
            new Vector2(105f, -85f), cancelColor,
            () => OnCancel?.Invoke(),
            UIHelper.ButtonRole.Danger);

        // ── Pop-in animation ──
        StartCoroutine(UIHelper.PopIn(centerPanel.transform, 0.25f));
    }

    private void OnInputChanged(string value)
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = $"⚠  {message}";
            errorText.gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        isShown = false;
        Destroy(gameObject);
    }
}
