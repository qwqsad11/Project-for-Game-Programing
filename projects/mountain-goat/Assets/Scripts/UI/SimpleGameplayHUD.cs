using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameplayHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text hungerText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private float coinPopScale = 1.25f;
    [SerializeField] private float coinPopDuration = 0.16f;

    private int lastSessionCoins = -1;
    private Coroutine coinPopRoutine;
    private Vector3 coinBaseScale = Vector3.one;

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
        Refresh();
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        Refresh();
    }

    private void HandleHungerChanged(float currentHunger, float maxHunger)
    {
        Refresh();
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

    private void Refresh()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {GameManager.Instance.Score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {GameManager.Instance.HighScore}";
        }

        if (hungerText != null)
        {
            int hungerPercent = Mathf.RoundToInt(GameManager.Instance.MaxHunger <= 0f
                ? 0f
                : (GameManager.Instance.CurrentHunger / GameManager.Instance.MaxHunger) * 100f);
            hungerText.text = $"Hunger: {hungerPercent}%";
        }

        if (coinsText != null)
        {
            coinsText.text = $"Coins: {GameManager.Instance.SessionCoins}";
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = $"Total Coins: {GameManager.Instance.TotalCoins}";
        }
    }

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
}
