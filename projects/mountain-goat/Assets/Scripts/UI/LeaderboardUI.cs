using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel Styling")]
    [SerializeField] private Color panelBackgroundColor = new Color(0.12f, 0.14f, 0.1f, 0.94f);
    [SerializeField] private Color entryNormalColor = new Color(0.22f, 0.25f, 0.20f, 0.85f);
    [SerializeField] private Color entryHighlightColor = new Color(0.35f, 0.45f, 0.18f, 0.90f);
    [SerializeField] private Color entryDeletedColor = new Color(0.18f, 0.18f, 0.16f, 0.75f);
    [SerializeField] private Color topRankGold = new Color(0.95f, 0.80f, 0.10f, 1f);
    [SerializeField] private Color topRankSilver = new Color(0.80f, 0.80f, 0.80f, 1f);
    [SerializeField] private Color topRankBronze = new Color(0.80f, 0.55f, 0.25f, 1f);
    [SerializeField] private Color backButtonColor = new Color(0.55f, 0.50f, 0.40f, 0.90f);

    private GameObject rootPanel;
    private GameObject centerPanel;
    private RectTransform contentContainer;
    private ScrollRect scrollRect;
    private List<GameObject> entryObjects = new List<GameObject>();
    private TMP_Text totalEntriesText;
    private bool isShown;

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        BuildUI();
        RefreshEntries();
    }

    private void OnEnable()
    {
        RefreshEntries();
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnActiveProfileChanged += RefreshEntries;
        }
    }

    private void OnDisable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnActiveProfileChanged -= RefreshEntries;
        }
    }

    private void Update()
    {
        if (isShown && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
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
        RefreshEntries();
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
        rootPanel = UIHelper.CreateOverlayPanel(transform, "Leaderboard",
            new Vector2(620f, 500f), out centerPanel, () => Close());

        // Override panel background
        Image panelBg = centerPanel.GetComponent<Image>();
        if (panelBg != null) panelBg.color = panelBackgroundColor;

        // ── Title ──
        UIHelper.CreateStyledText(centerPanel.transform, "Title", "\U0001F3C6  Leaderboard",
            new Vector2(0f, 215f), new Vector2(400f, 50f), 34f,
            TextAlignmentOptions.Center, Color.white, bold: true);

        // ── Total entries text ──
        totalEntriesText = UIHelper.CreateStyledText(centerPanel.transform, "TotalEntries", "",
            new Vector2(0f, 178f), new Vector2(400f, 28f), 15f,
            TextAlignmentOptions.Center, new Color(0.65f, 0.65f, 0.65f, 0.75f));

        // ── Scroll View ──
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(centerPanel.transform, false);

        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollViewRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollViewRect.sizeDelta = new Vector2(570f, 320f);
        scrollViewRect.anchoredPosition = new Vector2(0f, -25f);

        Image scrollBg = scrollViewObj.AddComponent<Image>();
        scrollBg.sprite = UIHelper.GetRoundedRectSprite();
        scrollBg.type = Image.Type.Sliced;
        scrollBg.color = new Color(0.07f, 0.09f, 0.05f, 0.75f);

        scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        // ── Viewport ──
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);

        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(0f, 0f);
        viewportRect.offsetMax = new Vector2(-14f, 0f); // leave room for scrollbar

        Image viewportImg = viewportObj.AddComponent<Image>();
        viewportImg.color = new Color(0f, 0f, 0f, 0f);
        viewportImg.raycastTarget = true;

        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // ── Content ──
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        contentContainer = contentRect;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        // ── Scrollbar ──
        CreateScrollbar(scrollViewObj.transform, scrollViewRect, scrollRect);

        // ── Back button ──
        UIHelper.CreateStyledButton(centerPanel.transform, "BackButton", "Back",
            new Vector2(0f, -235f), backButtonColor,
            () => Close(), UIHelper.ButtonRole.Secondary);

        // ── Pop-in animation ──
        Show();
    }

    private void CreateScrollbar(Transform scrollViewTransform, RectTransform scrollViewRect, ScrollRect scroll)
    {
        // Scrollbar container
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollViewTransform, false);

        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 0.5f);
        scrollbarRect.sizeDelta = new Vector2(12f, -8f);
        scrollbarRect.anchoredPosition = new Vector2(2f, 0f);

        // Track background
        Image trackImg = scrollbarObj.AddComponent<Image>();
        trackImg.sprite = UIHelper.GetRoundedRectSprite();
        trackImg.type = Image.Type.Sliced;
        trackImg.color = UIColorPalette.ScrollbarTrack;
        trackImg.raycastTarget = true;

        // Sliding area
        GameObject slidingAreaObj = new GameObject("SlidingArea");
        slidingAreaObj.transform.SetParent(scrollbarObj.transform, false);

        RectTransform slidingAreaRect = slidingAreaObj.AddComponent<RectTransform>();
        slidingAreaRect.anchorMin = Vector2.zero;
        slidingAreaRect.anchorMax = Vector2.one;
        slidingAreaRect.offsetMin = new Vector2(1f, 1f);
        slidingAreaRect.offsetMax = new Vector2(-1f, -1f);

        // Handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(slidingAreaObj.transform, false);

        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0f);
        handleRect.anchorMax = new Vector2(1f, 0f);
        handleRect.pivot = new Vector2(0.5f, 0f);
        handleRect.sizeDelta = new Vector2(0f, 20f);

        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.sprite = UIHelper.GetRoundedRectSprite();
        handleImg.type = Image.Type.Sliced;
        handleImg.color = UIColorPalette.ScrollbarHandle;

        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImg;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.size = 0.15f;

        ColorBlock sbColors = scrollbar.colors;
        sbColors.normalColor = UIColorPalette.ScrollbarHandle;
        sbColors.highlightedColor = UIColorPalette.HoverVariant(UIColorPalette.ScrollbarHandle);
        sbColors.pressedColor = UIColorPalette.PressVariant(UIColorPalette.ScrollbarHandle);
        sbColors.selectedColor = UIColorPalette.ScrollbarHandle;
        sbColors.fadeDuration = 0.1f;
        scrollbar.colors = sbColors;

        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
    }

    private void RefreshEntries()
    {
        // Clear existing entries
        for (int i = entryObjects.Count - 1; i >= 0; i--)
        {
            if (entryObjects[i] != null)
                Destroy(entryObjects[i]);
        }
        entryObjects.Clear();

        ILeaderboardProvider leaderboard = GameManager.Instance?.Leaderboard;
        if (leaderboard == null)
        {
            totalEntriesText.text = "No leaderboard data available.";
            return;
        }

        List<LeaderboardEntry> entries = leaderboard.GetTopEntries(100);
        string activeProfileId = GameManager.Instance?.ActiveProfileId ?? "";

        if (entries.Count == 0)
        {
            totalEntriesText.text = "No entries yet. Play a game!";
            contentContainer.sizeDelta = new Vector2(555f, 0f);
            return;
        }

        totalEntriesText.text = $"Top {entries.Count} Players";

        float entryHeight = 44f;
        float spacing = 2f;
        float totalHeight = entries.Count * (entryHeight + spacing);
        contentContainer.sizeDelta = new Vector2(555f, totalHeight);

        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntry entry = entries[i];
            int rank = i + 1;
            bool isCurrentPlayer = !string.IsNullOrEmpty(activeProfileId) &&
                                    entry.profileId == activeProfileId;

            GameObject entryObj = CreateEntry(entry, rank, isCurrentPlayer,
                new Vector2(0f, -i * (entryHeight + spacing)));
            entryObjects.Add(entryObj);
        }
    }

    private GameObject CreateEntry(LeaderboardEntry entry, int rank, bool isCurrentPlayer,
        Vector2 position)
    {
        GameObject entryObj = new GameObject($"Entry_{rank}");
        entryObj.transform.SetParent(contentContainer, false);

        RectTransform rect = entryObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(550f, 44f);
        rect.anchoredPosition = position;

        Image bg = entryObj.AddComponent<Image>();
        bg.sprite = UIHelper.GetRoundedRectSprite();
        bg.type = Image.Type.Sliced;

        // Color the entry based on status
        if (isCurrentPlayer)
        {
            bg.color = entryHighlightColor;
        }
        else if (string.IsNullOrEmpty(entry.profileId))
        {
            bg.color = entryDeletedColor;
        }
        else
        {
            bg.color = entryNormalColor;
        }

        // Rank number with medal emojis for top 3
        Color rankColor = Color.white;
        string rankText;
        if (rank == 1)      { rankColor = topRankGold;   rankText = "\U0001F947  1st"; }
        else if (rank == 2) { rankColor = topRankSilver; rankText = "\U0001F948  2nd"; }
        else if (rank == 3) { rankColor = topRankBronze; rankText = "\U0001F949  3rd"; }
        else                { rankColor = new Color(0.6f, 0.6f, 0.6f, 0.9f); rankText = $"#{rank}"; }

        CreateEntryText(entryObj.transform, "RankText", rankText,
            new Vector2(-240f, 0f), new Vector2(90f, 38f), 18f,
            TextAlignmentOptions.Center, rankColor);

        // Character icon + player name
        string charIcon = GetCharacterIcon(entry.characterId);
        CreateEntryText(entryObj.transform, "NameText", $"{charIcon}  {entry.playerName}",
            new Vector2(-120f, 0f), new Vector2(220f, 38f), 20f,
            TextAlignmentOptions.Left, Color.white);

        // Score
        CreateEntryText(entryObj.transform, "ScoreText", entry.score.ToString("N0"),
            new Vector2(140f, 0f), new Vector2(120f, 38f), 22f,
            TextAlignmentOptions.Right,
            isCurrentPlayer ? new Color(1f, 0.9f, 0.2f, 1f) : new Color(0.9f, 0.85f, 0.3f, 0.95f));

        // Time ago
        string timeAgo = entry.GetTimeAgoText();
        if (!string.IsNullOrEmpty(timeAgo))
        {
            CreateEntryText(entryObj.transform, "TimeText", timeAgo,
                new Vector2(225f, 0f), new Vector2(80f, 38f), 12f,
                TextAlignmentOptions.Right, new Color(0.5f, 0.5f, 0.5f, 0.8f));
        }

        return entryObj;
    }

    private void CreateEntryText(Transform parent, string name, string content,
        Vector2 position, Vector2 size, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TMP_Text text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
    }

    private static string GetCharacterIcon(int characterId)
    {
        switch (characterId)
        {
            case 0: return "\U0001F410";  // Goat
            case 1: return "\U0001F410";  // Goat (dark)
            case 2: return "\U0001F411";  // Sheep
            case 3: return "\U0001F411";  // Sheep (cream)
            case 4: return "\U0001F411";  // Sheep (dark)
            case 5: return "\U0001F98C";  // Fawn
            case 6: return "\U0001F98C";  // Deer
            case 7: return "\U0001F98C";  // Deer (female)
            case 8: return "\U0001F98C";  // Elk
            case 9: return "\U0001F98C";  // Elk (albino)
            default: return "\U0001F410";
        }
    }
}
