using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SimpleMenuUI : MonoBehaviour
{
    private bool _initialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        RefreshProfileNameDisplay();
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        EnsureEventSystem();
        EnsureTutorialButton();
        EnsureProfileButtons();
        BindButtons();
        _initialized = true;
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

    private void EnsureTutorialButton()
    {
        // Check if a Tutorial button already exists in the scene
        Button[] allButtons = GetMenuButtons();
        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] != null && allButtons[i].gameObject.name.Contains("Tutorial"))
            {
                return; // Already exists
            }
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        if (canvas == null) return;

        // Create the Tutorial button with emoji icon
        UIHelper.CreateStyledButton(canvas.transform, "TutorialButton", "\U0001F4D6  Tutorial",
            new Vector2(0f, -210f), UIColorPalette.BtnTutorial,
            StartTutorial, UIHelper.ButtonRole.Secondary);
    }

    private void EnsureProfileButtons()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        if (canvas == null) return;

        // Check if already created
        Button[] allButtons = GetMenuButtons();
        bool hasProfiles = false;
        bool hasLeaderboard = false;
        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] != null)
            {
                if (allButtons[i].gameObject.name.Contains("Profiles")) hasProfiles = true;
                if (allButtons[i].gameObject.name.Contains("Leaderboard")) hasLeaderboard = true;
            }
        }

        if (!hasProfiles)
        {
            UIHelper.CreateStyledButton(canvas.transform, "ProfilesButton", "\U0001F464  Profiles",
                new Vector2(0f, -280f), UIColorPalette.BtnProfiles,
                OpenProfiles, UIHelper.ButtonRole.Secondary);
        }

        if (!hasLeaderboard)
        {
            UIHelper.CreateStyledButton(canvas.transform, "LeaderboardButton", "\U0001F3C6  Leaderboard",
                new Vector2(0f, -345f), UIColorPalette.BtnLeaderboard,
                OpenLeaderboard, UIHelper.ButtonRole.Secondary);
        }

        // Ensure profile name display
        EnsureProfileNameText(canvas.transform);
    }

    private void EnsureProfileNameText(Transform canvasTransform)
    {
        Transform existing = canvasTransform.Find("ProfileNameText");
        if (existing != null) return;

        GameObject textObj = new GameObject("ProfileNameText");
        textObj.transform.SetParent(canvasTransform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -35f);
        rect.sizeDelta = new Vector2(400f, 40f);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = UIColorPalette.TextSecondary;
        tmp.fontStyle = FontStyles.Bold;

        RefreshProfileNameText(tmp);
    }

    private void RefreshProfileNameText(TMP_Text tmp)
    {
        if (tmp == null) return;
        string name = "Player";
        if (ProfileManager.Instance != null && ProfileManager.Instance.HasActiveProfile)
        {
            name = ProfileManager.Instance.GetActiveProfile().profileName;
        }
        tmp.text = $"\U0001F3AE  Playing as: {name}";
    }

    private void RefreshProfileNameDisplay()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Transform textTransform = canvas.transform.Find("ProfileNameText");
        if (textTransform == null) return;

        TMP_Text tmp = textTransform.GetComponent<TMP_Text>();
        RefreshProfileNameText(tmp);
    }

    private void BindButtons()
    {
        Button[] buttons = GetMenuButtons();
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            string buttonName = button.gameObject.name;
            button.onClick.RemoveListener(StartGame);
            button.onClick.RemoveListener(ReturnToMenu);
            button.onClick.RemoveListener(RestartGame);
            button.onClick.RemoveListener(QuitGame);
            button.onClick.RemoveListener(ChooseCharacter);
            button.onClick.RemoveListener(StartTutorial);
            button.onClick.RemoveListener(OpenProfiles);
            button.onClick.RemoveListener(OpenLeaderboard);

            // Apply transition colors for any existing scene buttons (not created by UIHelper)
            Image img = button.GetComponent<Image>();
            if (img != null && button.transition == Selectable.Transition.ColorTint)
            {
                // Already configured by UIHelper — skip
            }
            else if (img != null)
            {
                // Scene-placed button: apply transitions based on name
                Color baseColor = img.color;
                if (buttonName.Contains("Quit"))
                    baseColor = UIColorPalette.BtnQuit;
                UIHelper.SetupButtonTransitions(button, baseColor);
            }

            if (buttonName.Contains("CharacterChoose"))
            {
                button.onClick.AddListener(ChooseCharacter);
            }
            else if (buttonName.Contains("Start"))
            {
                button.onClick.AddListener(StartGame);
            }
            else if (buttonName.Contains("Quit"))
            {
                button.onClick.AddListener(QuitGame);
            }
            else if (buttonName.Contains("Back"))
            {
                button.onClick.AddListener(ReturnToMenu);
            }
            else if (buttonName.Contains("Restart"))
            {
                button.onClick.AddListener(RestartGame);
            }
            else if (buttonName.Contains("Tutorial"))
            {
                button.onClick.AddListener(StartTutorial);
            }
            else if (buttonName.Contains("Profiles"))
            {
                button.onClick.AddListener(OpenProfiles);
            }
            else if (buttonName.Contains("Leaderboard"))
            {
                button.onClick.AddListener(OpenLeaderboard);
            }
        }
    }

    private Button[] GetMenuButtons()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            return canvas.GetComponentsInChildren<Button>(true);
        }

        return GetComponentsInChildren<Button>(true);
    }

    public void StartGame()
    {
        Debug.Log("SimpleMenuUI: StartGame clicked");
        GameManager.Instance.StartGame();
    }

    public void ChooseCharacter()
    {
        Debug.Log("SimpleMenuUI: CharacterChoose clicked");
        GameManager.Instance.ChooseCharacter();
    }

    public void ReturnToMenu()
    {
        Debug.Log("SimpleMenuUI: ReturnToMenu clicked");
        GameManager.Instance.ReturnToMenu();
    }

    public void RestartGame()
    {
        Debug.Log("SimpleMenuUI: RestartGame clicked");
        GameManager.Instance.RestartGame();
    }

    public void StartTutorial()
    {
        Debug.Log("SimpleMenuUI: Tutorial clicked");
        GameManager.Instance.StartTutorial();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("SimpleMenuUI: QuitGame clicked in Editor");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenProfiles()
    {
        Debug.Log("SimpleMenuUI: OpenProfiles clicked");

        ProfileSelectUI existing = FindObjectOfType<ProfileSelectUI>();
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject go = new GameObject("ProfileSelectUI");
        go.transform.SetParent(canvas != null ? canvas.transform : null, false);
        go.AddComponent<ProfileSelectUI>();
    }

    public void OpenLeaderboard()
    {
        Debug.Log("SimpleMenuUI: OpenLeaderboard clicked");

        LeaderboardUI existing = FindObjectOfType<LeaderboardUI>();
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject go = new GameObject("LeaderboardUI");
        go.transform.SetParent(canvas != null ? canvas.transform : null, false);
        go.AddComponent<LeaderboardUI>();
    }
}
