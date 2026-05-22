using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(JumpController))]
[RequireComponent(typeof(Collider))]
public class GoatMovement : GoatController
{
    [Header("Grid")]
    [SerializeField] private float tileWidth = 1.5f;
    [SerializeField] private float tileDepth = 0.75f;
    [SerializeField] private float tileHeight = 0.5f;
    [SerializeField] private Vector2Int startGrid = Vector2Int.zero;
    [SerializeField] private Vector2Int minGrid = new Vector2Int(-2, 0);
    [SerializeField] private Vector2Int maxGrid = new Vector2Int(2, 99999);

    [Header("Jump")]
    [SerializeField] private float jumpDuration = 0.15f;
    [SerializeField] private float jumpHeight = 0.85f;
    [SerializeField] private float squashAmount = 0.12f;
    [SerializeField] private float stretchAmount = 0.12f;
    [SerializeField] private float springJumpMultiplier = 1.35f;

    [Header("State")]
    [SerializeField] private float fallDeathY = -8f;
    [SerializeField] private float spawnProtectionDuration = 0.5f;

    [Header("Platform Check")]
    [SerializeField] private bool requirePlatformToMove = true;
    [SerializeField] private bool fallIfNoPlatform = true;

    [Header("Treasure Chests")]
    [SerializeField] private bool attackAdjacentTreasureChests = true;
    [SerializeField] private float treasureAttackDuration = 0.35f;
    [SerializeField] private float treasureAttackLeanDistance = 0.18f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private LevelGenerator levelGenerator;

    private JumpController jumpController;
    private bool isDead;
    private bool isMoving;
    private bool isAttacking;
    private float spawnProtectionTimer;
    private float pendingJumpMultiplier = 1f;

    public bool IsMoving => isMoving || isAttacking;

    public override bool CanDie => !isDead && spawnProtectionTimer <= 0f && !isMoving;

    private void Awake()
    {
        jumpController = GetComponent<JumpController>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (levelGenerator == null)
        {
            levelGenerator = FindObjectOfType<LevelGenerator>();
        }
    }

    private void Start()
    {
        if (GridManager.Instance != null)
        {
            currentGrid = ClampGrid(GridManager.Instance.WorldToGrid(transform.position));
        }
        else
        {
            currentGrid = ClampGrid(startGrid);
        }

        transform.position = GetWorldPosition(currentGrid);
        spawnProtectionTimer = spawnProtectionDuration;

        if (animator != null)
        {
            SetAnimatorBool("isJump", false);
            SetAnimatorBool("isDead", false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing || isDead)
        {
            return;
        }

        if (spawnProtectionTimer > 0f)
        {
            spawnProtectionTimer -= Time.deltaTime;
        }

        if (!IsMoving && CanDie && transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        if (!IsMoving)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Vector2Int delta = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            delta = UpperLeft;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            delta = new Vector2Int(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            delta = new Vector2Int(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            delta = LowerRight;
        }

        if (delta != Vector2Int.zero)
        {
            TryMove(delta);
        }
    }

    public void TryMove(Vector2Int delta)
    {
        if (isDead || IsMoving)
        {
            return;
        }

        Vector2Int targetGrid = ClampGrid(currentGrid + delta);
        if (targetGrid == currentGrid)
        {
            return;
        }

        EnsureLevelGenerator();
        if (levelGenerator != null)
        {
            levelGenerator.RequestAheadForJump(currentGrid, targetGrid);
            levelGenerator.EnsurePlatformAt(targetGrid);
        }

        if (requirePlatformToMove && GridManager.Instance != null && !GridManager.Instance.HasPlatform(targetGrid))
        {
            if (fallIfNoPlatform)
            {
                Die();
            }

            return;
        }

        Vector3 start = transform.position;
        Vector3 end = GetWorldPosition(targetGrid);
        Vector3 flatDirection = end - start;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        }

        currentGrid = targetGrid;
        isMoving = true;

        SetAnimatorBool("isJump", true);

        StartCoroutine(jumpController.JumpRoutine(
            transform,
            start,
            end,
            jumpDuration,
            jumpHeight * pendingJumpMultiplier,
            squashAmount,
            stretchAmount,
            OnLanded));

        pendingJumpMultiplier = 1f;
    }

    private void OnLanded()
    {
        isMoving = false;

        SetAnimatorBool("isJump", false);

        if (GridManager.Instance != null && GridManager.Instance.TryGetTile(currentGrid, out Tile tile))
        {
            tile.OnPlayerLanded(this);
        }

        EnsureLevelGenerator();
        if (levelGenerator != null)
        {
            levelGenerator.NotifyLanded(currentGrid);
        }

        TryAttackNearbyTreasureChest();
    }

    public override void Die()
    {
        if (!CanDie)
        {
            return;
        }

        isDead = true;
        isMoving = false;
        isAttacking = false;

        SetAnimatorBool("isJump", false);
        SetAnimatorBool("isAttack", false);
        SetAnimatorBool("isDead", true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public override void ApplySpringBoost()
    {
        pendingJumpMultiplier = springJumpMultiplier;
    }

    public override void QueueSpringHop()
    {
        ApplySpringBoost();
    }

    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator != null ? newAnimator : GetComponent<Animator>();
        SetAnimatorBool("isJump", false);
        SetAnimatorBool("isAttack", false);
        SetAnimatorBool("isDead", false);
    }

    private void TryAttackNearbyTreasureChest()
    {
        if (!attackAdjacentTreasureChests || isDead || isAttacking || GridManager.Instance == null)
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

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2Int grid = currentGrid + new Vector2Int(x, y);
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
        }

        return bestChest;
    }

    private IEnumerator AttackTreasureChestRoutine(TreasureChestPickup chest)
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

        SetAnimatorBool("isAttack", true);
        SetAnimatorTrigger("Attack");
        SetAnimatorTrigger("attack");

        Vector3 leanPosition = originalPosition + chestDirection.normalized * treasureAttackLeanDistance;
        float halfDuration = Mathf.Max(0.05f, treasureAttackDuration * 0.5f);

        for (float timer = 0f; timer < halfDuration; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(originalPosition, leanPosition, timer / halfDuration);
            yield return null;
        }

        if (chest != null)
        {
            chest.TryOpen(this);
        }

        for (float timer = 0f; timer < halfDuration; timer += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(leanPosition, originalPosition, timer / halfDuration);
            yield return null;
        }

        transform.position = originalPosition;
        SetAnimatorBool("isAttack", false);
        isAttacking = false;
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator == null || !HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            return;
        }

        animator.SetBool(parameterName, value);
    }

    private void SetAnimatorTrigger(string parameterName)
    {
        if (animator == null || !HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Trigger))
        {
            return;
        }

        animator.SetTrigger(parameterName);
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter.type == parameterType && parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 GetWorldPosition(Vector2Int grid)
    {
        if (GridManager.Instance != null)
        {
            return GridManager.Instance.GridToWorld(grid);
        }

        return IsoGrid.ToWorld(grid, tileWidth, tileDepth, tileHeight);
    }

    private void EnsureLevelGenerator()
    {
        if (levelGenerator == null)
        {
            levelGenerator = FindObjectOfType<LevelGenerator>();
        }
    }

    private Vector2Int ClampGrid(Vector2Int grid)
    {
        return new Vector2Int(
            grid.x,
            Mathf.Clamp(grid.y, minGrid.y, maxGrid.y));
    }
}
