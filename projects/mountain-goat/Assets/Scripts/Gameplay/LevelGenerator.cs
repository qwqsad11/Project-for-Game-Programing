using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoatController goat;
    [SerializeField] private PlayerController player;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ChunkSpawner chunkSpawner;

    [Header("Visibility")]
    [SerializeField] private int visibleAreaSize = 50;
    [SerializeField] private int recenterThreshold = 2;

    [Header("Tile Variety")]
    [SerializeField] private bool safePlatformOnly = true;
    [SerializeField, Range(0f, 1f)] private float crumbleChance = 0.12f;
    [SerializeField, Range(0f, 1f)] private float springChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float hazardChance = 0.04f;
    [SerializeField, Range(0f, 1f)] private float dangerousPlatformChance = 0.08f;

    [Header("Thunderclouds")]
    [SerializeField] private GameObject thundercloudPrefab;
    [SerializeField, Range(0f, 1f)] private float thundercloudChance = 0.08f;
    [SerializeField] private int thundercloudMinY = 4;
    [SerializeField] private Vector3 thundercloudOffset = new Vector3(0f, 1.8f, 0f);

    private readonly Dictionary<Vector2Int, Tile> activeTiles = new Dictionary<Vector2Int, Tile>();
    private readonly Dictionary<Vector2Int, ThundercloudHazard> activeThunderclouds = new Dictionary<Vector2Int, ThundercloudHazard>();
    private readonly HashSet<Vector2Int> generatedCells = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> protectedCells = new HashSet<Vector2Int>();
    private Vector2Int windowCenter;
    private bool hasWindowCenter;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
        ClearAll();

        Vector2Int startGrid = GetReferenceGrid();
        SetWindowCenter(startGrid, true);
    }

    public void RequestAheadForJump(Vector2Int currentGrid, Vector2Int targetGrid)
    {
        EnsureCell(currentGrid, true, true);

        if (IsInsideCurrentWindow(targetGrid))
        {
            EnsureCell(targetGrid, true, true);
        }
    }

    public void NotifyLanded(Vector2Int landedGrid)
    {
        EnsureCell(landedGrid, true, true);
        if (!hasWindowCenter || GridDistance(landedGrid, windowCenter) >= recenterThreshold)
        {
            SetWindowCenter(landedGrid, true);
        }
    }

    public bool EnsurePlatformAt(Vector2Int gridPosition)
    {
        if (!CanPlaceCell(gridPosition) || !IsInsideCurrentWindow(gridPosition))
        {
            return false;
        }

        EnsureCell(gridPosition, true, true);
        return gridManager != null && gridManager.HasPlatform(gridPosition);
    }

    private void SetWindowCenter(Vector2Int centerGrid, bool pruneAfterGenerate)
    {
        windowCenter = centerGrid;
        hasWindowCenter = true;
        EnsureVisibleWindow(centerGrid);

        if (pruneAfterGenerate)
        {
            PruneOutsideWindow(centerGrid);
        }
    }

    private void EnsureVisibleWindow(Vector2Int centerGrid)
    {
        int halfSize = Mathf.Max(1, visibleAreaSize / 2);
        for (int y = centerGrid.y - halfSize; y <= centerGrid.y + halfSize; y++)
        {
            for (int x = centerGrid.x - halfSize; x <= centerGrid.x + halfSize; x++)
            {
                Vector2Int grid = new Vector2Int(x, y);
                EnsureCell(grid, GridDistance(grid, centerGrid) <= 1, GridDistance(grid, centerGrid) <= 1);
            }
        }
    }

    private void EnsureCell(Vector2Int gridPosition, bool isMainPath, bool protectFromPrune)
    {
        if (!CanPlaceCell(gridPosition))
        {
            return;
        }

        if (protectFromPrune)
        {
            protectedCells.Add(gridPosition);
        }

        if (generatedCells.Contains(gridPosition) || activeTiles.ContainsKey(gridPosition))
        {
            return;
        }

        generatedCells.Add(gridPosition);
        SpawnTile(gridPosition, PickTileKind(gridPosition, isMainPath), isMainPath);
    }

    private TileType PickTileKind(Vector2Int gridPosition, bool isMainPath)
    {
        if (!isMainPath && gridPosition.y > 1 && Random.value < dangerousPlatformChance)
        {
            return TileType.CrumbleTile;
        }

        if (safePlatformOnly || gridPosition.y <= 1 || isMainPath)
        {
            return TileType.NormalTile;
        }

        float roll = Random.value;
        if (roll < crumbleChance)
        {
            return TileType.CrumbleTile;
        }

        roll -= crumbleChance;
        if (roll < springChance)
        {
            return TileType.SpringTile;
        }

        roll -= springChance;
        if (roll < hazardChance)
        {
            return TileType.HazardTile;
        }

        return TileType.NormalTile;
    }

    private void SpawnTile(Vector2Int gridPosition, TileType kind, bool isMainPath)
    {
        if (chunkSpawner == null || gridManager == null || activeTiles.ContainsKey(gridPosition))
        {
            return;
        }

        Tile tile = chunkSpawner.Spawn(gridPosition, ToLegacyKind(kind), isMainPath, transform);
        if (tile != null)
        {
            activeTiles[gridPosition] = tile;
            TrySpawnThundercloud(gridPosition, isMainPath);
        }
    }

    private void TrySpawnThundercloud(Vector2Int gridPosition, bool isMainPath)
    {
        if (gridPosition.y < thundercloudMinY || activeThunderclouds.ContainsKey(gridPosition))
        {
            return;
        }

        if (isMainPath && GridDistance(gridPosition, windowCenter) <= 1)
        {
            return;
        }

        if (Random.value >= thundercloudChance)
        {
            return;
        }

        GameObject prefab = ResolveThundercloudPrefab();
        if (prefab == null)
        {
            return;
        }

        Vector3 tilePosition = GridManager.Instance != null
            ? GridManager.Instance.GridToWorld(gridPosition)
            : IsoGrid.ToWorld(gridPosition, 1.5f, 0.75f, 0.5f);
        GameObject instance = Instantiate(prefab, tilePosition + thundercloudOffset, Quaternion.identity, transform);
        ThundercloudHazard hazard = instance.GetComponent<ThundercloudHazard>();
        if (hazard == null)
        {
            hazard = instance.AddComponent<ThundercloudHazard>();
        }

        hazard.Initialize(gridPosition);
        activeThunderclouds[gridPosition] = hazard;
    }

    private void PruneOutsideWindow(Vector2Int centerGrid)
    {
        List<Vector2Int> toRemove = null;
        int halfSize = Mathf.Max(1, visibleAreaSize / 2);
        foreach (KeyValuePair<Vector2Int, Tile> entry in activeTiles)
        {
            Vector2Int grid = entry.Key;
            if (protectedCells.Contains(grid) || IsInsideWindow(grid, centerGrid, halfSize))
            {
                continue;
            }

            toRemove ??= new List<Vector2Int>();
            toRemove.Add(grid);
        }

        if (toRemove == null)
        {
            return;
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            Vector2Int grid = toRemove[i];
            if (activeTiles.TryGetValue(grid, out Tile tile) && tile != null)
            {
                chunkSpawner.Recycle(grid);
            }

            RecycleThundercloud(grid);
            activeTiles.Remove(grid);
            generatedCells.Remove(grid);
            protectedCells.Remove(grid);
        }
    }

    private void ClearAll()
    {
        List<Vector2Int> keys = new List<Vector2Int>(activeTiles.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            Vector2Int grid = keys[i];
            if (activeTiles.TryGetValue(grid, out Tile tile) && tile != null)
            {
                if (chunkSpawner != null)
                {
                    chunkSpawner.Recycle(grid);
                }
                else
                {
                    Destroy(tile.gameObject);
                }
            }

            activeTiles.Remove(grid);
        }

        List<Vector2Int> thundercloudKeys = new List<Vector2Int>(activeThunderclouds.Keys);
        for (int i = 0; i < thundercloudKeys.Count; i++)
        {
            RecycleThundercloud(thundercloudKeys[i]);
        }

        generatedCells.Clear();
        protectedCells.Clear();
        hasWindowCenter = false;
    }

    private void RecycleThundercloud(Vector2Int gridPosition)
    {
        if (!activeThunderclouds.TryGetValue(gridPosition, out ThundercloudHazard hazard))
        {
            return;
        }

        activeThunderclouds.Remove(gridPosition);
        if (hazard != null)
        {
            Destroy(hazard.gameObject);
        }
    }

    private void ResolveReferences()
    {
        if (gridManager == null)
        {
            gridManager = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        }

        if (chunkSpawner == null)
        {
            chunkSpawner = FindObjectOfType<ChunkSpawner>();
        }

        if (goat == null)
        {
            goat = FindObjectOfType<GoatController>();
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }

    private Vector2Int GetReferenceGrid()
    {
        if (goat != null)
        {
            return goat.CurrentGrid;
        }

        if (player != null)
        {
            return player.CurrentGrid;
        }

        return Vector2Int.zero;
    }

    private static PlatformKind ToLegacyKind(TileType tileType)
    {
        return tileType switch
        {
            TileType.CrumbleTile => PlatformKind.Crumble,
            TileType.SpringTile => PlatformKind.Spring,
            TileType.HazardTile => PlatformKind.Hazard,
            TileType.CoinTile => PlatformKind.Coin,
            TileType.EmptyGap => PlatformKind.Gap,
            _ => PlatformKind.Grass
        };
    }

    private static bool IsInsideWindow(Vector2Int gridPosition, Vector2Int centerGrid, int halfSize)
    {
        return Mathf.Abs(gridPosition.x - centerGrid.x) <= halfSize
            && Mathf.Abs(gridPosition.y - centerGrid.y) <= halfSize;
    }

    private bool IsInsideCurrentWindow(Vector2Int gridPosition)
    {
        if (!hasWindowCenter)
        {
            return true;
        }

        int halfSize = Mathf.Max(1, visibleAreaSize / 2);
        return IsInsideWindow(gridPosition, windowCenter, halfSize);
    }

    private static int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    private static bool CanPlaceCell(Vector2Int gridPosition)
    {
        return gridPosition.y >= 0;
    }

    private GameObject ResolveThundercloudPrefab()
    {
        if (thundercloudPrefab != null)
        {
            return thundercloudPrefab;
        }

#if UNITY_EDITOR
        thundercloudPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Thundercloud_Prewarm.prefab");
#endif
        return thundercloudPrefab;
    }
}
