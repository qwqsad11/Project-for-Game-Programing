using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoatController : MonoBehaviour
{
    public static readonly Vector2Int UpperLeft = new Vector2Int(-1, 1);
    public static readonly Vector2Int UpperRight = new Vector2Int(1, 1);
    public static readonly Vector2Int LowerLeft = new Vector2Int(-1, -1);
    public static readonly Vector2Int LowerRight = new Vector2Int(1, -1);

    [Header("Grid")]
    [SerializeField] private Vector2Int startGrid = Vector2Int.zero;
    [SerializeField] private Vector2Int currentGridPosition;
    [SerializeField] private Vector2Int targetGridPosition;

    [Header("Jump")]
    [SerializeField] private float jumpDuration = 0.18f;
    [SerializeField] private float jumpHeight = 0.95f;
    [SerializeField] private float squashAmount = 0.12f;
    [SerializeField] private float stretchAmount = 0.12f;
    [SerializeField] private float springJumpMultiplier = 1.35f;

    [Header("State")]
    [SerializeField] private float fallDeathY = -8f;
    [SerializeField] private float spawnProtectionDuration = 0.35f;
    [SerializeField] private float worldYOffset = 0.45f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private CameraFollow cameraFollow;

    private GridManager gridManager;
    private Coroutine jumpCoroutine;
    private bool isDead;
    private float spawnProtectionTimer;
    private Vector3 visualBaseScale;
    private float pendingJumpHeightMultiplier = 1f;

    public Vector2Int CurrentGrid => currentGridPosition;
    public Vector2Int CurrentGridPosition => currentGridPosition;
    public Vector2Int TargetGridPosition => targetGridPosition;
    public bool IsDead => isDead;
    public bool IsJumping => isJumping;
    public bool CanDie => !isDead && spawnProtectionTimer <= 0f && !IsJumping;

    private bool isJumping;

    protected virtual void Awake()
    {
        gridManager = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        visualBaseScale = visualRoot != null ? visualRoot.localScale : transform.localScale;
        if (cameraFollow == null)
        {
            cameraFollow = FindObjectOfType<CameraFollow>();
        }
    }

    protected virtual void Start()
    {
        gridManager ??= GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        currentGridPosition = startGrid;
        targetGridPosition = currentGridPosition;

        if (gridManager != null)
        {
            currentGridPosition = gridManager.SnapToGrid(currentGridPosition);
            targetGridPosition = currentGridPosition;
            transform.position = GetWorldPosition(currentGridPosition);
        }
        else
        {
            transform.position = GetWorldPosition(currentGridPosition);
        }

        spawnProtectionTimer = spawnProtectionDuration;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing || isDead)
        {
            return;
        }

        if (spawnProtectionTimer > 0f)
        {
            spawnProtectionTimer -= Time.deltaTime;
        }

        if (!IsJumping && transform.position.y < fallDeathY && CanDie)
        {
            Die();
            return;
        }

        if (!IsJumping)
        {
            HandleInput();
        }
    }

    public void MoveUpperLeft() => TryMove(UpperLeft);
    public void MoveUpperRight() => TryMove(UpperRight);
    public void MoveLowerLeft() => TryMove(LowerLeft);
    public void MoveLowerRight() => TryMove(LowerRight);

    public void MoveLeft() => TryMove(UpperLeft);
    public void MoveRight() => TryMove(UpperRight);
    public void MoveUp() => TryMove(UpperRight);
    public void MoveDown() => TryMove(LowerLeft);

    public void TryMove(Vector2Int delta)
    {
        if (isDead || IsJumping || delta == Vector2Int.zero)
        {
            return;
        }

        if (!GridManager.IsDiagonalStep(delta))
        {
            return;
        }

        Vector2Int candidateGrid = currentGridPosition + delta;

        if (gridManager != null && !gridManager.HasTile(candidateGrid))
        {
            return;
        }

        targetGridPosition = candidateGrid;
        StartJumpRoutine();

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
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

    public void QueueSpringHop()
    {
        ApplySpringBoost();
    }

    public void ApplySpringBoost()
    {
        pendingJumpHeightMultiplier = Mathf.Max(pendingJumpHeightMultiplier, springJumpMultiplier);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryMove(UpperLeft);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryMove(UpperRight);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            TryMove(LowerLeft);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            TryMove(LowerRight);
        }
    }

    private void StartJumpRoutine()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
        }

        isJumping = true;
        jumpCoroutine = StartCoroutine(JumpToGridRoutine());
    }

    private IEnumerator JumpToGridRoutine()
    {
        isJumping = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = GetWorldPosition(targetGridPosition);
        float appliedJumpHeightMultiplier = pendingJumpHeightMultiplier;
        pendingJumpHeightMultiplier = 1f;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight * appliedJumpHeightMultiplier;

            Vector3 position = Vector3.Lerp(startPosition, endPosition, t);
            position.y += arc;
            transform.position = position;

            if (visualRoot != null)
            {
                float squashPhase = Mathf.Sin(t * Mathf.PI);
                float squash = 1f - (squashAmount * squashPhase);
                float stretch = 1f + (stretchAmount * squashPhase);
                visualRoot.localScale = new Vector3(
                    visualBaseScale.x * squash,
                    visualBaseScale.y * stretch,
                    visualBaseScale.z * squash);
            }

            yield return null;
        }

        transform.position = endPosition;

        if (visualRoot != null)
        {
            visualRoot.localScale = visualBaseScale;
        }

        currentGridPosition = targetGridPosition;
        isJumping = false;
        jumpCoroutine = null;

        if (cameraFollow != null)
        {
            cameraFollow.TriggerShake();
        }

        if (gridManager != null && gridManager.TryGetTile(currentGridPosition, out Tile tile))
        {
            tile.OnPlayerLanded(this);
        }

        if (!isDead && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(1);
        }
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        Vector3 world = gridManager != null
            ? gridManager.GridToWorld(gridPosition)
            : IsoGrid.ToWorld(gridPosition, 1.5f, 0.75f, 0.5f);
        world.y += worldYOffset;
        return world;
    }
}
