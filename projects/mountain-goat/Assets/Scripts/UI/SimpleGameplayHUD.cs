using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameplayHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text hungerText;

    private void OnEnable()
    {
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnStateChanged += HandleStateChanged;
        GameManager.Instance.OnHungerChanged += HandleHungerChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnHungerChanged -= HandleHungerChanged;
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

    private void Refresh()
    {
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
    }
}
