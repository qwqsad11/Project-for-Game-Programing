using UnityEngine;

public static class IsoGrid
{
    public static Vector3 GridToWorld(Vector2Int gridPosition, float tileWidth, float tileDepth, float heightStep)
    {
        float worldX = (gridPosition.x - gridPosition.y) * tileWidth * 0.5f;
        float worldZ = (gridPosition.x + gridPosition.y) * tileDepth * 0.5f;
        float worldY = gridPosition.y * heightStep;
        return new Vector3(worldX, worldY, worldZ);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPosition, float tileWidth, float tileDepth)
    {
        float x = worldPosition.x / (tileWidth * 0.5f);
        float z = worldPosition.z / (tileDepth * 0.5f);

        int gridX = Mathf.RoundToInt((z + x) * 0.5f);
        int gridY = Mathf.RoundToInt((z - x) * 0.5f);
        return new Vector2Int(gridX, gridY);
    }

    public static Vector3 ToWorld(Vector2Int grid, float tileWidth, float tileDepth, float heightStep)
    {
        return GridToWorld(grid, tileWidth, tileDepth, heightStep);
    }

    public static Vector2Int ToGrid(Vector3 world, float tileWidth, float tileDepth)
    {
        return WorldToGrid(world, tileWidth, tileDepth);
    }
}
