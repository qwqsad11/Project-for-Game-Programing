using UnityEngine;

public class SafePlatform : Tile
{
    [Header("Collectibles")]
    [SerializeField, Range(0f, 1f)] private float grassPickupChance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float coinPickupChance = 0.1f;
    [SerializeField] private float pickupYOffset = 0.65f;
    [SerializeField] private GameObject grassPickupPrefab;
    [SerializeField] private GameObject coinPickupPrefab;

    [SerializeField] private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;
    private Vector3 baseScale = Vector3.one;
    private GameObject attachedPickupInstance;

    private void Awake()
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

    private void TrySpawnPickup()
    {
        if (Kind == PlatformKind.Hazard || Kind == PlatformKind.Gap)
        {
            return;
        }

        float roll = Random.value;
        GameObject pickupPrefab = null;

        if (roll < coinPickupChance)
        {
            pickupPrefab = coinPickupPrefab;
        }
        else if (roll < coinPickupChance + grassPickupChance)
        {
            pickupPrefab = grassPickupPrefab;
        }

        if (pickupPrefab == null)
        {
            return;
        }

        attachedPickupInstance = Instantiate(pickupPrefab, transform);
        attachedPickupInstance.transform.localPosition = new Vector3(0f, pickupYOffset, 0f);
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
    }

    private void ClearAttachedPickup()
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
#endif
    }

    private void ApplyKindVisual(TileType kind, bool isMainPath)
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
