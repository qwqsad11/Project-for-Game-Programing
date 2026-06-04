using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(JumpController))]
public class PlayerMovement : GoatController
{
    private enum PlayerAction
    {
        Idle,
        Jump,
        Death,
        Hit,
        Attack
    }

    [Header("Grid")]
    [SerializeField] private float tileWidth = 1.5f;
    [SerializeField] private float tileDepth = 0.75f;
    [SerializeField] private float tileHeight = 0.5f;
    [SerializeField] private Vector2Int startGrid = Vector2Int.zero;
    [SerializeField] private Vector2Int minGrid = new Vector2Int(-2, 0);
    [SerializeField] private Vector2Int maxGrid = new Vector2Int(2, 99999);

    [Header("Jump")]
    [SerializeField] private float jumpDuration = 0.3f;
    [SerializeField] private float jumpHeight = 0.85f;
    [SerializeField] private float squashAmount = 0.12f;
    [SerializeField] private float stretchAmount = 0.12f;
    [SerializeField] private float springJumpMultiplier = 1.35f;
    [SerializeField] private bool faceOppositeJumpDirection = true;
    [SerializeField] private bool playJumpWhenZChanges = true;
    [SerializeField] private float zChangeJumpThreshold = 0.01f;
    [SerializeField] private float zChangeJumpAnimationDuration = 0.4f;

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
    [SerializeField] private float hitReactionDuration = 0.35f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private LevelGenerator levelGenerator;

    [Header("Malbers Deer Animation States")]
    [SerializeField] private string[] idleStateNames = { "DIdle 1", "DIdle Look", "DIdle Scratch", "DIdle Head Shake" };
    [SerializeField] private string[] jumpStateNames = { "DJump Trot", "DJump Run" };
    [SerializeField] private string[] deathStateNames = { "DDeath Side" };
    [SerializeField] private string[] hitStateNames = { "DGetHit Front L", "DGetHit Front R", "DGetHit L", "DGetHit R", "DGetHit Back L", "DGetHit Back R" };
    [SerializeField] private string[] attackStateNames = { "DAttackFrontLegs", "DAttack Horns 1", "DAttack Horns 2", "DAttack Back Legs" };

    private JumpController jumpController;
    private bool isDead;
    private bool isMoving;
    private bool isAttacking;
    private float spawnProtectionTimer;
    private float pendingJumpMultiplier = 1f;
    private Coroutine hitRoutine;
    private Coroutine zChangeJumpRoutine;
    private float lastObservedZ;
    private static readonly Vector2Int[] TreasureChestNeighborOffsets =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public bool IsMoving => isMoving || isAttacking;
    public override bool CanDie => !isDead && spawnProtectionTimer <= 0f && !isMoving;

