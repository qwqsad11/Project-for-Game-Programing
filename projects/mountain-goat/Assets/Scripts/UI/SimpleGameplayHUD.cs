using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameplayHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    private void OnEnable()
    {
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnStateChanged += HandleStateChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
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
    }
}
