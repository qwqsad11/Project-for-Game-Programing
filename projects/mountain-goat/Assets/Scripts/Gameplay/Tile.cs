using System.Collections;
using UnityEngine;

public enum TileType
{
    NormalTile = 0,
    CrumbleTile = 1,
    SpringTile = 2,
    HazardTile = 3,
    CoinTile = 4,
    EmptyGap = 5
}

public enum PlatformKind
{
    Grass = (int)TileType.NormalTile,
    Crumble = (int)TileType.CrumbleTile,
    Spring = (int)TileType.SpringTile,
    Hazard = (int)TileType.HazardTile,
    Coin = (int)TileType.CoinTile,
    Gap = (int)TileType.EmptyGap,
    Obstacle = Hazard,
    Bonus = Coin
}

[DisallowMultipleComponent]
public class Tile : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private PlatformKind kind = PlatformKind.Grass;
    [SerializeField] private bool isMainPath;
    [SerializeField] private float crumbleDelay = 0.35f;

    public Vector2Int GridPosition => gridPosition;
    public PlatformKind Kind => kind;
    public TileType TileType => ToTileType(kind);
    public bool IsMainPath => isMainPath;

    private bool isCrumbling;

    public virtual void Initialize(Vector2Int coordinate, TileType tileType, bool mainPath)
    {
        Initialize(coordinate, ToLegacyKind(tileType), mainPath);
    }

    public virtual void Initialize(Vector2Int coordinate, PlatformKind tileKind, bool mainPath)
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterTile(this);
        }

        gridPosition = coordinate;
        kind = tileKind;
        isMainPath = mainPath;

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RegisterTile(this);
        }
    }

    public virtual void Recycle()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterTile(this);
        }
    }

    public virtual void OnPlayerLanded(PlayerController player)
    {
        ApplyLanding(player != null ? player.GetComponent<GoatController>() : null);
    }

    public virtual void OnPlayerLanded(GoatController goat)
    {
        ApplyLanding(goat);
    }

    private void ApplyLanding(GoatController goat)
    {
        if (goat == null)
        {
            return;
        }

        switch (TileType)
        {
            case TileType.HazardTile:
                if (goat.CanDie)
                {
                    goat.Die();
                }
                break;
            case TileType.CoinTile:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddCoin(1);
                }
                break;
            case TileType.SpringTile:
                goat.ApplySpringBoost();
                break;
            case TileType.CrumbleTile:
                if (!isCrumbling)
                {
                    StartCoroutine(CrumbleRoutine());
                }
                break;
        }
    }

    protected virtual void OnDisable()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterTile(this);
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

    private static TileType ToTileType(PlatformKind platformKind)
    {
        return platformKind switch
        {
            PlatformKind.Crumble => TileType.CrumbleTile,
            PlatformKind.Spring => TileType.SpringTile,
            PlatformKind.Hazard => TileType.HazardTile,
            PlatformKind.Coin => TileType.CoinTile,
            PlatformKind.Gap => TileType.EmptyGap,
            _ => TileType.NormalTile
        };
    }

    private IEnumerator CrumbleRoutine()
    {
        isCrumbling = true;
        yield return new WaitForSeconds(crumbleDelay);

        Recycle();
        gameObject.SetActive(false);
        isCrumbling = false;
    }
}
