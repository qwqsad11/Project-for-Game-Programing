using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(JumpController))]
public class PlayerController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private float tileWidth = 1.5f;
    [SerializeField] private float tileDepth = 1.5f;
    [SerializeField] private float tileHeight = 0.5f;
    [SerializeField] private Vector2Int startGrid = Vector2Int.zero;
    [SerializeField] private Vector2Int minGrid = new Vector2Int(-2, 0);
    [SerializeField] private Vector2Int maxGrid = new Vector2Int(2, 99999);

    [Header("Jump")]
    [SerializeField] private float jumpDuration = 0.3f;
    [SerializeField] private float jumpHeight = 0.85f;
    [SerializeField] private float squashAmount = 0.12f;
    [SerializeField] private float stretchAmount = 0.12f;

    [Header("State")]
    [SerializeField] private float fallDeathY = -8f;
    [SerializeField] private float spawnProtectionDuration = 0.5f;

    [Header("Platform Check")]
    [SerializeField] private bool requirePlatformToMove = true;
    [SerializeField] private bool fallIfNoPlatform = true;

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Treasure Chests")]
    [SerializeField] private bool attackAdjacentTreasureChests = true;
    [SerializeField] private float treasureAttackDuration = 0.35f;
    [SerializeField] private float treasureAttackLeanDistance = 0.18f;

    private JumpController jumpController;
    private bool isDead;
    private float spawnProtectionTimer;
    private Vector2Int currentGrid;
    private bool useLegacyControl = true;
    private bool isAttacking;

    public Vector2Int CurrentGrid => currentGrid;
    public bool IsDead => isDead;
    public bool CanDie => !isDead && spawnProtectionTimer <= 0f && (jumpController == null || !jumpController.IsJumping);
    public bool IsMoving => (jumpController != null && jumpController.IsJumping) || isAttacking;

    private void Awake()
    {
        useLegacyControl = GetComponent<GoatController>() == null;
        jumpController = GetComponent<JumpController>();
    }

    private void Start()
    {
        currentGrid = ClampGrid(startGrid);
        transform.position = IsoGrid.ToWorld(currentGrid, tileWidth, tileDepth, tileHeight);
        spawnProtectionTimer = spawnProtectionDuration;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing || isDead)
        {
            return;
        }

        if (!useLegacyControl)
        {
            return;
        }

        if (spawnProtectionTimer > 0f)
        {
            spawnProtectionTimer -= Time.deltaTime;
        }

        if (CanDie && transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        if (jumpController == null || !jumpController.IsJumping)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (!useLegacyControl)
        {
            return;
        }

        Vector2Int delta = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryOpenAdjacentChest();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            delta = new Vector2Int(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            delta = new Vector2Int(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            delta = new Vector2Int(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            delta = new Vector2Int(1, 0);
        }

        if (delta != Vector2Int.zero)
        {
            TryMove(delta);
        }
    }

    public void TryMove(Vector2Int delta)
    {
        if (isDead || (jumpController != null && jumpController.IsJumping))
        {
            return;
        }

        Vector2Int targetGrid = ClampGrid(currentGrid + delta);
        if (targetGrid == currentGrid)
        {
            return;
        }

        if (requirePlatformToMove)
        {
            if (GridManager.Instance == null)
            {
                return;
            }

            if (!GridManager.Instance.HasPlatform(targetGrid))
            {
                if (fallIfNoPlatform)
                {
                    Die();
                }

                return;
            }
        }

        // Block movement onto tiles occupied by unopened treasure chests
        if (GridManager.Instance != null && GridManager.Instance.HasTreasureChest(targetGrid))
        {
            return;
        }

        Vector3 start = transform.position;
        Vector3 end = IsoGrid.ToWorld(targetGrid, tileWidth, tileDepth, tileHeight);
        currentGrid = targetGrid;

        StartCoroutine(jumpController.JumpRoutine(
            transform,
            start,
            end,
            jumpDuration,
            jumpHeight,
            squashAmount,
            stretchAmount,
            OnLanded));

        if (animator != null)
        {
            Vector3 flatDirection = new Vector3(end.x - start.x, 0f, end.z - start.z);
            transform.rotation = Quaternion.LookRotation(flatDirection.sqrMagnitude > 0.0001f ? flatDirection.normalized : transform.forward, Vector3.up);
            animator.SetTrigger("Jump");
        }
    }

    private void OnLanded()
    {
        if (GridManager.Instance != null && GridManager.Instance.TryGetTile(currentGrid, out Tile tile))
        {
            tile.OnPlayerLanded(this);
        }

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
        }
    }

    // All 8 grid-adjacent offsets (Chebyshev distance = 1).
    // Must cover diagonal neighbours because the movement system blocks
    // jumping onto chest tiles — the player needs to open from any side.
    private static readonly Vector2Int[] TreasureChestNeighborOffsets =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1)
    };

    private void TryOpenAdjacentChest()
    {
        if (!attackAdjacentTreasureChests || isDead || isAttacking || IsMoving || GridManager.Instance == null)
        {
            return;
        }

        TreasureChestPickup targetChest = FindNearbyTreasureChest();
        if (targetChest == null)
        {
            return;
        }

        StartCoroutine(AttackTreasureChestRoutine(targetChest));
    }

    private TreasureChestPickup FindNearbyTreasureChest()
    {
        TreasureChestPickup bestChest = null;
        float bestDistance = float.MaxValue;

        // Check adjacent tiles
        for (int offsetIndex = 0; offsetIndex < TreasureChestNeighborOffsets.Length; offsetIndex++)
        {
            Vector2Int grid = currentGrid + TreasureChestNeighborOffsets[offsetIndex];
            if (!GridManager.Instance.TryGetTile(grid, out Tile tile) || tile == null)
            {
                continue;
            }

            TreasureChestPickup[] chests = tile.GetComponentsInChildren<TreasureChestPickup>(true);
            for (int i = 0; i < chests.Length; i++)
            {
                TreasureChestPickup chest = chests[i];
                if (chest == null || chest.IsOpened)
                {
                    continue;
                }

                float distance = (chest.transform.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestChest = chest;
                }
            }
        }

        return bestChest;
    }

    private System.Collections.IEnumerator AttackTreasureChestRoutine(TreasureChestPickup chest)
    {
        if (chest == null)
        {
            yield break;
        }

        isAttacking = true;

        Vector3 originalPosition = transform.position;
        Vector3 chestDirection = chest.transform.position - transform.position;
        chestDirection.y = 0f;

        if (chestDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(chestDirection.normalized, Vector3.up);
        }

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Vector3 leanPosition = originalPosition + chestDirection.normalized * treasureAttackLeanDistance;
        float halfDuration = Mathf.Max(0.05f, treasureAttackDuration * 0.5f);

        for (float timer = 0f; timer < halfDuration; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(originalPosition, leanPosition, timer / halfDuration);
            yield return null;
        }

        if (chest != null)
        {
            chest.TryOpen(null);
        }

        for (float timer = 0f; timer < halfDuration; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(leanPosition, originalPosition, timer / halfDuration);
            yield return null;
        }

        transform.position = originalPosition;
        isAttacking = false;
    }

    public void Die()
    {
        if (!CanDie)
        {
            return;
        }

        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        GameManager.Instance.GameOver();
    }

    private Vector2Int ClampGrid(Vector2Int grid)
    {
        return new Vector2Int(
            grid.x,
            Mathf.Clamp(grid.y, minGrid.y, maxGrid.y));
    }
}
