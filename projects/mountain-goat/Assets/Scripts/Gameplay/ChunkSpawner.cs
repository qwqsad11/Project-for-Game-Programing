using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject safePlatformPrefab;
    [SerializeField] private GameObject crumblePlatformPrefab;
    [SerializeField] private GameObject dangerousPlatformPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject bonusPrefab;

    [Header("Pooling")]
    [SerializeField] private Transform spawnedRoot;
    [SerializeField] private ObjectPooler pooler;

    private readonly Dictionary<Vector2Int, Tile> activeByGrid = new Dictionary<Vector2Int, Tile>();
    private readonly Dictionary<PlatformKind, GameObject> runtimePrefabs = new Dictionary<PlatformKind, GameObject>();

    public bool HasTile(Vector2Int grid)
    {
        return activeByGrid.ContainsKey(grid);
    }

    private void Awake()
    {
        if (pooler == null)
        {
            pooler = GetComponent<ObjectPooler>();
        }

        if (pooler == null)
        {
            pooler = gameObject.AddComponent<ObjectPooler>();
        }
    }

    public Tile Spawn(Vector2Int grid, PlatformKind kind, bool isMainPath, Transform parent = null)
    {
        GameObject prefab = ResolvePrefab(kind);
        if (prefab == null)
        {
            return null;
        }

        Vector3 worldPosition = GridManager.Instance != null
            ? GridManager.Instance.GridToWorld(grid)
            : IsoGrid.ToWorld(grid, 1.5f, 1.5f, 0.5f);
        GameObject instance = pooler != null
            ? pooler.Spawn(prefab, worldPosition, Quaternion.identity, parent != null ? parent : spawnedRoot)
            : Instantiate(prefab, worldPosition, Quaternion.identity, parent != null ? parent : spawnedRoot);

        if (instance == null)
        {
            return null;
        }

        Tile platform = instance.GetComponent<Tile>();
        if (platform == null)
        {
            platform = instance.AddComponent<SafePlatform>();
        }

        platform.Initialize(grid, kind, isMainPath);
        activeByGrid[grid] = platform;
        return platform;
    }

    public void RecycleBelow(int minimumY)
    {
        List<Vector2Int> toRecycle = null;

        foreach (KeyValuePair<Vector2Int, Tile> entry in activeByGrid)
        {
            if (entry.Key.y < minimumY)
            {
                toRecycle ??= new List<Vector2Int>();
                toRecycle.Add(entry.Key);
            }
        }

        if (toRecycle == null)
        {
            return;
        }

        foreach (Vector2Int grid in toRecycle)
        {
            Recycle(grid);
        }
    }

    public void Recycle(Vector2Int grid)
    {
        if (!activeByGrid.TryGetValue(grid, out Tile platform) || platform == null)
        {
            activeByGrid.Remove(grid);
            return;
        }

        GameObject instance = platform.gameObject;
        platform.Recycle();
        if (pooler != null)
        {
            pooler.Despawn(instance);
        }
        else
        {
            Destroy(instance);
        }

        activeByGrid.Remove(grid);
    }

    public void ClearAll()
    {
        List<Vector2Int> keys = new List<Vector2Int>(activeByGrid.Keys);
        foreach (Vector2Int key in keys)
        {
            Recycle(key);
        }
    }

    private GameObject ResolvePrefab(PlatformKind kind)
    {
        if (kind == PlatformKind.Gap)
        {
            return null;
        }

        GameObject prefab = kind switch
        {
            PlatformKind.Grass => safePlatformPrefab,
            PlatformKind.Crumble => ResolveDangerousPlatformPrefab(),
            PlatformKind.Spring => safePlatformPrefab,
            PlatformKind.Hazard => obstaclePrefab != null ? obstaclePrefab : safePlatformPrefab,
            PlatformKind.Coin => bonusPrefab != null ? bonusPrefab : safePlatformPrefab,
            _ => safePlatformPrefab
        };

        if (prefab != null)
        {
            return prefab;
        }

        GameObject editorPrefab = LoadEditorPrefab(kind);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }

        if (!runtimePrefabs.TryGetValue(kind, out GameObject runtimePrefab) || runtimePrefab == null)
        {
            runtimePrefab = CreateRuntimePrefab(kind);
            runtimePrefabs[kind] = runtimePrefab;
        }

        return runtimePrefab;
    }

    private GameObject ResolveDangerousPlatformPrefab()
    {
        if (dangerousPlatformPrefab != null)
        {
            return dangerousPlatformPrefab;
        }

        GameObject editorPrefab = LoadEditorPrefab(PlatformKind.Crumble);
        if (editorPrefab != null)
        {
            dangerousPlatformPrefab = editorPrefab;
            return dangerousPlatformPrefab;
        }

        return crumblePlatformPrefab != null ? crumblePlatformPrefab : safePlatformPrefab;
    }

    private GameObject CreateRuntimePrefab(PlatformKind kind)
    {
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
        template.name = $"Runtime_{kind}_Prefab";
        template.hideFlags = HideFlags.HideAndDontSave;
        template.SetActive(false);

        Transform t = template.transform;
        t.localScale = new Vector3(1.5f, 0.3f, 0.75f);

        Renderer renderer = template.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = kind switch
            {
                PlatformKind.Crumble => new Color(0.78f, 0.58f, 0.30f, 1f),
                PlatformKind.Spring => new Color(0.35f, 0.78f, 0.95f, 1f),
                PlatformKind.Hazard => new Color(0.82f, 0.25f, 0.22f, 1f),
                PlatformKind.Coin => new Color(0.95f, 0.80f, 0.18f, 1f),
                _ => new Color(0.30f, 0.75f, 0.28f, 1f)
            };
            renderer.sharedMaterial = material;
        }

        return template;
    }

    private GameObject LoadEditorPrefab(PlatformKind kind)
    {
#if UNITY_EDITOR
        string path = kind switch
        {
            PlatformKind.Grass => "Assets/Prefabs/SafePlatform.prefab",
            PlatformKind.Crumble => "Assets/Prefabs/DangerousPlatform.prefab",
            PlatformKind.Spring => "Assets/Prefabs/SafePlatform.prefab",
            PlatformKind.Hazard => "Assets/Prefabs/SafePlatform.prefab",
            PlatformKind.Coin => "Assets/Prefabs/Coin.prefab",
            _ => "Assets/Prefabs/SafePlatform.prefab"
        };

        GameObject loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (loaded == null && kind == PlatformKind.Grass)
        {
            loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GrassPickup.prefab");
        }

        return loaded;
#else
        return null;
#endif
    }
}
