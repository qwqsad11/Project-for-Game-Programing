using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TilePrefabs
    {
        public GameObject grassTile;
        public GameObject crumbleTile;
        public GameObject treeObstacle;
        public GameObject fallingRock;
    }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TilePrefabs prefabs;

    [Header("Generation")]
    [SerializeField] private int halfWidth = 2;
    [SerializeField] private int rowsAhead = 18;
    [SerializeField] private int rowsBehind = 8;
    [SerializeField] private int initialRows = 24;
    [SerializeField] private float gapChance = 0.18f;
    [SerializeField] private float crumbleChance = 0.12f;
    [SerializeField] private float treeChance = 0.08f;
    [SerializeField] private float rockChance = 0.05f;

    private readonly Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();
    private int highestGeneratedY = -1;
    private int currentSafeColumn = 0;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }

    private void Start()
    {
        GenerateUntil(initialRows - 1);
    }

    private void Update()
    {
        if (player == null || gridManager == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        int playerY = player.CurrentGrid.y;
        GenerateUntil(playerY + rowsAhead);
        PruneBelow(playerY - rowsBehind);
    }

    private void GenerateUntil(int targetY)
    {
        while (highestGeneratedY < targetY)
        {
            highestGeneratedY++;
            GenerateRow(highestGeneratedY);
        }
    }

    private void GenerateRow(int gridY)
    {
        if (gridY == 0)
        {
            currentSafeColumn = Random.Range(-halfWidth, halfWidth + 1);
        }
        else
        {
            int nextDelta = Random.Range(-1, 2);
            currentSafeColumn = Mathf.Clamp(currentSafeColumn + nextDelta, -halfWidth, halfWidth);
        }

        for (int gridX = -halfWidth; gridX <= halfWidth; gridX++)
        {
            Vector2Int coord = new Vector2Int(gridX, gridY);
            if (activeTiles.ContainsKey(coord))
            {
                continue;
            }

            bool isSafePath = gridX == currentSafeColumn;
            bool isGap = gridY > 0 && !isSafePath && Random.value < gapChance;
            if (isGap)
            {
                continue;
            }

            GameObject tilePrefab = SelectTilePrefab(isSafePath);
            if (tilePrefab == null)
            {
                continue;
            }

            Vector3 world = gridManager != null
                ? gridManager.GridToWorld(coord)
                : IsoGrid.ToWorld(coord, 1.5f, 1.5f, 0.5f);
            GameObject tile = Instantiate(tilePrefab, world, Quaternion.identity, transform);
            activeTiles.Add(coord, tile);
        }
    }

    private GameObject SelectTilePrefab(bool forceSafeTile)
    {
        if (forceSafeTile || prefabs.grassTile == null)
        {
            return prefabs.grassTile;
        }

        float roll = Random.value;

        if (prefabs.crumbleTile != null && roll < crumbleChance)
        {
            return prefabs.crumbleTile;
        }

        if (prefabs.treeObstacle != null && roll < crumbleChance + treeChance)
        {
            return prefabs.treeObstacle;
        }

        if (prefabs.fallingRock != null && roll < crumbleChance + treeChance + rockChance)
        {
            return prefabs.fallingRock;
        }

        return prefabs.grassTile;
    }

    private void PruneBelow(int minimumY)
    {
        List<Vector2Int> removeList = null;

        foreach (KeyValuePair<Vector2Int, GameObject> entry in activeTiles)
        {
            if (entry.Key.y < minimumY)
            {
                removeList ??= new List<Vector2Int>();
                removeList.Add(entry.Key);
            }
        }

        if (removeList == null)
        {
            return;
        }

        foreach (Vector2Int coord in removeList)
        {
            if (activeTiles.TryGetValue(coord, out GameObject tile) && tile != null)
            {
                Destroy(tile);
            }

            activeTiles.Remove(coord);
        }
    }
}
