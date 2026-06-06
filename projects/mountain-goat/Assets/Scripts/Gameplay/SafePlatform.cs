using UnityEngine;

public class SafePlatform : Tile
{
    [Header("Collectibles")]
    [SerializeField, Range(0f, 1f)] private float grassPickupChance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float coinPickupChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float treasureChestChance = 0.025f;
    [SerializeField] private float pickupYOffset = 0.65f;
    [SerializeField] private float treasureChestYOffset = 0.45f;
    [SerializeField] private GameObject grassPickupPrefab;
    [SerializeField] private GameObject coinPickupPrefab;

    [Header("Treasure Chest Rarity")]
    [SerializeField] private GameObject[] treasureChestPrefabs; // [0]=Common, [1]=Rare, [2]=Epic, [3]=Legendary

    // Rarity weights: Common 60%, Rare 25%, Epic 12%, Legendary 3%
    private static readonly float[] ChestRarityWeights = { 0.60f, 0.25f, 0.12f, 0.03f };
    // Altitude bonus: per this many grid Y levels, Legendary/Epic weights increase
    private const float RarityAltitudeScale = 0.015f;
    private const int RarityAltitudeStep = 5;

    [SerializeField] private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;
    private Vector3 baseScale = Vector3.one;
    private GameObject attachedPickupInstance;

    protected virtual float GrassPickupChance => grassPickupChance;
    protected virtual float CoinPickupChance => coinPickupChance;
    protected virtual float TreasureChestChance => treasureChestChance;
    protected virtual bool CanSpawnGrassPickup => true;

