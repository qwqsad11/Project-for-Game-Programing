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

        CreateLabel(canvas.transform, "Choose Animal", new Vector2(0f, 215f), 42f);
        CreatePreviewClickArea(canvas.transform);
        CreateArrowButton(canvas.transform, "PreviousButton", "<", new Vector2(-255f, 0f), -1);
        CreateArrowButton(canvas.transform, "NextButton", ">", new Vector2(255f, 0f), 1);
        characterNameText = CreateLabel(canvas.transform, string.Empty, new Vector2(0f, -128f), 34f);
        CreateConfirmButton(canvas.transform);

        CreateBackButton(canvas.transform);
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

    private TextMeshProUGUI CreateLabel(Transform parent, string textValue, Vector2 anchoredPosition, float fontSize)
    {
        GameObject labelObject = new GameObject("Title");
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 80f);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.14f, 0.16f, 0.12f, 1f);
        return text;
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

    private void CreateArrowButton(Transform parent, string buttonName, string label, Vector2 anchoredPosition, int direction)
    {
        GameObject buttonObject = new GameObject(buttonName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(64f, 64f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.94f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() => CycleCharacter(direction));

        CreateButtonText(buttonObject.transform, label);
    }

    private void CreateConfirmButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("ConfirmButton");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -190f);
        rect.sizeDelta = new Vector2(220f, 54f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.95f, 0.98f, 0.88f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(ConfirmSelection);

        CreateButtonText(buttonObject.transform, "Select");
    }

    private void CreateBackButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("BackButton");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -252f);
        rect.sizeDelta = new Vector2(160f, 44f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.86f, 0.88f, 0.84f, 0.94f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());

        CreateButtonText(buttonObject.transform, "Back");
    }

    private void CreateButtonText(Transform parent, string label)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.16f, 0.18f, 0.14f, 1f);
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
