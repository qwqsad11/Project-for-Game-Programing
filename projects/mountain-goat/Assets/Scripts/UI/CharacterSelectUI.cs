using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [System.Serializable]
    private class CharacterOption
    {
        [SerializeField] private PlayerCharacterSelection.Character character;
        [SerializeField] private string label;
        [SerializeField] private GameObject previewPrefab;
        [SerializeField] private Vector3 previewPosition;
        [SerializeField] private Vector3 previewEulerAngles = new Vector3(0f, 160f, 0f);
        [SerializeField] private Vector3 previewScale = Vector3.one;

        public PlayerCharacterSelection.Character Character => character;
        public string Label => label;
        public GameObject PreviewPrefab => previewPrefab;
        public Vector3 PreviewPosition => previewPosition;
        public Vector3 PreviewEulerAngles => previewEulerAngles;
        public Vector3 PreviewScale => previewScale;
    }

    [SerializeField] private CharacterOption[] options;
    [SerializeField] private Vector3 previewAnchor = new Vector3(0f, -0.55f, 0f);

    private static readonly string[] PreviewIdleStateNames =
    {
        "idle",
        "DIdle 1",
        "DIdle Look",
        "DIdle Scratch",
        "DIdle Head Shake"
    };

    private GameObject activePreview;
    private TextMeshProUGUI characterNameText;
    private int currentIndex;

    private void Awake()
    {
        EnsureEventSystem();
        EnsurePreviewCamera();
        BuildUI();
        SelectSavedCharacterIndex();
        ShowCurrentOption();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void BuildUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
        }

        // Title with drop shadow
        CreateTitleLabel(canvas.transform, "\U0001F410  Choose Animal", new Vector2(0f, 220f), 44f);
        CreatePreviewClickArea(canvas.transform);

        // Arrow buttons — styled icon buttons
        CreateArrowButton(canvas.transform, "PreviousButton", "◀", new Vector2(-260f, 0f), -1);
        CreateArrowButton(canvas.transform, "NextButton", "▶", new Vector2(260f, 0f), 1);

        // Character name display
        characterNameText = CreateStyledLabel(canvas.transform, string.Empty, new Vector2(0f, -128f), 36f,
            Color.white, bold: true);

        // Confirm button — primary
        UIHelper.CreateStyledButton(canvas.transform, "ConfirmButton", "Select",
            new Vector2(0f, -195f), UIColorPalette.BtnConfirm,
            ConfirmSelection, UIHelper.ButtonRole.Primary);

        // Back button — secondary
        UIHelper.CreateStyledButton(canvas.transform, "BackButton", "Back",
            new Vector2(0f, -258f), UIColorPalette.BtnBack,
            () => GameManager.Instance.ReturnToMenu(), UIHelper.ButtonRole.Secondary);
    }

    private void SelectSavedCharacterIndex()
    {
        if (options == null || options.Length == 0)
        {
            currentIndex = 0;
            return;
        }

        PlayerCharacterSelection.Character savedCharacter = PlayerCharacterSelection.GetSavedCharacter();
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].Character == savedCharacter)
            {
                currentIndex = i;
                return;
            }
        }
    }

    private void ShowCurrentOption()
    {
        if (options == null || options.Length == 0)
        {
            if (characterNameText != null)
            {
                characterNameText.text = "No Goat";
            }

            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, options.Length - 1);
        CharacterOption option = options[currentIndex];

        if (activePreview != null)
        {
            Destroy(activePreview);
        }

        if (option.PreviewPrefab != null)
        {
            activePreview = Instantiate(option.PreviewPrefab);
            activePreview.name = option.Label + " Preview";
            activePreview.transform.position = previewAnchor + option.PreviewPosition;
            activePreview.transform.rotation = Quaternion.Euler(option.PreviewEulerAngles);
            activePreview.transform.localScale = option.PreviewScale;
            StripPreviewBehaviours(activePreview);
            PlayPreviewIdle(activePreview);
        }

        if (characterNameText != null)
        {
            characterNameText.text = option.Label;
        }
    }

    private void CycleCharacter(int direction)
    {
        if (options == null || options.Length == 0)
        {
            return;
        }

        currentIndex = (currentIndex + direction + options.Length) % options.Length;
        ShowCurrentOption();
    }

    private void StripPreviewBehaviours(GameObject preview)
    {
        Collider[] colliders = preview.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        MonoBehaviour[] behaviours = preview.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
            {
                behaviours[i].enabled = false;
            }
        }

        Animator animator = preview.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    private void PlayPreviewIdle(GameObject preview)
    {
        Animator animator = preview.GetComponentInChildren<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        for (int i = 0; i < PreviewIdleStateNames.Length; i++)
        {
            int stateHash = Animator.StringToHash(PreviewIdleStateNames[i]);
            if (animator.HasState(0, stateHash))
            {
                animator.Play(stateHash, 0, 0f);
                animator.Update(0f);
                return;
            }
        }
    }

    private void EnsurePreviewCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        GameObject cameraObject = new GameObject("CharacterSelectCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.66f, 0.78f, 0.72f, 1f);
        cameraObject.transform.position = new Vector3(0f, 2.2f, -8f);
        cameraObject.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
    }

    // ── UI Creation Helpers ──

    private void CreateTitleLabel(Transform parent, string textValue, Vector2 anchoredPosition, float fontSize)
    {
        UIHelper.CreateStyledText(parent, "Title", textValue,
            anchoredPosition, new Vector2(560f, 70f), fontSize,
            TextAlignmentOptions.Center, UIColorPalette.TextDark,
            bold: true, shadow: true);
    }

    private TextMeshProUGUI CreateStyledLabel(Transform parent, string textValue, Vector2 anchoredPosition,
        float fontSize, Color color, bool bold = false)
    {
        return UIHelper.CreateStyledText(parent, "Label", textValue,
            anchoredPosition, new Vector2(520f, 60f), fontSize,
            TextAlignmentOptions.Center, color, bold: bold);
    }

    private void CreatePreviewClickArea(Transform parent)
    {
        GameObject buttonObject = new GameObject("PreviewClickArea");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 18f);
        rect.sizeDelta = new Vector2(420f, 260f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() => CycleCharacter(1));
    }

    private void CreateArrowButton(Transform parent, string buttonName, string label,
        Vector2 anchoredPosition, int direction)
    {
        UIHelper.CreateStyledButton(parent, buttonName, label,
            anchoredPosition, UIColorPalette.BtnArrow,
            () => CycleCharacter(direction), UIHelper.ButtonRole.Icon);

        // Override the button text color to dark since arrow buttons are light
        Transform btnTransform = parent.Find(buttonName);
        if (btnTransform != null)
        {
            Transform textTransform = btnTransform.Find("Text");
            if (textTransform != null)
            {
                TextMeshProUGUI tmp = textTransform.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = UIColorPalette.TextDark;
                    tmp.fontSize = 32f;
                }
            }
        }
    }

    private void ConfirmSelection()
    {
        if (options != null && options.Length > 0)
        {
            PlayerCharacterSelection.SaveSelection(options[currentIndex].Character);
        }

        GameManager.Instance.StartGameplay();
    }
}
