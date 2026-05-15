using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    [SerializeField] private int halfWidth = 2;
    [SerializeField] private int seedColumn = 0;

    public int CurrentSafeColumn { get; private set; }

    private void Awake()
    {
        CurrentSafeColumn = Mathf.Clamp(seedColumn, -halfWidth, halfWidth);
    }

    public int AdvanceSafeColumn()
    {
        int direction = Random.value < 0.5f ? -1 : 1;

        if (CurrentSafeColumn <= -halfWidth)
        {
            direction = 1;
        }
        else if (CurrentSafeColumn >= halfWidth)
        {
            direction = -1;
        }

        CurrentSafeColumn = Mathf.Clamp(CurrentSafeColumn + direction, -halfWidth, halfWidth);
        return CurrentSafeColumn;
    }

    public int GetSeedColumn()
    {
        return CurrentSafeColumn;
    }
}
