using UnityEngine;
using UnityEngine.UI;

public class HungerSystem : MonoBehaviour
{
    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float startingHunger = 0f;
    [SerializeField] private float hungerIncreasePerSecond = 10f;
    [SerializeField] private float highHungerThreshold = 70f;

    [Header("Warning Bar")]
    [SerializeField] private float highHungerDeathDelay = 3f;
    [SerializeField] private Vector3 warningBarOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector2 warningBarSize = new Vector2(1.2f, 0.16f);
    [SerializeField] private Color warningBarBackgroundColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color warningBarStartColor = new Color(0.15f, 0.85f, 0.25f, 0.95f);
    [SerializeField] private Color warningBarEndColor = new Color(1f, 0.2f, 0.1f, 0.95f);

    private float currentHunger;
    private float highHungerTimer;
    private bool hasTriggeredDeath;
    private Canvas warningCanvas;
    private Image warningFillImage;
    private Camera mainCamera;

    public float CurrentHunger => currentHunger;
    public float MaxHunger => maxHunger;
    public float HungerPercent => maxHunger <= 0f ? 0f : currentHunger / maxHunger;
    public bool IsHighHunger => currentHunger >= highHungerThreshold;

    private void Awake()
    {
        currentHunger = Mathf.Clamp(startingHunger, 0f, maxHunger);
        highHungerTimer = 0f;
        hasTriggeredDeath = false;
        CreateWarningBar();
        SetWarningBarVisible(false);
        NotifyChanged();
    }

    private void Update()
    {
        if ((GameManager.Instance.CurrentState != GameManager.GameState.Playing && GameManager.Instance.CurrentState != GameManager.GameState.Tutorial) || hasTriggeredDeath)
        {
            return;
        }

        ModifyHunger(hungerIncreasePerSecond * Time.deltaTime);
        UpdateWarningBar();
    }

    private void LateUpdate()
    {
        if (warningCanvas == null)
        {
            return;
        }

        warningCanvas.transform.position = transform.position + warningBarOffset;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            warningCanvas.transform.rotation = Quaternion.LookRotation(
                warningCanvas.transform.position - mainCamera.transform.position,
                Vector3.up);
        }
    }

    public void ReduceHunger(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        ModifyHunger(-amount);
    }

    public void ClearHunger()
    {
        SetHunger(0f);
        ResetHighHungerWarning();
    }

    public void RestoreHunger(float amount)
    {
        ReduceHunger(amount);
    }

    public void ResetForNewRun()
    {
        currentHunger = Mathf.Clamp(startingHunger, 0f, maxHunger);
        highHungerTimer = 0f;
        hasTriggeredDeath = false;
        SetWarningBarVisible(false);
        NotifyChanged();
    }

    private void ModifyHunger(float delta)
    {
        SetHunger(currentHunger + delta);
    }

    private void SetHunger(float value)
    {
        float previous = currentHunger;
        currentHunger = Mathf.Clamp(value, 0f, maxHunger);

        if (!Mathf.Approximately(previous, currentHunger))
        {
            NotifyChanged();
        }
    }

    private void UpdateWarningBar()
    {
        if (currentHunger < highHungerThreshold)
        {
            ResetHighHungerWarning();
            return;
        }

        highHungerTimer += Time.deltaTime;
        float warningProgress = highHungerDeathDelay <= 0f ? 1f : highHungerTimer / highHungerDeathDelay;
        SetWarningBarVisible(true);

        if (warningFillImage != null)
        {
            float clampedProgress = Mathf.Clamp01(warningProgress);
            warningFillImage.fillAmount = clampedProgress;
            warningFillImage.color = Color.Lerp(warningBarStartColor, warningBarEndColor, clampedProgress);
        }

        if (warningProgress >= 1f)
        {
            TriggerDeath();
        }
    }

    private void ResetHighHungerWarning()
    {
        highHungerTimer = 0f;
        SetWarningBarVisible(false);

        if (warningFillImage != null)
        {
            warningFillImage.fillAmount = 0f;
            warningFillImage.color = warningBarStartColor;
        }
    }

    private void TriggerDeath()
    {
        GoatController goat = GetComponent<GoatController>();
        if (goat == null || !goat.CanDie)
        {
            return;
        }

        hasTriggeredDeath = true;
        goat.Die();
    }

    private void CreateWarningBar()
    {
        GameObject canvasObject = new GameObject("HungerWarningBar");
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = warningBarOffset;

        warningCanvas = canvasObject.AddComponent<Canvas>();
        warningCanvas.renderMode = RenderMode.WorldSpace;
        warningCanvas.sortingOrder = 50;

        RectTransform canvasRect = warningCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = warningBarSize;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = warningBarBackgroundColor;
        RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);
        warningFillImage = fillObject.AddComponent<Image>();
        warningFillImage.color = warningBarStartColor;
        warningFillImage.type = Image.Type.Filled;
        warningFillImage.fillMethod = Image.FillMethod.Horizontal;
        warningFillImage.fillOrigin = 0;
        warningFillImage.fillAmount = 0f;

        RectTransform fillRect = warningFillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }

    private void SetWarningBarVisible(bool isVisible)
    {
        if (warningCanvas != null && warningCanvas.gameObject.activeSelf != isVisible)
        {
            warningCanvas.gameObject.SetActive(isVisible);
        }
    }

    private void NotifyChanged()
    {
        GameManager.Instance.NotifyHungerChanged(currentHunger, maxHunger);
    }
}
