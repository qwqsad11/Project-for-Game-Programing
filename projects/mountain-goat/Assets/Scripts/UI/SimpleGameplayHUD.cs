using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameplayHUD : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text hungerText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text profileNameText;

    [Header("Coin Pop Animation")]
    [SerializeField] private float coinPopScale = 1.25f;
    [SerializeField] private float coinPopDuration = 0.16f;

    [Header("Hunger Bar (created at runtime if null)")]
    [SerializeField] private Image hungerBarFill;
    [SerializeField] private Image hungerBarBackground;
    [SerializeField] private float hungerBarWidth = 180f;
    [SerializeField] private float hungerBarHeight = 14f;

    private int lastSessionCoins = -1;
    private Coroutine coinPopRoutine;
    private Vector3 coinBaseScale = Vector3.one;
    private float displayedScore;
    private Coroutine scoreRollRoutine;
    private bool hungerBarCreated;

    private void OnEnable()
    {
        if (coinsText != null)
        {
            coinBaseScale = coinsText.transform.localScale;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnHungerChanged += HandleHungerChanged;
            GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
        }

        EnsureHungerBar();
        Refresh();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnHungerChanged -= HandleHungerChanged;
            GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
        }
    }

    private void HandleScoreChanged(int score)
    {
        // Animate score counting up
        if (scoreRollRoutine != null)
            StopCoroutine(scoreRollRoutine);
        scoreRollRoutine = StartCoroutine(ScoreRollRoutine(score));
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        Refresh();
    }

    private void HandleHungerChanged(float currentHunger, float maxHunger)
    {
        RefreshHungerDisplay(currentHunger, maxHunger);
    }

    private void HandleCoinsChanged(int sessionCoins, int totalCoins)
    {
        bool shouldPop = lastSessionCoins >= 0 && sessionCoins > lastSessionCoins;
        lastSessionCoins = sessionCoins;
        Refresh();

        if (shouldPop)
        {
            PlayCoinPop();
        }
    }

    // ── Hunger Bar (created programmatically) ──────

    private void EnsureHungerBar()
    {
        if (hungerBarCreated) return;
        if (hungerBarFill != null && hungerBarBackground != null) return;

        // Find or create a parent container for the hunger bar
        Transform parent = hungerText != null ? hungerText.transform.parent : transform;

        // Create a container for the bar + text
        GameObject barContainer = new GameObject("HungerBarContainer");
        barContainer.transform.SetParent(parent, false);
        barContainer.transform.SetAsFirstSibling();

        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        // Position it near the hunger text
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(hungerBarWidth, hungerBarHeight + 4f);

        // Try to match hunger text position
        if (hungerText != null)
        {
            RectTransform hungerRect = hungerText.rectTransform;
            containerRect.anchoredPosition = hungerRect.anchoredPosition + new Vector2(0f, 22f);
        }
        else
        {
            containerRect.anchoredPosition = new Vector2(0f, 40f);
        }

        // Background track
        GameObject bgObj = new GameObject("BarBackground");
        bgObj.transform.SetParent(barContainer.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = UIHelper.GetRoundedRectSprite();
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.1f, 0.1f, 0.08f, 0.85f);
        hungerBarBackground = bgImg;

        // Fill bar
        GameObject fillObj = new GameObject("BarFill");
        fillObj.transform.SetParent(barContainer.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(hungerBarWidth, hungerBarHeight);

        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.sprite = UIHelper.GetRoundedRectSprite();
        fillImg.type = Image.Type.Sliced;
        fillImg.color = UIColorPalette.HungerGreen;
        hungerBarFill = fillImg;

        hungerBarCreated = true;
    }

    // ── Refresh ────────────────────────────────────

    private void Refresh()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (scoreText != null)
        {
            scoreText.text = $"<size=60%>SCORE</size>\n{GameManager.Instance.Score:N0}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"\U0001F3C6  {GameManager.Instance.HighScore:N0}";
        }

        if (hungerText != null)
        {
            float maxHunger = GameManager.Instance.MaxHunger;
            float currentHunger = GameManager.Instance.CurrentHunger;
            RefreshHungerDisplay(currentHunger, maxHunger);
        }

        if (coinsText != null)
        {
            coinsText.text = $"\U0001F4B0  {GameManager.Instance.SessionCoins}";
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = $"\U0001F4B0  Total: {GameManager.Instance.TotalCoins:N0}";
        }

        if (profileNameText != null)
        {
            profileNameText.text = $"\U0001F464  {GameManager.Instance.ActiveProfileName}";
        }
    }

    private void RefreshHungerDisplay(float currentHunger, float maxHunger)
    {
        float ratio = maxHunger <= 0f ? 0f : Mathf.Clamp01(currentHunger / maxHunger);
        int percent = Mathf.RoundToInt(ratio * 100f);

        if (hungerText != null)
        {
            // Text-based bar using block characters
            int blocks = Mathf.RoundToInt(ratio * 10f);
            string bar = new string('█', blocks) + new string('░', 10 - blocks);

            // Color tag based on hunger level
            string colorTag;
            if (ratio > 0.6f)      colorTag = "<color=#4DD94D>";
            else if (ratio > 0.3f) colorTag = "<color=#F2CC0F>";
            else                   colorTag = "<color=#F2400F>";

            hungerText.text = $"{colorTag}Hunger: {bar}</color>  {percent}%";
        }

        // Update the fill bar
        if (hungerBarFill != null)
        {
            float barWidth = hungerBarWidth * ratio;
            hungerBarFill.rectTransform.sizeDelta = new Vector2(barWidth, hungerBarHeight);

            // Color transition: green → yellow → red
            Color barColor;
            if (ratio > 0.6f)
                barColor = Color.Lerp(UIColorPalette.HungerYellow, UIColorPalette.HungerGreen, (ratio - 0.6f) / 0.4f);
            else if (ratio > 0.3f)
                barColor = Color.Lerp(UIColorPalette.HungerRed, UIColorPalette.HungerYellow, (ratio - 0.3f) / 0.3f);
            else
                barColor = UIColorPalette.HungerRed;

            hungerBarFill.color = barColor;
        }
    }

    // ── Animations ─────────────────────────────────

    private void PlayCoinPop()
    {
        if (coinsText == null)
        {
            return;
        }

        if (coinPopRoutine != null)
        {
            StopCoroutine(coinPopRoutine);
        }

        coinPopRoutine = StartCoroutine(CoinPopRoutine());
    }

    private System.Collections.IEnumerator CoinPopRoutine()
    {
        Transform coinTransform = coinsText.transform;
        float halfDuration = Mathf.Max(0.01f, coinPopDuration * 0.5f);
        Vector3 peakScale = coinBaseScale * coinPopScale;

        for (float timer = 0f; timer < halfDuration; timer += Time.unscaledDeltaTime)
        {
            coinTransform.localScale = Vector3.Lerp(coinBaseScale, peakScale, timer / halfDuration);
            yield return null;
        }

        for (float timer = 0f; timer < halfDuration; timer += Time.unscaledDeltaTime)
        {
            coinTransform.localScale = Vector3.Lerp(peakScale, coinBaseScale, timer / halfDuration);
            yield return null;
        }

        coinTransform.localScale = coinBaseScale;
        coinPopRoutine = null;
    }

    private System.Collections.IEnumerator ScoreRollRoutine(int targetScore)
    {
        float startScore = displayedScore;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-out
            t = 1f - (1f - t) * (1f - t);
            displayedScore = Mathf.Lerp(startScore, targetScore, t);

            if (scoreText != null)
            {
                scoreText.text = $"<size=60%>SCORE</size>\n{Mathf.RoundToInt(displayedScore):N0}";
            }

            yield return null;
        }

        displayedScore = targetScore;
        if (scoreText != null)
        {
            scoreText.text = $"<size=60%>SCORE</size>\n{targetScore:N0}";
        }

        scoreRollRoutine = null;
    }
}