    private void Awake()
    {
        jumpController = GetComponent<JumpController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
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
        lastObservedZ = transform.position.z;
        spawnProtectionTimer = spawnProtectionDuration;

        if (animator != null)
        {
            SetAnimatorBool("isJump", false);
            SetAnimatorBool("isDead", false);
        }

        PlayAction(PlayerAction.Idle);
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

    private void LateUpdate()
    {
        TrackZAxisJumpAnimation();
    }

    private void HandleInput()
    {
        Vector2Int delta = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryOpenAdjacentChest();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            delta = UpperLeft;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            delta = UpperRight;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            delta = LowerLeft;
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
        FaceWorldPosition(end);

        currentGrid = targetGrid;
        isMoving = true;

        SetAnimatorBool("isJump", true);
        PlayAction(PlayerAction.Jump);

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
        StopZChangeJumpRoutine();
        PlayAction(PlayerAction.Idle);

        if (GridManager.Instance != null && GridManager.Instance.TryGetTile(currentGrid, out Tile tile))
        {
            tile.OnPlayerLanded(this);
        }

        EnsureLevelGenerator();
        if (levelGenerator != null)
        {
            levelGenerator.NotifyLanded(currentGrid);
        }
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
        StopHitRoutine();
        StopZChangeJumpRoutine();

        SetAnimatorBool("isJump", false);
        SetAnimatorBool("isDead", true);
        PlayAction(PlayerAction.Death);

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

    public override void TakeHit()
    {
        if (isDead)
        {
            return;
        }

        PlayAction(PlayerAction.Hit);
        StopHitRoutine();
        hitRoutine = StartCoroutine(HitRecoveryRoutine());
    }

    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator != null ? newAnimator : GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        StopZChangeJumpRoutine();
        PlayAction(PlayerAction.Idle);
    }

    private void TryOpenAdjacentChest()
    {
        if (!attackAdjacentTreasureChests || isDead || isAttacking || isMoving || GridManager.Instance == null)
        {
            return;
        }

        TreasureChestPickup targetChest = FindNearbyTreasureChest();
        if (targetChest != null)
        {
            StartCoroutine(AttackTreasureChestRoutine(targetChest));
        }
    }

    private void TryAttackNearbyTreasureChest()
    {
        if (!attackAdjacentTreasureChests || isDead || isAttacking || GridManager.Instance == null)
        {
            return;
        }

        TreasureChestPickup targetChest = FindNearbyTreasureChest();
        if (targetChest != null)
        {
            StartCoroutine(AttackTreasureChestRoutine(targetChest));
        }
    }

    private TreasureChestPickup FindNearbyTreasureChest()
    {
        TreasureChestPickup bestChest = null;
        float bestDistance = float.MaxValue;

        // First check the current tile (the one the player is standing on)
        if (GridManager.Instance.TryGetTile(currentGrid, out Tile currentTile) && currentTile != null)
        {
            TreasureChestPickup[] chests = currentTile.GetComponentsInChildren<TreasureChestPickup>(true);
            for (int i = 0; i < chests.Length; i++)
            {
                TreasureChestPickup chest = chests[i];
                if (chest != null && !chest.IsOpened)
                {
                    float distance = (chest.transform.position - transform.position).sqrMagnitude;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestChest = chest;
                    }
                }
            }
        }

        // Then check adjacent tiles
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

    private IEnumerator AttackTreasureChestRoutine(TreasureChestPickup chest)
    {
        isAttacking = true;
        StopHitRoutine();
        StopZChangeJumpRoutine();

        Vector3 originalPosition = transform.position;
        Vector3 chestDirection = chest.transform.position - transform.position;
        chestDirection.y = 0f;
        FaceWorldPosition(chest.transform.position);

        PlayAction(PlayerAction.Attack);

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
        isAttacking = false;

        if (!isDead)
        {
            PlayAction(PlayerAction.Idle);
        }
    }

    private IEnumerator HitRecoveryRoutine()
    {
        yield return new WaitForSeconds(hitReactionDuration);
        hitRoutine = null;

        if (!isDead && !isMoving && !isAttacking)
        {
            PlayAction(PlayerAction.Idle);
        }
    }

    private void TrackZAxisJumpAnimation()
    {
        float currentZ = transform.position.z;
        if (!playJumpWhenZChanges || isDead || isMoving || isAttacking)
        {
            lastObservedZ = currentZ;
            return;
        }

        if (Mathf.Abs(currentZ - lastObservedZ) > zChangeJumpThreshold)
        {
            PlayZChangeJumpAnimation();
        }

        lastObservedZ = currentZ;
    }

    private void PlayZChangeJumpAnimation()
    {
        StopHitRoutine();
        StopZChangeJumpRoutine();

        PlayAction(PlayerAction.Jump);
        zChangeJumpRoutine = StartCoroutine(ZChangeJumpRecoveryRoutine());
    }

    private IEnumerator ZChangeJumpRecoveryRoutine()
    {
        yield return new WaitForSeconds(zChangeJumpAnimationDuration);
        zChangeJumpRoutine = null;

        if (!isDead && !isMoving && !isAttacking)
        {
            PlayAction(PlayerAction.Idle);
        }
    }

    private void StopZChangeJumpRoutine()
    {
        if (zChangeJumpRoutine == null)
        {
            return;
        }

        StopCoroutine(zChangeJumpRoutine);
        zChangeJumpRoutine = null;
    }

    private void StopHitRoutine()
    {
        if (hitRoutine == null)
        {
            return;
        }

        StopCoroutine(hitRoutine);
        hitRoutine = null;
    }

    private void PlayAction(PlayerAction action)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        string[] stateNames = GetStateNames(action);
        for (int i = 0; i < stateNames.Length; i++)
        {
            string stateName = stateNames[i];
            if (!string.IsNullOrEmpty(stateName) && animator.HasState(0, Animator.StringToHash(stateName)))
            {
                animator.Play(stateName, 0, 0f);
                return;
            }
        }
    }

    private void FaceWorldPosition(Vector3 targetPosition)
    {
        Vector3 flatDirection = targetPosition - transform.position;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude > 0.0001f)
        {
            Vector3 lookDirection = faceOppositeJumpDirection ? -flatDirection.normalized : flatDirection.normalized;
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    private string[] GetStateNames(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.Jump:
                return jumpStateNames;
            case PlayerAction.Death:
                return deathStateNames;
            case PlayerAction.Hit:
                return hitStateNames;
            case PlayerAction.Attack:
                return attackStateNames;
            default:
                return idleStateNames;
        }
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator == null || !HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            return;
        }

        animator.SetBool(parameterName, value);
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
