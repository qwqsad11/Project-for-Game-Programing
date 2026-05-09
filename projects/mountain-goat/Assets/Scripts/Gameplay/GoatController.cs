using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(HungerSystem))]
public class GoatController : MonoBehaviour
{
    [Header("Grid Movement")]
    [FormerlySerializedAs("diagonalStep")]
    [SerializeField] private float laneOffset = 1.5f;
    [FormerlySerializedAs("diagonalStep")]
    [SerializeField] private float forwardStep = 1.5f;
    [SerializeField] private float heightStep = 0.5f;
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private float jumpHeight = 0.8f;
    [SerializeField] private int startLane = 0;
    [SerializeField] private int minRow = 0;

    [Header("Bounds")]
    [SerializeField] private int leftLaneLimit = -1;
    [SerializeField] private int rightLaneLimit = 1;
    [SerializeField] private float fallDeathY = -5f;
    [SerializeField] private float spawnProtectionDuration = 0.5f;

    [Header("Optional References")]
    [SerializeField] private Animator animator;

    private HungerSystem hungerSystem;
    private bool isMoving;
    private bool isDead;
    private int currentLane;
    private int currentRow;
    private float spawnProtectionTimer;

    public int CurrentLane => currentLane;
    public int CurrentRow => currentRow;
    public bool IsDead => isDead;
    public bool CanDie => !isDead && spawnProtectionTimer <= 0f;

    private void Start()
    {
        hungerSystem = GetComponent<HungerSystem>();
        currentLane = Mathf.Clamp(startLane, leftLaneLimit, rightLaneLimit);
        currentRow = Mathf.Max(minRow, Mathf.RoundToInt(transform.position.z / forwardStep));
        transform.position = GetWorldPosition(currentLane, currentRow);
        if (hungerSystem != null)
        {
            hungerSystem.ResetForNewRun();
        }

        spawnProtectionTimer = spawnProtectionDuration;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing || isDead)
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

        if (!isMoving)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        bool moveLeftPressed = Input.GetKeyDown(KeyCode.Q);
        bool moveForwardPressed = Input.GetKeyDown(KeyCode.E);
        bool moveBackwardPressed = Input.GetKeyDown(KeyCode.A);
        bool moveRightPressed = Input.GetKeyDown(KeyCode.D);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (touch.position.x < Screen.width * 0.5f)
                {
                    moveLeftPressed = true;
                }
                else
                {
                    moveRightPressed = true;
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < Screen.width * 0.5f)
            {
                moveLeftPressed = true;
            }
            else
            {
                moveRightPressed = true;
            }
        }

        if (moveLeftPressed)
        {
            TryMove(-1, 0);
        }
        else if (moveForwardPressed)
        {
            TryMove(0, 1);
        }
        else if (moveBackwardPressed)
        {
            TryMove(0, -1);
        }
        else if (moveRightPressed)
        {
            TryMove(1, 0);
        }
    }

    private void TryMove(int laneDelta, int rowDelta)
    {
        int targetLane = Mathf.Clamp(currentLane + laneDelta, leftLaneLimit, rightLaneLimit);
        int targetRow = currentRow + rowDelta;

        if ((targetLane == currentLane && targetRow == currentRow) || targetRow < minRow)
        {
            return;
        }

        currentLane = targetLane;
        currentRow = targetRow;
        Vector3 targetPosition = GetWorldPosition(currentLane, currentRow);
        StartCoroutine(MoveRoutine(targetPosition));
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

    private IEnumerator MoveRoutine(Vector3 targetPosition)
    {
        isMoving = true;

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / moveDuration);
            float arc = 4f * jumpHeight * progress * (1f - progress);

            Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            nextPosition.y = startPosition.y + arc;
            transform.position = nextPosition;

            yield return null;
        }

        transform.position = targetPosition;
        if (targetPosition.z > startPosition.z)
        {
            GameManager.Instance.AddScore(1);
        }
        isMoving = false;
    }

    private Vector3 GetWorldPosition(int lane, int row)
    {
        return new Vector3(lane * laneOffset, row * heightStep, row * forwardStep);
    }
}