    protected virtual void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        baseScale = transform.localScale;

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        ResolvePickupPrefabs();
    }

    protected override void OnDisable()
    {
        ClearAttachedPickup();
        base.OnDisable();
    }

    public override void Initialize(Vector2Int gridCoordinate, PlatformKind kind, bool isMainPath)
    {
        ClearAttachedPickup();
        base.Initialize(gridCoordinate, kind, isMainPath);
        transform.localScale = baseScale * (isMainPath ? 1.03f : 0.94f);
        ApplyKindVisual(TileType, isMainPath);
        TrySpawnPickup();
    }

    public override void Recycle()
    {
        ClearAttachedPickup();
        base.Recycle();
    }

    protected virtual void TrySpawnPickup()
    {
        if (Kind == PlatformKind.Hazard || Kind == PlatformKind.Gap)
        {
            return;
        }

        float roll = Random.value;
        bool spawningChest = false;

        if (roll < TreasureChestChance)
        {
            spawningChest = true;
        }
        // Only spawn coin/grass if not spawning a chest
        else if (roll < TreasureChestChance + CoinPickupChance)
        {
            SpawnPickupInstance(coinPickupPrefab, pickupYOffset, isCoin: true);
            return;
        }
        else if (CanSpawnGrassPickup && roll < TreasureChestChance + CoinPickupChance + GrassPickupChance)
        {
            SpawnPickupInstance(grassPickupPrefab, pickupYOffset, isCoin: false);
            return;
        }

        if (!spawningChest)
        {
            return;
        }

        // Select chest rarity based on weighted probability + altitude bonus
        TreasureChestPickup.ChestRarity selectedRarity = PickChestRarity();
        GameObject chestPrefab = GetChestPrefabForRarity(selectedRarity);
        if (chestPrefab == null)
        {
            return;
        }

        SpawnPickupInstance(chestPrefab, treasureChestYOffset, isCoin: false, isTreasureChest: true, selectedRarity);
    }

    private void SpawnPickupInstance(GameObject pickupPrefab, float yOffset, bool isCoin, bool isTreasureChest = false, TreasureChestPickup.ChestRarity chestRarity = TreasureChestPickup.ChestRarity.Common)
    {
        if (pickupPrefab == null)
        {
#if UNITY_EDITOR
            if (isTreasureChest) Debug.LogWarning($"[SafePlatform] Chest prefab is null for rarity {chestRarity} at grid {GridPosition}");
#endif
            return;
        }

        attachedPickupInstance = Instantiate(pickupPrefab, transform);
        attachedPickupInstance.transform.localPosition = new Vector3(0f, yOffset, 0f);
        attachedPickupInstance.transform.localRotation = Quaternion.identity;

        Collider pickupCollider = attachedPickupInstance.GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }

        Collider2D pickupCollider2D = attachedPickupInstance.GetComponent<Collider2D>();
        if (pickupCollider2D != null)
        {
            pickupCollider2D.isTrigger = true;
        }

        if (isCoin && attachedPickupInstance.GetComponent<CoinPickup>() == null)
        {
            attachedPickupInstance.AddComponent<CoinPickup>();
        }

        if (isTreasureChest)
        {
            TreasureChestPickup chestComponent = attachedPickupInstance.GetComponent<TreasureChestPickup>();
            if (chestComponent == null)
            {
                if (attachedPickupInstance.GetComponent<Collider>() == null)
                {
                    BoxCollider box = attachedPickupInstance.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.center = Vector3.zero;
                    box.size = Vector3.one;
                }
                chestComponent = attachedPickupInstance.AddComponent<TreasureChestPickup>();
            }

            chestComponent.Initialize(chestRarity);
#if UNITY_EDITOR
            Debug.Log($"[SafePlatform] Spawned {chestRarity} chest at grid {GridPosition}, world {attachedPickupInstance.transform.position}");
#endif
        }

        CoinPickup coinPickup = attachedPickupInstance.GetComponent<CoinPickup>();
        if (coinPickup != null)
        {
            coinPickup.ResetAnimationOrigin();
        }
    }

    private TreasureChestPickup.ChestRarity PickChestRarity()
    {
        // Altitude bonus: higher Y = better chest odds
        float altitudeBonus = (GridPosition.y / (float)RarityAltitudeStep) * RarityAltitudeScale;

        float roll = Random.value;
        float cumulative = 0f;

        for (int i = ChestRarityWeights.Length - 1; i >= 0; i--)
        {
            float adjustedWeight = ChestRarityWeights[i];
            if (i >= 2) // Epic and Legendary get altitude bonus
            {
                adjustedWeight += altitudeBonus;
            }
            else if (i == 0) // Common weight decreases at altitude
            {
                adjustedWeight -= altitudeBonus * 0.5f;
                if (adjustedWeight < 0.1f) adjustedWeight = 0.1f;
            }

            cumulative += adjustedWeight;
        }

        // Normalize and pick
        if (cumulative <= 0f) return TreasureChestPickup.ChestRarity.Common;

        float normalizedRoll = roll * cumulative;
        float runningTotal = 0f;

        for (int i = ChestRarityWeights.Length - 1; i >= 0; i--)
        {
            float adjustedWeight = ChestRarityWeights[i];
            if (i >= 2)
            {
                adjustedWeight += altitudeBonus;
            }
            else if (i == 0)
            {
                adjustedWeight -= altitudeBonus * 0.5f;
                if (adjustedWeight < 0.1f) adjustedWeight = 0.1f;
            }

            runningTotal += adjustedWeight;
            if (normalizedRoll <= runningTotal)
            {
                return (TreasureChestPickup.ChestRarity)i;
            }
        }

        return TreasureChestPickup.ChestRarity.Common;
    }

    private GameObject GetChestPrefabForRarity(TreasureChestPickup.ChestRarity rarity)
    {
        int index = (int)rarity;
        if (treasureChestPrefabs != null && index < treasureChestPrefabs.Length && treasureChestPrefabs[index] != null)
        {
            return treasureChestPrefabs[index];
        }

        return ResolveChestPrefabFallback(index);
    }

    private GameObject ResolveChestPrefabFallback(int index)
    {
#if UNITY_EDITOR
        string path = $"Assets/TreasureChest/Prefabs/TreasureChest_{index}.prefab";
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
#else
        return null;
#endif
    }

    protected void ClearAttachedPickup()
    {
        if (attachedPickupInstance != null)
        {
            Destroy(attachedPickupInstance);
            attachedPickupInstance = null;
        }
    }

    private void ResolvePickupPrefabs()
    {
#if UNITY_EDITOR
        if (grassPickupPrefab == null)
        {
            grassPickupPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GrassPickup.prefab");
        }

        if (coinPickupPrefab == null)
        {
            coinPickupPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coin.prefab");
        }

        if (treasureChestPrefabs == null || treasureChestPrefabs.Length == 0)
        {
            treasureChestPrefabs = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                treasureChestPrefabs[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/TreasureChest/Prefabs/TreasureChest_{i}.prefab");
            }
        }
        else
        {
            // Fill any null slots
            for (int i = 0; i < treasureChestPrefabs.Length && i < 4; i++)
            {
                if (treasureChestPrefabs[i] == null)
                {
                    treasureChestPrefabs[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/TreasureChest/Prefabs/TreasureChest_{i}.prefab");
                }
            }
        }
#endif
    }

    protected void ApplyKindVisual(TileType kind, bool isMainPath)
    {
        if (renderers == null)
        {
            return;
        }

        Color color = kind switch
        {
            TileType.NormalTile => isMainPath ? new Color(0.62f, 0.58f, 0.52f) : new Color(0.48f, 0.45f, 0.40f),
            TileType.CrumbleTile => isMainPath ? new Color(0.70f, 0.62f, 0.54f) : new Color(0.56f, 0.49f, 0.42f),
            TileType.SpringTile => isMainPath ? new Color(0.66f, 0.74f, 0.78f) : new Color(0.50f, 0.58f, 0.62f),
            TileType.HazardTile => new Color(0.28f, 0.24f, 0.22f),
            TileType.CoinTile => new Color(0.92f, 0.82f, 0.34f),
            TileType.EmptyGap => new Color(0.1f, 0.1f, 0.1f, 0.15f),
            _ => Color.white
        };

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
