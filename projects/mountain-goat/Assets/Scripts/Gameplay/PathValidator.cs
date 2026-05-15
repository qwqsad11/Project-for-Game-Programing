using System.Collections.Generic;
using UnityEngine;

public static class PathValidator
{
    public static int ChooseNextSafeColumn(int currentSafeColumn, int halfWidth)
    {
        int next = currentSafeColumn + Random.Range(-1, 2);
        return Mathf.Clamp(next, -halfWidth, halfWidth);
    }

    public static bool IsStepReachable(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        return dy == 1 && dx <= 1;
    }

    public static bool HasPathToTop(IReadOnlyCollection<Vector2Int> tiles, Vector2Int start, int targetY)
    {
        HashSet<Vector2Int> tileSet = new HashSet<Vector2Int>(tiles);
        if (!tileSet.Contains(start))
        {
            return false;
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] neighbors =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current.y >= targetY)
            {
                return true;
            }

            foreach (Vector2Int neighbor in neighbors)
            {
                Vector2Int next = current + neighbor;
                if (visited.Contains(next) || !tileSet.Contains(next))
                {
                    continue;
                }

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return false;
    }
}
