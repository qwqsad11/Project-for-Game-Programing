using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileSelectUI : MonoBehaviour
{
    [Header("Panel Colors")]
    [SerializeField] private Color panelBackgroundColor = new Color(0.12f, 0.14f, 0.1f, 0.94f);
    [SerializeField] private Color slotEmptyColor = new Color(0.30f, 0.32f, 0.28f, 0.80f);
    [SerializeField] private Color slotOccupiedColor = new Color(0.25f, 0.35f, 0.22f, 0.90f);
    [SerializeField] private Color slotActiveColor = new Color(0.30f, 0.50f, 0.22f, 0.95f);
    [SerializeField] private Color deleteButtonColor = new Color(0.72f, 0.25f, 0.20f, 0.90f);
    [SerializeField] private Color playButtonColor = new Color(0.30f, 0.60f, 0.25f, 0.95f);
    [SerializeField] private Color backButtonColor = new Color(0.55f, 0.50f, 0.40f, 0.90f);

    private GameObject rootPanel;
    private GameObject centerPanel;
    private List<GameObject> slotCards = new List<GameObject>();
    private bool isShown;

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        BuildUI();
        RefreshAllSlots();
    }

    private void Update()
    {
        if (isShown && Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
        }
    }

    public void Show()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(true);
            if (centerPanel != null)
                StartCoroutine(UIHelper.PopIn(centerPanel.transform, 0.28f));
        }
        isShown = true;
        RefreshAllSlots();
    }

    public void Hide()
    {
        isShown = false;
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

    public void Close()
    {
        isShown = false;
        Destroy(gameObject);
    }

    private void BuildUI()
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

        // Use the overlay panel helper for the dim background + center card
        rootPanel = UIHelper.CreateOverlayPanel(transform, "ProfileSelect",
            new Vector2(720f, 440f), out centerPanel, OnBackClicked);

        // Override panel background color with serialized value
        Image panelBg = centerPanel.GetComponent<Image>();
        if (panelBg != null) panelBg.color = panelBackgroundColor;

        // ── Title ──
        UIHelper.CreateStyledText(centerPanel.transform, "Title", "Select Profile",
            new Vector2(0f, 185f), new Vector2(400f, 50f), 34f,
            TextAlignmentOptions.Center, Color.white, bold: true);

        // ── Profile slots: 3 cards side by side ──
        float cardWidth = 190f;
        float cardSpacing = 210f;
        float startX = -cardSpacing;

        for (int i = 0; i < 3; i++)
        {
            GameObject card = CreateSlotCard(centerPanel.transform, i,
                new Vector2(startX + i * cardSpacing, 20f),
                new Vector2(cardWidth, 260f));
            slotCards.Add(card);
        }

        // ── Play button (primary) ──
        UIHelper.CreateStyledButton(centerPanel.transform, "PlayButton", "▶  Play",
            new Vector2(-110f, -155f), playButtonColor,
            OnPlayClicked, UIHelper.ButtonRole.Primary);

        // ── Back button (secondary) ──
        UIHelper.CreateStyledButton(centerPanel.transform, "BackButton", "Back",
            new Vector2(110f, -155f), backButtonColor,
            OnBackClicked, UIHelper.ButtonRole.Secondary);

        // ── Pop-in animation ──
        Show();
    }

    private GameObject CreateSlotCard(Transform parent, int slotIndex, Vector2 position, Vector2 size)
    {
        GameObject card = new GameObject($"SlotCard_{slotIndex}");
        card.transform.SetParent(parent, false);

        RectTransform rect = card.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        // Rounded card background
        Image bg = card.AddComponent<Image>();
        bg.sprite = UIHelper.GetRoundedRectSprite();
        bg.type = Image.Type.Sliced;
        bg.color = slotEmptyColor;

        Button cardButton = card.AddComponent<Button>();
        int index = slotIndex;
        cardButton.onClick.AddListener(() => OnSlotClicked(index));

        // Add transition colors
        UIHelper.SetupButtonTransitions(cardButton, bg.color, addShadow: false);

        return card;
    }

    private void RefreshAllSlots()
    {
        if (ProfileManager.Instance == null)
            return;

        List<PlayerProfile> profiles = ProfileManager.Instance.GetAllProfilesWithEmptySlots();
        PlayerProfile activeProfile = ProfileManager.Instance.GetActiveProfile();

        for (int i = 0; i < slotCards.Count; i++)
        {
            RefreshSlot(slotCards[i], i, i < profiles.Count ? profiles[i] : null, activeProfile);
        }
    }

    private void RefreshSlot(GameObject card, int slotIndex, PlayerProfile profile, PlayerProfile activeProfile)
    {
        // Clear existing children (except the Image and Button components)
        for (int i = card.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(card.transform.GetChild(i).gameObject);
        }

        Image bg = card.GetComponent<Image>();
        Button cardButton = card.GetComponent<Button>();
        bool isProfile = profile != null;
        bool isActive = isProfile && activeProfile != null && profile.profileId == activeProfile.profileId;

        Color baseColor;
        if (isActive)       baseColor = slotActiveColor;
        else if (isProfile) baseColor = slotOccupiedColor;
        else                baseColor = slotEmptyColor;

        bg.color = baseColor;
        UIHelper.SetupButtonTransitions(cardButton, baseColor, addShadow: false);

        if (!isProfile)
        {
            // Empty slot
            CreateSlotText(card.transform, slotIndex, "Empty Slot", new Vector2(0f, 30f), 22f,
                new Color(0.6f, 0.6f, 0.6f, 0.8f));
            CreateSlotText(card.transform, slotIndex, "(tap to create)", new Vector2(0f, -10f), 15f,
                new Color(0.5f, 0.5f, 0.5f, 0.7f));
            return;
        }

        // Character emoji
        string charIcon = GetCharacterIcon(profile.selectedCharacter);

        // Profile name
        CreateSlotText(card.transform, slotIndex, $"{charIcon}  {profile.profileName}",
            new Vector2(0f, 90f), 20f, Color.white);

        // High score
        CreateSlotText(card.transform, slotIndex, $"\U0001F3C6  Best: {profile.highScore:N0}",
            new Vector2(0f, 50f), 15f, new Color(0.9f, 0.85f, 0.3f, 0.95f));

        // Total coins
        CreateSlotText(card.transform, slotIndex, $"\U0001F4B0  Coins: {profile.totalCoins:N0}",
            new Vector2(0f, 25f), 14f, new Color(0.8f, 0.8f, 0.75f, 0.9f));

        // Total plays
        CreateSlotText(card.transform, slotIndex, $"Plays: {profile.totalPlays}",
            new Vector2(0f, 0f), 13f, new Color(0.7f, 0.7f, 0.65f, 0.85f));

        // Last played
        CreateSlotText(card.transform, slotIndex, profile.GetLastPlayedText(),
            new Vector2(0f, -20f), 12f, new Color(0.6f, 0.6f, 0.55f, 0.8f));

        // Active indicator
        if (isActive)
        {
            CreateSlotText(card.transform, slotIndex, "▼  ACTIVE  ▼",
                new Vector2(0f, 115f), 13f, new Color(0.3f, 1f, 0.3f, 0.9f));
        }

        // Delete button (only if more than 1 profile exists)
        if (ProfileManager.Instance.ProfileCount > 1)
        {
            CreateSlotDeleteButton(card.transform, slotIndex, "DeleteBtn", "✕",
                new Vector2(68f, -68f), new Vector2(40f, 32f),
                deleteButtonColor, () => OnDeleteClicked(slotIndex, profile.profileId));
        }
    }

    private void CreateSlotText(Transform parent, int slotIndex, string content,
        Vector2 anchoredPosition, float fontSize, Color color)
    {
        GameObject go = new GameObject($"Text_{slotIndex}_{content.GetHashCode()}");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(175f, 28f);
        rect.anchoredPosition = anchoredPosition;

        TMP_Text text = go.AddComponent<TextMeshProUGUI>();
        UIHelper.AssignDefaultFont(text);
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
    }

    private void CreateSlotDeleteButton(Transform parent, int slotIndex, string name, string label,
        Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = btnObj.AddComponent<Image>();
        image.sprite = UIHelper.GetRoundedRectSprite();
        image.type = Image.Type.Sliced;
        image.color = color;

        Button button = btnObj.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        UIHelper.SetupButtonTransitions(button, color, addShadow: false);

        GameObject labelObj = new GameObject("Text");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text tmp = labelObj.AddComponent<TextMeshProUGUI>();
        UIHelper.AssignDefaultFont(tmp);
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    private void OnSlotClicked(int slotIndex)
    {
        List<PlayerProfile> profiles = ProfileManager.Instance.GetAllProfiles();

        if (slotIndex < profiles.Count)
        {
            ProfileManager.Instance.SetActiveProfile(profiles[slotIndex].profileId);
            RefreshAllSlots();
        }
        else
        {
            ShowCreatePanel();
        }
    }

    private void OnDeleteClicked(int slotIndex, string profileId)
    {
        bool deleted = ProfileManager.Instance.DeleteProfile(profileId);
        if (deleted)
        {
            RefreshAllSlots();
        }
    }

    private void OnPlayClicked()
    {
        if (ProfileManager.Instance == null || !ProfileManager.Instance.HasActiveProfile)
        {
            if (ProfileManager.Instance.NeedsProfileCreation)
            {
                PlayerProfile created = ProfileManager.Instance.CreateProfile("Player");
                if (created == null)
                {
                    Debug.LogWarning("[ProfileSelectUI] Failed to create default profile.");
                    return;
                }
            }
            else
            {
                List<PlayerProfile> profiles = ProfileManager.Instance.GetAllProfiles();
                if (profiles.Count > 0)
                {
                    ProfileManager.Instance.SetActiveProfile(profiles[0].profileId);
                }
                else
                {
                    return;
                }
            }
        }

        Close();
        GameManager.Instance.StartGame();
    }

    private void OnBackClicked()
    {
        if (ProfileManager.Instance != null && ProfileManager.Instance.NeedsProfileCreation)
        {
            ProfileManager.Instance.CreateProfile("Player");
        }

        Close();
    }

    private void ShowCreatePanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject go = new GameObject("ProfileCreatePanel");
        go.transform.SetParent(canvas != null ? canvas.transform : transform, false);

        ProfileCreatePanel createPanel = go.AddComponent<ProfileCreatePanel>();
        createPanel.OnConfirm = (name) =>
        {
            string trimmed = name?.Trim() ?? "";
            if (trimmed.Length < 3)
            {
                createPanel.ShowError("Name must be at least 3 characters.");
                return;
            }
            if (trimmed.Length > 16)
            {
                createPanel.ShowError("Name must be at most 16 characters.");
                return;
            }

            List<PlayerProfile> existing = ProfileManager.Instance.GetAllProfiles();
            for (int i = 0; i < existing.Count; i++)
            {
                if (existing[i].profileName.Equals(trimmed, System.StringComparison.OrdinalIgnoreCase))
                {
                    createPanel.ShowError("Name already in use.");
                    return;
                }
            }

            PlayerProfile created = ProfileManager.Instance.CreateProfile(trimmed);
            if (created != null)
            {
                createPanel.Close();
                RefreshAllSlots();
            }
            else
            {
                createPanel.ShowError("Max 3 profiles allowed.");
            }
        };

        createPanel.OnCancel = () =>
        {
            createPanel.Close();
        };
    }

    private static string GetCharacterIcon(int characterId)
    {
        switch (characterId)
        {
            case 0: return "\U0001F410";  // Goat
            case 1: return "\U0001F410";  // Goat (dark)
            case 2: return "\U0001F411";  // Sheep (white)
            case 3: return "\U0001F411";  // Sheep (cream)
            case 4: return "\U0001F411";  // Sheep (dark)
            case 5: return "\U0001F98C";  // Fawn
            case 6: return "\U0001F98C";  // Deer
            case 7: return "\U0001F98C";  // Deer (female)
            case 8: return "\U0001F98C";  // Elk
            case 9: return "\U0001F98C";  // Elk (albino)
            default: return "\U0001F410"; // Goat default
        }
    }
}
