using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver, Paused }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "GamePlay";
    [SerializeField] private string gameOverSceneName = "GameOver";

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

    private void Start()
    {
        SyncStateWithActiveScene();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
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
        LoadSceneIfNeeded(mainMenuSceneName);
    }

    private void HandlePlayingState()
    {
        Time.timeScale = 1f;
        Score = 0;
        OnScoreChanged?.Invoke(Score);
        LoadSceneIfNeeded(gameplaySceneName);
    }

    private void HandleGameOverState()
    {
        Time.timeScale = 0f;
        if (Score > HighScore)
        {
            HighScore = Score;
            SaveHighScore();
        }

        LoadSceneIfNeeded(gameOverSceneName);
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

    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }

    public void ReturnToMenu()
    {
        ChangeState(GameState.Menu);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        currentState = GameState.Menu;
        StartGame();
    }

    public void SetPaused(bool paused)
    {
        if (paused)
        {
            ChangeState(GameState.Paused);
            return;
        }

        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            OnStateChanged?.Invoke(currentState);
        }
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

    private void LoadSceneIfNeeded(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("GameManager scene name is empty.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == sceneName)
        {
            SyncStateWithActiveScene();
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SyncStateWithActiveScene();
    }

    private void SyncStateWithActiveScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName == gameplaySceneName)
        {
            currentState = GameState.Playing;
        }
        else if (activeSceneName == gameOverSceneName)
        {
            currentState = GameState.GameOver;
        }
        else
        {
            currentState = GameState.Menu;
        }

        OnStateChanged?.Invoke(currentState);
    }

    // Events
    public delegate void StateChanged(GameState state);
    public event StateChanged OnStateChanged;

    public delegate void ScoreChanged(int score);
    public event ScoreChanged OnScoreChanged;
}
