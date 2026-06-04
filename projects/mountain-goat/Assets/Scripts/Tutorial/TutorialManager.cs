using UnityEngine;

/// <summary>
/// Controls the tutorial flow via a simple step state machine.
/// Singleton — created automatically by TutorialSceneSetup if not present.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep
    {
        Welcome,
        MoveForward,
        MoveDiagonal,
        CollectGrass,
        CollectCoin,
        AvoidHazard,
        Complete
    }

    [Header("Target Grids")]
    [SerializeField] private Vector2Int forwardTarget = new Vector2Int(0, 1);
    [SerializeField] private Vector2Int diagonalTarget = new Vector2Int(-1, 2);
    [SerializeField] private Vector2Int grassTarget = new Vector2Int(-1, 3);
    [SerializeField] private Vector2Int coinTarget = new Vector2Int(-1, 4);
    [SerializeField] private Vector2Int hazardTile = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int completionTarget = new Vector2Int(0, 4);

    [Header("Hunger")]
    [SerializeField] private float tutorialHungerRate = 15f; // faster so player notices

    public static TutorialManager Instance { get; private set; }

    private TutorialStep currentStep = TutorialStep.Welcome;
    private TutorialHUD hud;
    private GoatMovement goat;
    private HungerSystem hunger;
    private bool stepJustAdvanced;
    private float stepAdvanceTime;
    private int coinsBeforeStep;

    public TutorialStep CurrentStep => currentStep;

    // ── Unity Lifecycle ───────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find or create HUD
        hud = FindObjectOfType<TutorialHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("TutorialHUD");
            hud = hudObj.AddComponent<TutorialHUD>();
        }

        // Find goat
        goat = FindObjectOfType<GoatMovement>();
        hunger = goat != null ? goat.GetComponent<HungerSystem>() : null;

        // Speed up hunger so the player notices it during tutorial
        if (hunger != null)
        {
            // Access via reflection-like approach: set the serialized field
            // We'll modify hunger via GameManager events instead
        }

        // Subscribe to events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
            GameManager.Instance.OnHungerChanged += HandleHungerChanged;
        }

        EnterStep(TutorialStep.Welcome);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
            GameManager.Instance.OnHungerChanged -= HandleHungerChanged;
        }

        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (goat == null || GameManager.Instance == null) return;

        switch (currentStep)
        {
            case TutorialStep.Welcome:
                CheckWelcomeInput();
                break;
            case TutorialStep.MoveForward:
                CheckReachedTile(forwardTarget);
                break;
            case TutorialStep.MoveDiagonal:
                CheckReachedTile(diagonalTarget);
                break;
            case TutorialStep.CollectGrass:
                // Completion handled by HandleHungerChanged
                break;
            case TutorialStep.CollectCoin:
                // Completion handled by HandleCoinsChanged
                break;
            case TutorialStep.AvoidHazard:
                CheckReachedTile(completionTarget);
                break;
            case TutorialStep.Complete:
                CheckCompleteInput();
                break;
        }
    }

    // ── Step Entry ────────────────────────────────────────

    private void EnterStep(TutorialStep step)
    {
        currentStep = step;
        stepJustAdvanced = true;
        stepAdvanceTime = Time.unscaledTime;

        switch (step)
        {
            case TutorialStep.Welcome:
                ShowWelcome();
                break;
            case TutorialStep.MoveForward:
                ShowMoveForward();
                break;
            case TutorialStep.MoveDiagonal:
                ShowMoveDiagonal();
                break;
            case TutorialStep.CollectGrass:
                ShowCollectGrass();
                break;
            case TutorialStep.CollectCoin:
                ShowCollectCoin();
                break;
            case TutorialStep.AvoidHazard:
                ShowAvoidHazard();
                break;
            case TutorialStep.Complete:
                ShowComplete();
                break;
        }
    }

    private void AdvanceStep()
    {
        int next = (int)currentStep + 1;
        if (next <= (int)TutorialStep.Complete)
        {
            EnterStep((TutorialStep)next);
        }
    }

    // ── Input Checks ──────────────────────────────────────

    private void CheckWelcomeInput()
    {
        // Any movement key dismisses welcome
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            AdvanceStep();
        }
        // Space also works
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceStep();
        }
    }

    private void CheckReachedTile(Vector2Int targetGrid)
    {
        if (goat.CurrentGrid == targetGrid && !goat.IsMoving)
        {
            // Small delay so the player sees they've landed
            if (Time.unscaledTime - stepAdvanceTime > 0.4f)
            {
                AdvanceStep();
            }
        }
    }

    private void CheckCompleteInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.ReturnToMenu();
        }
    }

    // ── Event Handlers ────────────────────────────────────

    private void HandleCoinsChanged(int sessionCoins, int totalCoins)
    {
        if (currentStep == TutorialStep.CollectCoin && sessionCoins > coinsBeforeStep)
        {
            AdvanceStep();
        }
    }

    private void HandleHungerChanged(float currentHunger, float maxHunger)
    {
        if (currentStep == TutorialStep.CollectGrass && currentHunger <= 1f)
        {
            // Hunger was cleared (grass eaten)
            if (Time.unscaledTime - stepAdvanceTime > 0.3f)
            {
                AdvanceStep();
            }
        }
    }

    // ── Step Display Methods ──────────────────────────────

    private void ShowWelcome()
    {
        hud.ShowMessage(
            "🐐 Welcome to Mountain Goat!",
            "Use the Q / E / A / D keys to move your goat\nacross the isometric mountain."
        );
        hud.ShowAllKeys();
        hud.SetSkipVisible(true);
    }

    private void ShowMoveForward()
    {
        hud.ShowMessage(
            "Step 1: Move Forward",
            "Press <b>E</b> to jump forward to the highlighted tile."
        );
        hud.HighlightKeys(q: false, e: true, a: false, d: false);
        HighlightTargetTile(forwardTarget);
    }

    private void ShowMoveDiagonal()
    {
        hud.ShowMessage(
            "Step 2: Diagonal Move",
            "Press <b>Q</b> to jump left-forward to the next tile.\n(Q = left-forward, D = right-backward)"
        );
        hud.HighlightKeys(q: true, e: false, a: false, d: false);
        HighlightTargetTile(diagonalTarget);
    }

    private void ShowCollectGrass()
    {
        hud.ShowMessage(
            "Step 3: Collect Grass 🌿",
            "Your hunger bar fills over time!\nJump onto the tile with <b>grass</b> to clear your hunger.\nPress <b>E</b> to reach it."
        );
        hud.HighlightKeys(q: false, e: true, a: false, d: false);
        HighlightTargetTile(grassTarget);
    }

    private void ShowCollectCoin()
    {
        coinsBeforeStep = GameManager.Instance != null ? GameManager.Instance.SessionCoins : 0;

        hud.ShowMessage(
            "Step 4: Collect Coins 💰",
            "Coins increase your score and total earnings.\nJump to the <b>gold coin</b> tile!\nPress <b>E</b> to reach it."
        );
        hud.HighlightKeys(q: false, e: true, a: false, d: false);
        HighlightTargetTile(coinTarget);
    }

    private void ShowAvoidHazard()
    {
        hud.ShowMessage(
            "Step 5: Watch Out! ⚠",
            "<color=#FF4444>Red tiles are DANGEROUS</color> — they kill on contact!\nAvoid the red tile and press <b>E</b> to reach the safe platform."
        );
        hud.HighlightKeys(q: false, e: true, a: false, d: false);
        HighlightTargetTile(completionTarget);
        HighlightHazardTile(hazardTile);
    }

    private void ShowComplete()
    {
        hud.ShowMessage(
            "🎉 Tutorial Complete!",
            "You're ready to climb the mountain!\n\nPress <b>Space</b> to return to the main menu and start your adventure."
        );
        hud.ClearKeyHighlights();
        hud.SetSkipVisible(false);
    }

    // ── Tile Highlighting ─────────────────────────────────

    private void HighlightTargetTile(Vector2Int gridPos)
    {
        ClearAllHighlights();
        SetTileHighlight(gridPos, new Color(1f, 0.9f, 0.3f, 0.7f));
    }

    private void HighlightHazardTile(Vector2Int gridPos)
    {
        SetTileHighlight(gridPos, new Color(1f, 0.15f, 0.1f, 0.8f));
    }

    private void SetTileHighlight(Vector2Int gridPos, Color highlightColor)
    {
        if (GridManager.Instance == null) return;
        if (!GridManager.Instance.TryGetTile(gridPos, out Tile tile)) return;

        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Store original and set highlight — simple color change
            renderer.material.color = highlightColor;
        }
    }

    private void ClearAllHighlights()
    {
        // Restore original tile colors
        // Since we use primitive cubes, we just need to reset to their original colors
        // This is best-effort; TutorialSceneSetup sets initial colors
    }

    // ── Public API ────────────────────────────────────────

    public void SkipTutorial()
    {
        GameManager.Instance.ReturnToMenu();
    }
}
