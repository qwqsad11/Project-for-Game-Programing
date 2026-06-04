using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleGameOverUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text newRecordText;
    [SerializeField] private TMP_Text profileNameText;

    [Header("New Record Animation")]
    [SerializeField] private Color newRecordColor = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] private float newRecordPulseInterval = 0.7f;

    private Coroutine newRecordPulseRoutine;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();

        // Start pulsing "New Record!" if visible
        if (newRecordText != null && newRecordText.gameObject.activeSelf)
        {
            StartNewRecordPulse();
        }
    }

    private void OnDisable()
    {
        if (newRecordPulseRoutine != null)
        {
            StopCoroutine(newRecordPulseRoutine);
            newRecordPulseRoutine = null;
        }
    }

    private void Refresh()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        // ── Score ──
        if (scoreText != null)
        {
            scoreText.text = $"<size=55%>FINAL SCORE</size>\n" +
                             $"<b><size=120%>{GameManager.Instance.Score:N0}</size></b>";
        }

        // ── High Score ──
        if (highScoreText != null)
        {
            highScoreText.text = $"\U0001F3C6  Best: {GameManager.Instance.HighScore:N0}";
        }

        // ── Coins ──
        if (coinsText != null)
        {
            coinsText.text = $"\U0001F4B0  Run Coins: {GameManager.Instance.SessionCoins}";
        }

        // ── Total Coins ──
        if (totalCoinsText != null)
        {
            totalCoinsText.text = $"\U0001F4B0  Total Coins: {GameManager.Instance.TotalCoins:N0}";
        }

        // ── Profile ──
        if (profileNameText != null)
        {
            profileNameText.text = $"\U0001F464  {GameManager.Instance.ActiveProfileName}";
        }

        // ── Rank ──
        if (rankText != null)
        {
            string profileId = GameManager.Instance.ActiveProfileId;
            ILeaderboardProvider lb = GameManager.Instance.Leaderboard;
            if (lb != null && !string.IsNullOrEmpty(profileId))
            {
                int rank = lb.GetRank(profileId);
                int total = lb.EntryCount;
                if (rank > 0)
                {
                    string rankEmoji = rank == 1 ? "\U0001F947" :
                                       rank == 2 ? "\U0001F948" :
                                       rank == 3 ? "\U0001F949" : "";

                    rankText.text = $"{rankEmoji}  Rank #{rank} / {total}";
                }
                else
                {
                    rankText.text = "";
                }
            }
            else
            {
                rankText.text = "";
            }
        }

        // ── New Record ──
        if (newRecordText != null)
        {
            PlayerProfile activeProfile = ProfileManager.Instance?.GetActiveProfile();
            bool isNewRecord = activeProfile != null &&
                               GameManager.Instance.Score > 0 &&
                               GameManager.Instance.Score >= activeProfile.highScore;
            newRecordText.gameObject.SetActive(isNewRecord);

            if (isNewRecord)
            {
                newRecordText.text = "\U0001F389  NEW RECORD!  \U0001F389";
                newRecordText.color = newRecordColor;
                newRecordText.fontStyle = FontStyles.Bold;
                StartNewRecordPulse();
            }
            else
            {
                if (newRecordPulseRoutine != null)
                {
                    StopCoroutine(newRecordPulseRoutine);
                    newRecordPulseRoutine = null;
                }
            }
        }
    }

    private void StartNewRecordPulse()
    {
        if (newRecordPulseRoutine != null) return;

        if (newRecordText != null && newRecordText.gameObject.activeInHierarchy)
        {
            newRecordPulseRoutine = StartCoroutine(
                UIHelper.PulseLoop(newRecordText.transform, 1.12f, newRecordPulseInterval));
        }
    }
}
