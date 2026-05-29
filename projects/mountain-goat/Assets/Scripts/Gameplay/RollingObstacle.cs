using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RollingObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private Vector3 worldDirection = new Vector3(0f, -0.35f, -1f);
    [SerializeField] private bool useGameState = true;

    [Header("Rolling")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Ground Contact")]
    [SerializeField] private bool followGridSlope = true;
    [SerializeField] private float groundOffset = 0.45f;

    [Header("Lifetime")]
    [SerializeField] private float viewportDestroyMargin = 0.25f;
    [SerializeField] private float fallbackLifetime = 12f;

    private Camera mainCamera;
    private GridManager gridManager;
    private Vector2Int startGrid;
    private Vector2Int gridDirection = new Vector2Int(0, -1);
    private Vector3 gridStartWorld;
    private Vector3 gridStepWorld;
    private float gridProgress;
    private float aliveTimer;
    private bool launchedOnGridPath;

    private void Awake()
    {
        EnsureColliderAndRigidbody();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        gridManager = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        worldDirection = worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : Vector3.back;
    }

    private void Update()
    {
        if (useGameState && (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing))
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        aliveTimer += deltaTime;

        if (launchedOnGridPath)
        {
            MoveAlongGridPath(deltaTime);
        }
        else
        {
            transform.position += worldDirection * moveSpeed * deltaTime;
        }

        transform.Rotate(rotationAxis.normalized * rotationSpeed * deltaTime, Space.Self);

        if (ShouldDestroy())
        {
            Destroy(gameObject);
        }
    }

    public void LaunchOnGridPath(Vector2Int startGrid, Vector2Int direction, float speed)
    {
        gridManager = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            moveSpeed = speed;
            return;
        }

        gridDirection = direction == Vector2Int.zero ? new Vector2Int(0, -1) : direction;
        this.startGrid = startGrid;
        moveSpeed = speed;
        gridProgress = 0f;
        gridStartWorld = GetGroundedWorld(startGrid);
        gridStepWorld = GetGroundedWorld(startGrid + gridDirection) - gridStartWorld;
        worldDirection = gridStepWorld.sqrMagnitude > 0.0001f ? gridStepWorld.normalized : Vector3.back;
        launchedOnGridPath = true;
        transform.position = gridStartWorld;
    }

    private void MoveAlongGridPath(float deltaTime)
    {
        if (gridStepWorld.sqrMagnitude <= 0.0001f)
        {
            transform.position += worldDirection * moveSpeed * deltaTime;
            return;
        }

        gridProgress += moveSpeed * deltaTime / gridStepWorld.magnitude;
        Vector2Int currentGrid = Vector2Int.RoundToInt((Vector2)gridDirection * Mathf.Floor(gridProgress));
        float stepFraction = gridProgress - Mathf.Floor(gridProgress);
        Vector3 currentStart = gridStartWorld + gridStepWorld * Mathf.Floor(gridProgress);
        Vector3 currentEnd = currentStart + gridStepWorld;

        if (followGridSlope && gridManager != null)
        {
            Vector2Int baseGrid = startGrid + currentGrid;
            currentStart = GetGroundedWorld(baseGrid);
            currentEnd = GetGroundedWorld(baseGrid + gridDirection);
        }

        transform.position = Vector3.Lerp(currentStart, currentEnd, stepFraction);
    }

    private Vector3 GetGroundedWorld(Vector2Int grid)
    {
        Vector3 position = gridManager != null
            ? gridManager.GridToWorld(grid)
            : IsoGrid.ToWorld(grid, 1.5f, 1.5f, 0.5f);

        return position + Vector3.up * groundOffset;
    }

    private bool ShouldDestroy()
    {
        if (aliveTimer >= fallbackLifetime)
        {
            return true;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return false;
        }

        Vector3 viewport = mainCamera.WorldToViewportPoint(transform.position);
        return viewport.z > 0f && viewport.y < -viewportDestroyMargin;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKillGoat(other != null ? other.gameObject : null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryKillGoat(collision != null && collision.collider != null ? collision.collider.gameObject : null);
    }

    private void TryKillGoat(GameObject otherObject)
    {
        if (otherObject == null)
        {
            return;
        }

        GoatController goat = otherObject.GetComponentInParent<GoatController>() ??
                              otherObject.GetComponentInChildren<GoatController>() ??
                              otherObject.GetComponent<GoatController>();
        if (goat != null)
        {
            goat.ForceDie();
        }
    }

    private void EnsureColliderAndRigidbody()
    {
        Collider obstacleCollider = GetComponent<Collider>();
        if (obstacleCollider == null)
        {
            obstacleCollider = gameObject.AddComponent<BoxCollider>();
        }

        obstacleCollider.isTrigger = true;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
    }
}
