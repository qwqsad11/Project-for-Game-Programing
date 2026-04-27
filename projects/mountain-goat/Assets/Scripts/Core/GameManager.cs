using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, Paused }

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private GameState currentState = GameState.Menu;
    public GameState CurrentState => currentState;

    public int Score { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        LoadHighScore();
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnStateChanged?.Invoke(currentState);

        switch (newState)
        {
            case GameState.Menu:
                HandleMenuState();
                break;
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.GameOver:
                HandleGameOverState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
        }
    }

    private void HandleMenuState()
    {
        Time.timeScale = 1f;
        // TODO: Load main menu scene
    }

    private void HandlePlayingState()
    {
        Time.timeScale = 1f;
        Score = 0;
        // TODO: Load game scene
    }

    private void HandleGameOverState()
    {
        Time.timeScale = 0f;
        if (Score > HighScore)
        {
            HighScore = Score;
            SaveHighScore();
        }
        // TODO: Load game over scene
    }

    private void HandlePausedState()
    {
        Time.timeScale = 0f;
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    private void LoadHighScore()
    {
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", HighScore);
        PlayerPrefs.Save();
    }

    // Events
    public delegate void StateChanged(GameState state);
    public event StateChanged OnStateChanged;

    public delegate void ScoreChanged(int score);
    public event ScoreChanged OnScoreChanged;
}