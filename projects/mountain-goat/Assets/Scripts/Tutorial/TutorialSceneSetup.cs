using UnityEngine;

/// <summary>
/// Programmatically creates the tutorial scene using the SAME build systems as GamePlay:
/// ChunkSpawner, ObjectPooler, GridManager, CameraFollow, and Player.prefab.
/// </summary>
public class TutorialSceneSetup : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2Int goatStartGrid = Vector2Int.zero;

    [Header("Prefab Paths (Editor only)")]
    [SerializeField] private string playerPrefabPath = "Assets/Prefabs/Player.prefab";
    [SerializeField] private string grassPickupPrefabPath = "Assets/Prefabs/GrassPickup.prefab";
    [SerializeField] private string coinPickupPrefabPath = "Assets/Prefabs/Coin.prefab";

    private ChunkSpawner chunkSpawner;
    private ObjectPooler objectPooler;

    // ── Safe area: every cell gets a tile so the player can't fall off ──
    private const int GridMinX = -2;
    private const int GridMaxX = 2;
    private const int GridMinY = 0;
    private const int GridMaxY = 5;

    /// <summary>Special tile overrides within the safe area.</summary>
    private static readonly (int x, int y, PlatformKind kind, bool grass, bool coin)[] SpecialTiles =
    {
        (-1,  3, PlatformKind.Grass,   true,  false),  // Grass pickup  (step 4)
        (-1,  4, PlatformKind.Grass,   false, true ),  // Coin pickup   (step 5)
        ( 1,  3, PlatformKind.Hazard,  false, false),  // Danger demo    (step 6)
    };

    // ── Unity Lifecycle ───────────────────────────────────

    private void Awake()
    {
        CreateDirectionalLight();
        CreateGridManager();
        CreateObjectPooler();
        CreateChunkSpawner();
        CreateTutorialTiles();
        CreateCamera();
        CreateGoat();
    }

    private void Start()
    {
        // Ensure TutorialManager initializes after scene setup
        if (TutorialManager.Instance == null)
        {
            GameObject obj = new GameObject("TutorialManager");
            obj.AddComponent<TutorialManager>();
        }
    }

    // ── Camera (same style as gameplay CameraFollow) ──────

    private void CreateCamera()
    {
        if (FindObjectOfType<Camera>() != null) return;

        GameObject camObj = new GameObject("MainCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.6f;
        cam.backgroundColor = new Color(0.25f, 0.45f, 0.55f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;

        // Use CameraFollow so the camera tracks the goat just like in gameplay
        camObj.AddComponent<CameraFollow>();
    }

    // ── Lighting ──────────────────────────────────────────

    private void CreateDirectionalLight()
    {
        if (FindObjectOfType<Light>() != null) return;

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // ── Grid Manager ──────────────────────────────────────

    private void CreateGridManager()
    {
        if (GridManager.Instance != null) return;

        GameObject obj = new GameObject("GridManager");
        obj.AddComponent<GridManager>();
    }

    // ── Object Pooler (required by ChunkSpawner) ──────────

    private void CreateObjectPooler()
    {
        objectPooler = FindObjectOfType<ObjectPooler>();
        if (objectPooler == null)
        {
            GameObject obj = new GameObject("ObjectPooler");
            objectPooler = obj.AddComponent<ObjectPooler>();
        }
    }

    // ── Chunk Spawner (same tile factory as gameplay) ─────

    private void CreateChunkSpawner()
    {
        chunkSpawner = FindObjectOfType<ChunkSpawner>();
        if (chunkSpawner == null)
        {
            GameObject obj = new GameObject("ChunkSpawner");
            chunkSpawner = obj.AddComponent<ChunkSpawner>();
        }
    }

    // ── Goat (same prefab as gameplay) ────────────────────

    private void CreateGoat()
    {
        GameObject goatPrefab = LoadPrefab(playerPrefabPath);
        GameObject goatObj;

        if (goatPrefab != null)
        {
            goatObj = Instantiate(goatPrefab);
            goatObj.name = "TutorialGoat";
        }
        else
        {
            goatObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            goatObj.name = "TutorialGoat";
            goatObj.AddComponent<GoatMovement>();
            goatObj.AddComponent<JumpController>();
            goatObj.AddComponent<HungerSystem>();
        }

        // Ensure required components
        if (goatObj.GetComponent<GoatMovement>() == null)
            goatObj.AddComponent<GoatMovement>();
        if (goatObj.GetComponent<JumpController>() == null)
            goatObj.AddComponent<JumpController>();
        if (goatObj.GetComponent<HungerSystem>() == null)
            goatObj.AddComponent<HungerSystem>();
        if (goatObj.GetComponent<Animator>() == null)
            goatObj.AddComponent<Animator>();
        if (goatObj.GetComponent<Collider>() == null)
            goatObj.AddComponent<CapsuleCollider>();

        // Position on the start grid
        Vector3 spawnPos = IsoGrid.ToWorld(goatStartGrid,
            GridManager.Instance != null ? GridManager.Instance.TileWidth : 1.5f,
            GridManager.Instance != null ? GridManager.Instance.TileDepth : 0.75f,
            GridManager.Instance != null ? GridManager.Instance.TileHeight : 0.5f);
        goatObj.transform.position = spawnPos + new Vector3(0f, 0.5f, 0f);
    }

    // ── Tutorial Tile Layout ──────────────────────────────

    private void CreateTutorialTiles()
    {
        // Track which positions have special tiles
        var specialPositions = new System.Collections.Generic.HashSet<Vector2Int>();
        for (int i = 0; i < SpecialTiles.Length; i++)
        {
            specialPositions.Add(new Vector2Int(SpecialTiles[i].x, SpecialTiles[i].y));
        }

        // Fill the safe area with normal tiles via ChunkSpawner
        for (int x = GridMinX; x <= GridMaxX; x++)
        {
            for (int y = GridMinY; y <= GridMaxY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!specialPositions.Contains(pos))
                {
                    chunkSpawner.Spawn(pos, PlatformKind.Grass, isMainPath: true);
                }
            }
        }

        // Create special tiles
        for (int i = 0; i < SpecialTiles.Length; i++)
        {
            var def = SpecialTiles[i];
            Vector2Int pos = new Vector2Int(def.x, def.y);
            Tile tile = chunkSpawner.Spawn(pos, def.kind, isMainPath: true);

            // Manually add pickups on specific tiles (overriding random spawn)
            if (tile != null)
            {
                if (def.grass) SpawnPickupOnTile(tile.gameObject, grassPickupPrefabPath, 0.6f);
                if (def.coin)  SpawnPickupOnTile(tile.gameObject, coinPickupPrefabPath, 0.65f);
            }
        }
    }

    private void SpawnPickupOnTile(GameObject tileObj, string prefabPath, float yOffset)
    {
        GameObject prefab = LoadPrefab(prefabPath);
        if (prefab == null) return;

        GameObject pickup = Instantiate(prefab, tileObj.transform);
        pickup.transform.localPosition = new Vector3(0f, yOffset, 0f);
        pickup.transform.localRotation = Quaternion.identity;

        // Ensure trigger collider
        Collider c = pickup.GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    // ── Helpers ───────────────────────────────────────────

    private static GameObject LoadPrefab(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        return null;
#endif
    }
}
