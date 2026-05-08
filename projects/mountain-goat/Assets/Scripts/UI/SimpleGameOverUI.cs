using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameOverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    private void Start()
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
