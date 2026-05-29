using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ThundercloudHazard : MonoBehaviour
{
    private static readonly Dictionary<Vector2Int, ThundercloudHazard> ActiveByGrid = new Dictionary<Vector2Int, ThundercloudHazard>();

    [Header("Timing")]
    [SerializeField] private float calmDuration = 2.2f;
    [SerializeField] private float strikeDuration = 0.8f;
    [SerializeField] private bool randomizeStartPhase = true;

    [Header("Visuals")]
    [SerializeField] private string lightningChildName = "Lightning";
    [SerializeField] private string electricSparksChildName = "ElectricSparks";

    private readonly List<GameObject> strikeVisuals = new List<GameObject>();
    private Vector2Int gridPosition;
    private float cycleTimer;
    private bool isInitialized;
    private bool visualsAreStriking;

    public Vector2Int GridPosition => gridPosition;
    public bool IsStriking
    {
        get
        {
            float cycleDuration = Mathf.Max(0.01f, calmDuration) + Mathf.Max(0.01f, strikeDuration);
            float phase = Mathf.Repeat(cycleTimer, cycleDuration);
            return phase >= calmDuration;
        }
    }

    public void Initialize(Vector2Int grid)
    {
        if (isInitialized)
        {
            Unregister();
        }

        gridPosition = grid;
        isInitialized = true;
        ActiveByGrid[gridPosition] = this;

        CacheStrikeVisuals();
        if (randomizeStartPhase)
        {
            cycleTimer = Random.Range(0f, Mathf.Max(0.01f, calmDuration + strikeDuration));
        }

        UpdateStrikeVisuals(true);
    }

    public static bool IsStrikingAt(Vector2Int grid)
    {
        return ActiveByGrid.TryGetValue(grid, out ThundercloudHazard hazard)
            && hazard != null
            && hazard.isActiveAndEnabled
            && hazard.IsStriking;
    }

    private void Awake()
    {
        CacheStrikeVisuals();
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            ActiveByGrid[gridPosition] = this;
        }
    }

    private void Update()
    {
        cycleTimer += Time.deltaTime;
        UpdateStrikeVisuals(false);
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    private void CacheStrikeVisuals()
    {
        strikeVisuals.Clear();
        AddChildVisual(lightningChildName);
        AddChildVisual(electricSparksChildName);
    }

    private void AddChildVisual(string childName)
    {
        if (string.IsNullOrWhiteSpace(childName))
        {
            return;
        }

        Transform child = FindChildRecursive(transform, childName);
        if (child != null)
        {
            strikeVisuals.Add(child.gameObject);
        }
    }

    private void UpdateStrikeVisuals(bool force)
    {
        bool striking = IsStriking;
        if (!force && visualsAreStriking == striking)
        {
            return;
        }

        visualsAreStriking = striking;
        for (int i = 0; i < strikeVisuals.Count; i++)
        {
            if (strikeVisuals[i] != null)
            {
                strikeVisuals[i].SetActive(striking);
            }
        }
    }

    private void Unregister()
    {
        if (!isInitialized)
        {
            return;
        }

        if (ActiveByGrid.TryGetValue(gridPosition, out ThundercloudHazard existing) && existing == this)
        {
            ActiveByGrid.Remove(gridPosition);
        }
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindChildRecursive(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}
