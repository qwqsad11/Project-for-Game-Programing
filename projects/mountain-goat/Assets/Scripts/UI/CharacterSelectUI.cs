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

    private void Awake()
    {
        EnsureEventSystem();
        EnsurePreviewCamera();
        BuildUI();
        BuildPreviews();
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

        CreateLabel(canvas.transform, "Choose Character", new Vector2(0f, 210f), 42f);

        for (int i = 0; i < options.Length; i++)
        {
            int row = i / 3;
            int col = i % 3;
            Vector2 position = new Vector2((col - 1) * 210f, 62f - row * 155f);
            CreateButton(canvas.transform, options[i].Label + "Button", options[i].Label, position, options[i].Character);
        }

        CreateBackButton(canvas.transform);
    }

    private void BuildPreviews()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].PreviewPrefab == null)
            {
                continue;
            }

            GameObject preview = Instantiate(options[i].PreviewPrefab);
            preview.name = options[i].Label + " Preview";
            preview.transform.position = options[i].PreviewPosition;
            preview.transform.rotation = Quaternion.Euler(options[i].PreviewEulerAngles);
            preview.transform.localScale = options[i].PreviewScale;
            StripPreviewBehaviours(preview);
        }
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

    private void CreateLabel(Transform parent, string textValue, Vector2 anchoredPosition, float fontSize)
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
    }

    private void CreateButton(Transform parent, string buttonName, string label, Vector2 anchoredPosition, PlayerCharacterSelection.Character character)
    {
        GameObject buttonObject = new GameObject(buttonName);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition + new Vector2(0f, -58f);
        rect.sizeDelta = new Vector2(180f, 48f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.94f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() => SelectCharacter(character));

        CreateButtonText(buttonObject.transform, label);
    }

    private void CreateBackButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("BackButton");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -215f);
        rect.sizeDelta = new Vector2(180f, 50f);

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

    private void SelectCharacter(PlayerCharacterSelection.Character character)
    {
        PlayerCharacterSelection.SaveSelection(character);
        GameManager.Instance.StartGameplay();
    }
}
