using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ChunkSpawner chunkSpawner;
    [SerializeField] private PathGenerator pathGenerator;

    [Header("Generation")]
    [SerializeField] private int halfWidth = 2;
    [SerializeField] private int initialRows = 24;
    [SerializeField] private int rowsAhead = 18;
    [SerializeField] private int rowsBehind = 8;

    [Header("Mountain Shape")]
    [SerializeField, Range(0f, 1f)] private float ridgeTurnChance = 0.28f;
    [SerializeField, Range(0f, 0.1f)] private float gapChance = 0.08f;
    [SerializeField, Range(0f, 1f)] private float extraPlatformChance = 0.22f;

    [Header("Tile Variety")]
    [SerializeField, Range(0f, 1f)] private float crumbleChance = 0.12f;
    [SerializeField, Range(0f, 1f)] private float springChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float hazardChance = 0.04f;

    private readonly Dictionary<Vector2Int, Tile> activeTiles = new Dictionary<Vector2Int, Tile>();
    private readonly HashSet<Vector2Int> mountainCells = new HashSet<Vector2Int>();
    private int highestGeneratedRow = -1;
    private Vector2Int ridgeCursor;
    private Vector2Int ridgeDirection = GoatController.UpperRight;
    private bool forceGapRecovery;

    private static readonly Vector2Int[] NeighborOffsets =
    {
        GoatController.UpperLeft,
        GoatController.UpperRight,
        GoatController.LowerLeft,
        GoatController.LowerRight
    };

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
        ClearAll();

        int startColumn = pathGenerator != null ? pathGenerator.GetSeedColumn() : 0;
        ridgeCursor = new Vector2Int(Mathf.Clamp(startColumn, -halfWidth, halfWidth), 0);
        ridgeDirection = Random.value < 0.5f ? GoatController.UpperRight : GoatController.UpperLeft;
        forceGapRecovery = false;
        highestGeneratedRow = -1;

        GenerateUntil(initialRows - 1);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        ResolveReferences();

        int playerY = GetPlayerGridY();
        GenerateUntil(playerY + rowsAhead);
        PruneBelow(playerY - rowsBehind);
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

        if (pathGenerator == null)
        {
            pathGenerator = FindObjectOfType<PathGenerator>();
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }

    private int GetPlayerGridY()
    {
        if (player != null)
        {
            return player.CurrentGrid.y;
        }

        GoatController goat = FindObjectOfType<GoatController>();
        if (goat != null)
        {
            return goat.CurrentGrid.y;
        }

        return 0;
    }

    private void GenerateUntil(int targetRow)
    {
        while (highestGeneratedRow < targetRow)
        {
            highestGeneratedRow++;
            GenerateRow(highestGeneratedRow);
        }
    }

    private void GenerateRow(int row)
    {
        if (row > 0)
        {
            ridgeCursor = ChooseNextRidgeCursor();
        }

        BuildMountainRow(ridgeCursor, row, row == 0);
    }

    private Vector2Int ChooseNextRidgeCursor()
    {
        bool canGoLeft = ridgeCursor.x > -halfWidth;
        bool canGoRight = ridgeCursor.x < halfWidth;

        if (!canGoLeft && canGoRight)
        {
            ridgeDirection = GoatController.UpperRight;
        }
        else if (!canGoRight && canGoLeft)
        {
            ridgeDirection = GoatController.UpperLeft;
        }
        else if (Random.value < ridgeTurnChance)
        {
            ridgeDirection = ridgeDirection == GoatController.UpperRight
                ? GoatController.UpperLeft
                : GoatController.UpperRight;
        }

        Vector2Int candidate = ClampToWorld(ridgeCursor + ridgeDirection);
        if (!GridManager.IsDiagonalStep(candidate - ridgeCursor))
        {
            ridgeDirection = ridgeDirection == GoatController.UpperRight
                ? GoatController.UpperLeft
                : GoatController.UpperRight;
            candidate = ClampToWorld(ridgeCursor + ridgeDirection);
        }

        return candidate;
    }

    private void BuildMountainRow(Vector2Int spinePosition, int row, bool isSeedRow)
    {
        HashSet<Vector2Int> rowCells = new HashSet<Vector2Int>();

        AddMountainCell(spinePosition, PickRidgeTileKind(row), true, rowCells);

        Vector2Int downhillOffset = ridgeDirection == GoatController.UpperRight
            ? GoatController.LowerRight
            : GoatController.LowerLeft;
        Vector2Int uphillOffset = ridgeDirection == GoatController.UpperRight
            ? GoatController.UpperLeft
            : GoatController.UpperRight;

        bool allowGap = !isSeedRow && !forceGapRecovery && Random.value < gapChance;
        if (!allowGap)
        {
            AddMountainCell(ClampToWorld(spinePosition + downhillOffset), PickSupportTileKind(row), false, rowCells);
            forceGapRecovery = false;
        }
        else
        {
            forceGapRecovery = true;
        }

        AddMountainCell(ClampToWorld(spinePosition + downhillOffset + downhillOffset), PickSupportTileKind(row), false, rowCells);

        if (Random.value < extraPlatformChance || isSeedRow)
        {
            AddMountainCell(ClampToWorld(spinePosition + uphillOffset), PickSupportTileKind(row), false, rowCells);
        }

        if (Random.value < extraPlatformChance * 0.65f)
        {
            AddMountainCell(ClampToWorld(spinePosition + uphillOffset + uphillOffset), PickSupportTileKind(row), false, rowCells);
        }

        GrowMountainFill(spinePosition, row, rowCells, isSeedRow);
    }

    private void GrowMountainFill(Vector2Int spinePosition, int row, HashSet<Vector2Int> rowCells, bool isSeedRow)
    {
        int targetDensity = isSeedRow ? 4 : 4;
        if (Random.value < extraPlatformChance)
        {
            targetDensity = 5;
        }

        Queue<Vector2Int> frontier = new Queue<Vector2Int>(rowCells);
        HashSet<Vector2Int> localVisited = new HashSet<Vector2Int>(rowCells);

        while (frontier.Count > 0 && rowCells.Count < targetDensity)
        {
            Vector2Int source = frontier.Dequeue();
            foreach (Vector2Int offset in OrderedNeighborOffsets(source, spinePosition))
            {
                Vector2Int candidate = ClampToWorld(source + offset);
                if (candidate == source || mountainCells.Contains(candidate) || localVisited.Contains(candidate))
                {
                    continue;
                }

                if (!CanPlaceCell(candidate))
                {
                    continue;
                }

                int neighborCount = CountNeighbors(candidate, mountainCells, rowCells);
                if (neighborCount < 2 && candidate != spinePosition)
                {
                    continue;
                }

                AddMountainCell(candidate, PickSupportTileKind(row), false, rowCells);
                localVisited.Add(candidate);
                frontier.Enqueue(candidate);

                if (rowCells.Count >= targetDensity)
                {
                    break;
                }
            }
        }

        if (rowCells.Count < targetDensity)
        {
            foreach (Vector2Int offset in OrderedNeighborOffsets(spinePosition, spinePosition))
            {
                if (rowCells.Count >= targetDensity)
                {
                    break;
                }

                Vector2Int candidate = ClampToWorld(spinePosition + offset);
                if (candidate == spinePosition || mountainCells.Contains(candidate) || rowCells.Contains(candidate))
                {
                    continue;
                }

                if (!CanPlaceCell(candidate))
                {
                    continue;
                }

                if (CountNeighbors(candidate, mountainCells, rowCells) >= 1)
                {
                    AddMountainCell(candidate, PickSupportTileKind(row), false, rowCells);
                }
            }
        }
    }

    private IEnumerable<Vector2Int> OrderedNeighborOffsets(Vector2Int source, Vector2Int spinePosition)
    {
        if (source == spinePosition)
        {
            if (ridgeDirection == GoatController.UpperRight)
            {
                yield return GoatController.LowerRight;
                yield return GoatController.UpperLeft;
                yield return GoatController.UpperRight;
                yield return GoatController.LowerLeft;
            }
            else
            {
                yield return GoatController.LowerLeft;
                yield return GoatController.UpperRight;
                yield return GoatController.UpperLeft;
                yield return GoatController.LowerRight;
            }

            yield break;
        }

        yield return GoatController.LowerLeft;
        yield return GoatController.LowerRight;
        yield return GoatController.UpperLeft;
        yield return GoatController.UpperRight;
    }

    private void AddMountainCell(Vector2Int gridPosition, TileType kind, bool isMainPath, HashSet<Vector2Int> rowCells)
    {
        if (!CanPlaceCell(gridPosition) || mountainCells.Contains(gridPosition))
        {
            return;
        }

        mountainCells.Add(gridPosition);
        rowCells.Add(gridPosition);
        SpawnTile(gridPosition, kind, isMainPath);
    }

    private bool CanPlaceCell(Vector2Int gridPosition)
    {
        return gridPosition.x >= -halfWidth && gridPosition.x <= halfWidth && gridPosition.y >= 0;
    }

    private int CountNeighbors(Vector2Int gridPosition, HashSet<Vector2Int> primary, HashSet<Vector2Int> secondary)
    {
        int count = 0;

        for (int i = 0; i < NeighborOffsets.Length; i++)
        {
            Vector2Int neighbor = gridPosition + NeighborOffsets[i];
            if (primary.Contains(neighbor) || secondary.Contains(neighbor))
            {
                count++;
            }
        }

        return count;
    }

    private TileType PickRidgeTileKind(int row)
    {
        float roll = Random.value;

        if (row > 2 && roll < crumbleChance)
        {
            return TileType.CrumbleTile;
        }

        roll -= crumbleChance;
        if (row > 0 && roll < springChance)
        {
            return TileType.SpringTile;
        }

        roll -= springChance;
        if (roll < hazardChance)
        {
            return TileType.HazardTile;
        }

        roll -= hazardChance;
        return TileType.NormalTile;
    }

    private TileType PickSupportTileKind(int row)
    {
        float roll = Random.value;
        if (row > 2 && roll < crumbleChance * 0.5f)
        {
            return TileType.CrumbleTile;
        }

        roll -= crumbleChance * 0.5f;
        if (roll < springChance * 0.4f)
        {
            return TileType.SpringTile;
        }

        roll -= springChance * 0.4f;
        if (roll < hazardChance * 0.25f)
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
        }
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

    private Vector2Int ClampToWorld(Vector2Int gridPosition)
    {
        return new Vector2Int(Mathf.Clamp(gridPosition.x, -halfWidth, halfWidth), Mathf.Max(0, gridPosition.y));
    }

    private void PruneBelow(int minimumY)
    {
        List<Vector2Int> toRemove = null;

        foreach (KeyValuePair<Vector2Int, Tile> entry in activeTiles)
        {
            if (entry.Key.y < minimumY)
            {
                toRemove ??= new List<Vector2Int>();
                toRemove.Add(entry.Key);
            }
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

            activeTiles.Remove(grid);
            mountainCells.Remove(grid);
        }
    }

    private void ClearAll()
    {
        List<Vector2Int> keys = new List<Vector2Int>(activeTiles.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            Vector2Int grid = keys[i];
            if (!activeTiles.TryGetValue(grid, out Tile tile) || tile == null)
            {
                activeTiles.Remove(grid);
                mountainCells.Remove(grid);
                continue;
            }

            tile.Recycle();
            if (chunkSpawner != null)
            {
                chunkSpawner.Recycle(grid);
            }
            else
            {
                Destroy(tile.gameObject);
            }

            activeTiles.Remove(grid);
            mountainCells.Remove(grid);
        }
    }
}
