using UnityEngine;

public class HungerSystem : MonoBehaviour
{
    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float startingHunger = 0f;
    [SerializeField] private float hungerIncreasePerSecond = 6f;
    [SerializeField] private float highHungerThreshold = 100f;

    private float currentHunger;
    private bool hasTriggeredDeath;

    public float CurrentHunger => currentHunger;
    public float MaxHunger => maxHunger;
    public float HungerPercent => maxHunger <= 0f ? 0f : currentHunger / maxHunger;
    public bool IsHighHunger => currentHunger >= highHungerThreshold;

    private void Awake()
    {
        currentHunger = Mathf.Clamp(startingHunger, 0f, maxHunger);
        hasTriggeredDeath = false;
        NotifyChanged();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing || hasTriggeredDeath)
        {
            return;
        }

        ModifyHunger(hungerIncreasePerSecond * Time.deltaTime);

        if (currentHunger >= maxHunger)
        {
            GoatController goat = GetComponent<GoatController>();
            if (goat != null && goat.CanDie)
            {
                hasTriggeredDeath = true;
                goat.Die();
            }
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

    public void RestoreHunger(float amount)
    {
        ReduceHunger(amount);
    }

    public void ResetForNewRun()
    {
        currentHunger = Mathf.Clamp(startingHunger, 0f, maxHunger);
        hasTriggeredDeath = false;
        NotifyChanged();
    }

    private void ModifyHunger(float delta)
    {
        float previous = currentHunger;
        currentHunger = Mathf.Clamp(currentHunger + delta, 0f, maxHunger);

        if (!Mathf.Approximately(previous, currentHunger))
        {
            NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        GameManager.Instance.NotifyHungerChanged(currentHunger, maxHunger);
    }
}
