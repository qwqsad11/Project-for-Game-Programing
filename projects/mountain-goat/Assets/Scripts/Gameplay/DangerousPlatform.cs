using System.Collections;
using UnityEngine;

public class DangerousPlatform : SafePlatform
{
    [Header("Danger")]
    [SerializeField] private float collapseDelay = 1f;
    [SerializeField] private float crumbleDuration = 0.18f;
    [SerializeField] private float fallDistance = 0.45f;
    [SerializeField, Range(0f, 1f)] private float dangerousCoinChance = 0.55f;
    [SerializeField, Range(0f, 1f)] private float dangerousTreasureChance = 0.01f;

    private bool collapsing;

    protected override float GrassPickupChance => 0f;
    protected override float CoinPickupChance => dangerousCoinChance;
    protected override float TreasureChestChance => dangerousTreasureChance;
    protected override bool CanSpawnGrassPickup => false;

    public override void Initialize(Vector2Int gridCoordinate, PlatformKind kind, bool isMainPath)
    {
        collapsing = false;
        Collider platformCollider = GetComponent<Collider>();
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }

        base.Initialize(gridCoordinate, PlatformKind.Crumble, isMainPath);
    }

    public override void OnPlayerLanded(GoatController goat)
    {
        StartCollapse(goat);
    }

    public override void OnPlayerLanded(GoatMovement goat)
    {
        StartCollapse(goat);
    }

    public override void OnPlayerLanded(PlayerController player)
    {
        GoatController goat = player != null ? player.GetComponent<GoatController>() : null;
        StartCollapse(goat);
    }

    private void StartCollapse(GoatController goat)
    {
        if (goat == null || collapsing)
        {
            return;
        }

        StartCoroutine(CollapseRoutine(goat));
    }

    public override void Recycle()
    {
        collapsing = false;
        base.Recycle();
    }

    protected override void OnDisable()
    {
        collapsing = false;
        base.OnDisable();
    }

    private IEnumerator CollapseRoutine(GoatController goat)
    {
        collapsing = true;
        Vector2Int collapseGrid = GridPosition;
        yield return new WaitForSeconds(collapseDelay);
        bool goatStillOnPlatform = goat != null && goat.CurrentGrid == collapseGrid;

        Collider platformCollider = GetComponent<Collider>();
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterTile(this);
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * fallDistance;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 0.15f;

        for (float timer = 0f; timer < crumbleDuration; timer += Time.deltaTime)
        {
            float t = Mathf.Clamp01(timer / crumbleDuration);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        if (goatStillOnPlatform)
        {
            goat.ForceDie();
        }

        gameObject.SetActive(false);
    }
}
