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

    private void OnEnable()
    {
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
        Refresh();
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
}
