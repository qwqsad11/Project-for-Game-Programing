using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Iso Grid")]
    [SerializeField] private float tileWidth = 1.5f;
    [SerializeField] private float tileDepth = 0.75f;
    [SerializeField] private float tileHeight = 0.5f;
    [SerializeField] private int gridRadius = 4;

    private readonly Dictionary<Vector2Int, Tile> platforms = new Dictionary<Vector2Int, Tile>();

    public float TileWidth => tileWidth;
    public float TileDepth => tileDepth;
    public float TileHeight => tileHeight;
    public int GridRadius => gridRadius;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static bool IsDiagonalStep(Vector2Int delta)
    {
        return Mathf.Abs(delta.x) == 1 && Mathf.Abs(delta.y) == 1;
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return IsoGrid.GridToWorld(gridPosition, tileWidth, tileDepth, tileHeight);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return IsoGrid.WorldToGrid(worldPosition, tileWidth, tileDepth);
    }

    public Vector2Int SnapToGrid(Vector2Int gridPosition)
    {
        int clampedX = Mathf.Clamp(gridPosition.x, -gridRadius, gridRadius);
        return new Vector2Int(clampedX, Mathf.Max(0, gridPosition.y));
    }

    public bool RegisterTile(Tile tile)
    {
        if (tile == null)
        {
            return false;
        }

        platforms[tile.GridPosition] = tile;
        return true;
    }

    public bool RegisterPlatform(Tile tile) => RegisterTile(tile);

    public void UnregisterTile(Tile tile)
    {
        if (tile == null)
        {
            return;
        }

        Vector2Int gridPosition = tile.GridPosition;
        if (platforms.TryGetValue(gridPosition, out Tile existing) && existing == tile)
        {
            platforms.Remove(gridPosition);
        }
    }

    public void UnregisterPlatform(Tile tile) => UnregisterTile(tile);

    public bool HasTile(Vector2Int gridPosition)
    {
        return platforms.ContainsKey(gridPosition);
    }

    public bool HasPlatform(Vector2Int gridPosition) => HasTile(gridPosition);

    public bool TryGetTile(Vector2Int gridPosition, out Tile tile)
    {
        return platforms.TryGetValue(gridPosition, out tile);
    }

    public IReadOnlyCollection<Vector2Int> GetActiveGrids()
    {
        return platforms.Keys;
    }
}
