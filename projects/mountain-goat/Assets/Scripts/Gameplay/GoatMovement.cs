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

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private LevelGenerator levelGenerator;

    private JumpController jumpController;
    private bool isDead;
    private bool isMoving;
    private float spawnProtectionTimer;
    private float pendingJumpMultiplier = 1f;

    public bool IsMoving => isMoving;

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
            animator.SetBool("isJump", false);
            animator.SetBool("isDead", false);
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

        if (!isMoving && CanDie && transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        if (!isMoving)
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
        if (isDead || isMoving)
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

        bool moveOnZAxis = delta.y != 0;
        if (animator != null)
        {
            animator.SetBool("isJump", moveOnZAxis);
        }

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

        if (animator != null)
        {
            animator.SetBool("isJump", false);
        }

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

        if (animator != null)
        {
            animator.SetBool("isJump", false);
            animator.SetBool("isDead", true);
        }

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
