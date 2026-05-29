using UnityEngine;

public class RollingObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private GameObject rockPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnIntervalMin = 2.2f;
    [SerializeField] private float spawnIntervalMax = 4.2f;
    [SerializeField] private int spawnAheadTiles = 11;
    [SerializeField] private int laneRadius = 2;
    [SerializeField] private Vector2Int rollingDirection = new Vector2Int(0, -1);
    [SerializeField] private float topViewportMargin = 0.18f;
    [SerializeField] private int maxViewportSearchTiles = 36;

    [Header("Motion")]
    [SerializeField] private float speedMin = 3.2f;
    [SerializeField] private float speedMax = 5.1f;

    private GoatController goat;
    private GridManager gridManager;
    private Camera mainCamera;
    private float timer;

    private void Awake()
    {
        LoadDefaultPrefabs();
        ResetTimer();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        ResolveGoat();
        if (goat == null)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            return;
        }

        SpawnObstacle();
        ResetTimer();
    }

    private void SpawnObstacle()
    {
        GameObject prefab = PickPrefab();
        if (prefab == null)
        {
            return;
        }

        int laneOffset = Random.Range(-laneRadius, laneRadius + 1);
        Vector2Int direction = GetScreenDownDirection();
        Vector2Int spawnGrid = GetSpawnGrid(laneOffset, direction);
        GameObject instance = Instantiate(prefab, transform);
        instance.name = prefab.name + "_Rolling";

        RollingObstacle rollingObstacle = instance.GetComponent<RollingObstacle>();
        if (rollingObstacle == null)
        {
            rollingObstacle = instance.AddComponent<RollingObstacle>();
        }

        rollingObstacle.LaunchOnGridPath(spawnGrid, direction, Random.Range(speedMin, speedMax));
    }

    private GameObject PickPrefab()
    {
        if (logPrefab != null && rockPrefab != null)
        {
            return Random.value < 0.5f ? logPrefab : rockPrefab;
        }

        return logPrefab != null ? logPrefab : rockPrefab;
    }

    private void ResetTimer()
    {
        timer = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    private void ResolveGoat()
    {
        if (goat == null)
        {
            goat = FindObjectOfType<GoatController>();
        }

        if (gridManager == null)
        {
            gridManager = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private Vector2Int GetScreenDownDirection()
    {
        if (gridManager == null || mainCamera == null || goat == null)
        {
            return rollingDirection == Vector2Int.zero ? new Vector2Int(0, -1) : rollingDirection;
        }

        Vector2Int direction = rollingDirection == Vector2Int.zero ? new Vector2Int(0, -1) : rollingDirection;
        Vector2Int referenceGrid = goat.CurrentGrid;
        float currentY = mainCamera.WorldToViewportPoint(gridManager.GridToWorld(referenceGrid)).y;
        float nextY = mainCamera.WorldToViewportPoint(gridManager.GridToWorld(referenceGrid + direction)).y;

        return nextY < currentY ? direction : -direction;
    }

    private Vector2Int GetSpawnGrid(int laneOffset, Vector2Int direction)
    {
        Vector2Int laneGrid = goat.CurrentGrid + new Vector2Int(laneOffset, 0);
        if (gridManager == null || mainCamera == null)
        {
            return laneGrid - direction * spawnAheadTiles;
        }

        Vector2Int searchDirection = -direction;
        Vector2Int bestGrid = laneGrid + searchDirection * spawnAheadTiles;

        for (int i = 1; i <= maxViewportSearchTiles; i++)
        {
            Vector2Int candidate = laneGrid + searchDirection * i;
            Vector3 world = gridManager.GridToWorld(candidate);
            float viewportY = mainCamera.WorldToViewportPoint(world).y;

            bestGrid = candidate;
            if (viewportY > 1f + topViewportMargin)
            {
                return candidate;
            }
        }

        return bestGrid;
    }

    private void LoadDefaultPrefabs()
    {
#if UNITY_EDITOR
        if (logPrefab == null)
        {
            logPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Log.prefab");
        }

        if (rockPrefab == null)
        {
            rockPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rock_blank.prefab");
        }
#endif
    }
}
