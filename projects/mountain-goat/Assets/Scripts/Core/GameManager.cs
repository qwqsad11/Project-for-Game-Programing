using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, CharacterSelect, Playing, GameOver, Paused }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private string gameplaySceneName = "GamePlay";
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private bool forceMenuOnPlay = true;

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
    public int SessionCoins { get; private set; } = 0;
    public int TotalCoins { get; private set; } = 0;
    public float CurrentHunger { get; private set; } = 0f;
    public float MaxHunger { get; private set; } = 100f;

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
        LoadTotalCoins();

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (forceMenuOnPlay && activeSceneName != mainMenuSceneName && activeSceneName != characterSelectSceneName)
        {
            currentState = GameState.Menu;
            Time.timeScale = 1f;
            LoadSceneIfNeeded(mainMenuSceneName);
        }
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
            case GameState.CharacterSelect:
                HandleCharacterSelectState();
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
        SessionCoins = 0;
        OnScoreChanged?.Invoke(Score);
        OnCoinsChanged?.Invoke(SessionCoins, TotalCoins);
        NotifyHungerChanged(0f, MaxHunger <= 0f ? 100f : MaxHunger);
        LoadSceneIfNeeded(gameplaySceneName);
    }

    private void HandleCharacterSelectState()
    {
        Time.timeScale = 1f;
        LoadSceneIfNeeded(characterSelectSceneName);
    }

    private void HandleGameOverState()
    {
        Time.timeScale = 0f;
        if (Score > HighScore)
        {
            HighScore = Score;
            SaveHighScore();
        }

        SaveTotalCoins();

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

    public void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SessionCoins += amount;
        TotalCoins += amount;
        SaveTotalCoins();
        OnCoinsChanged?.Invoke(SessionCoins, TotalCoins);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }

    public void ChooseCharacter()
    {
        ChangeState(GameState.CharacterSelect);
    }

    public void StartGameplay()
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

    private void LoadTotalCoins()
    {
        TotalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", HighScore);
        PlayerPrefs.Save();
    }

    private void SaveTotalCoins()
    {
        PlayerPrefs.SetInt("TotalCoins", TotalCoins);
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

        if (scene.name == gameplaySceneName)
        {
            EnsureGameplaySceneContent();
        }

        if (scene.name == characterSelectSceneName)
        {
            EnsureEventSystem();
            EnsureDirectionalLight();
        }

        if (scene.name == gameplaySceneName)
        {
            CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
            GoatController goat = FindObjectOfType<GoatController>();
            if (cameraFollow != null && goat != null)
            {
                cameraFollow.SetTarget(goat.transform);
            }
        }

        if (scene.name == gameOverSceneName)
        {
            EnsureGameOverUI();
        }
    }

    private void SyncStateWithActiveScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName == gameplaySceneName)
        {
            currentState = GameState.Playing;
        }
        else if (activeSceneName == characterSelectSceneName)
        {
            currentState = GameState.CharacterSelect;
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

    private void EnsureGameplaySceneContent()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        EnsureEventSystem();
        EnsureDirectionalLight();
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            GameObject gridObject = new GameObject("GridManager");
            gridObject.AddComponent<GridManager>();
        }

        ObjectPooler pooler = FindObjectOfType<ObjectPooler>();
        if (pooler == null)
        {
            GameObject poolObject = new GameObject("ObjectPooler");
            poolObject.AddComponent<ObjectPooler>();
        }

        PathGenerator pathGenerator = FindObjectOfType<PathGenerator>();
        if (pathGenerator == null)
        {
            GameObject pathObject = new GameObject("PathGenerator");
            pathObject.AddComponent<PathGenerator>();
        }

        ChunkSpawner chunkSpawner = FindObjectOfType<ChunkSpawner>();
        if (chunkSpawner == null)
        {
            GameObject chunkObject = new GameObject("ChunkSpawner");
            chunkSpawner = chunkObject.AddComponent<ChunkSpawner>();
        }

        LevelGenerator levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator == null)
        {
            GameObject levelObject = new GameObject("LevelGenerator");
            levelGenerator = levelObject.AddComponent<LevelGenerator>();
        }

        GoatController goat = FindObjectOfType<GoatController>();
        if (goat == null)
        {
            CreateFallbackGoat();
            goat = FindObjectOfType<GoatController>();
        }

        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow != null && goat != null)
        {
            cameraFollow.SetTarget(goat.transform);
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void CreateFallbackGoat()
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        Vector2Int startGrid = Vector2Int.zero;
        Vector3 spawnPosition = IsoGrid.ToWorld(startGrid, 1.5f, 0.75f, 0.5f);

        GameObject goatPrefab = LoadEditorPrefab("Assets/Prefabs/Player.prefab");
        GameObject goatObject = goatPrefab != null
            ? Instantiate(goatPrefab)
            : GameObject.CreatePrimitive(PrimitiveType.Capsule);

        goatObject.name = "RuntimeGoat";
        goatObject.transform.position = spawnPosition + new Vector3(0f, 0.5f, 0f);
        goatObject.transform.localScale = new Vector3(0.85f, 1.0f, 0.85f);

        if (goatObject.GetComponent<GoatController>() == null)
        {
            goatObject.AddComponent<GoatController>();
        }

        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(goatObject.transform);
        }

        if (gridManager != null)
        {
            goatObject.transform.position = gridManager.GridToWorld(startGrid) + new Vector3(0f, 0.5f, 0f);
        }
    }

    private void EnsureDirectionalLight()
    {
        if (FindObjectOfType<Light>() != null)
        {
            return;
        }

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        DontDestroyOnLoad(lightObject);
    }

    private void EnsureGameOverUI()
    {
        EnsureEventSystem();

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                canvasObject.layer = uiLayer;
            }

            Canvas createdCanvas = canvasObject.AddComponent<Canvas>();
            createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvas = createdCanvas;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            canvasRect.localScale = Vector3.one;
        }

        Button backButton = FindObjectOfType<Button>();
        if (backButton == null)
        {
            GameObject buttonObject = CreateSimpleButton("BackButton", "Back", new Vector2(0.5f, 0.5f), new Vector2(0f, -120f));
            backButton = buttonObject.GetComponent<Button>();
        }
        else
        {
            RectTransform buttonRect = backButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.SetParent(canvas.transform, false);
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = new Vector2(0f, -120f);
                buttonRect.sizeDelta = new Vector2(220f, 60f);
                buttonRect.localScale = Vector3.one;
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMenu);
        }
    }

    private GameObject CreateSimpleButton(string buttonName, string label, Vector2 anchor, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(buttonName);
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
        {
            buttonObject.layer = uiLayer;
        }
        buttonObject.transform.SetParent(FindObjectOfType<Canvas>().transform, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 60f);
        rect.localScale = Vector3.one;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();

        GameObject textObject = new GameObject("Text (TMP)");
        if (uiLayer >= 0)
        {
            textObject.layer = uiLayer;
        }
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        return buttonObject;
    }

    private GameObject LoadEditorPrefab(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        return null;
#endif
    }

    public void NotifyHungerChanged(float currentHunger, float maxHunger)
    {
        CurrentHunger = currentHunger;
        MaxHunger = maxHunger;
        OnHungerChanged?.Invoke(CurrentHunger, MaxHunger);
    }

    // Events
    public delegate void StateChanged(GameState state);
    public event StateChanged OnStateChanged;

    public delegate void ScoreChanged(int score);
    public event ScoreChanged OnScoreChanged;

    public delegate void HungerChanged(float currentHunger, float maxHunger);
    public event HungerChanged OnHungerChanged;

    public delegate void CoinsChanged(int sessionCoins, int totalCoins);
    public event CoinsChanged OnCoinsChanged;
}
