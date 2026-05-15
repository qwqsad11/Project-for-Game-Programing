using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameOverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text totalCoinsText;

    private void Start()
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

        if (coinsText != null)
        {
            coinsText.text = $"Run Coins: {GameManager.Instance.SessionCoins}";
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = $"Total Coins: {GameManager.Instance.TotalCoins}";
        }
    }
}
