using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoatController player;
    [SerializeField] private GameObject safePlatformPrefab;
    [SerializeField] private GameObject obstaclePlatformPrefab;

    [Header("Generation")]
    [SerializeField] private int lanes = 3;
    [SerializeField] private int initialRows = 12;
    [SerializeField] private int rowsAhead = 10;
    [FormerlySerializedAs("diagonalStep")]
    [SerializeField] private float laneOffset = 1.5f;
    [FormerlySerializedAs("diagonalStep")]
    [SerializeField] private float rowStep = 1.5f;
    [SerializeField] private float heightStep = 0.5f;
    [SerializeField] private float obstacleSpawnChance = 0.35f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private int lastGeneratedRow = -1;

    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<GoatController>();
        }

        GenerateUntilRow(initialRows - 1);
    }

    private void Update()
    {
        if (player == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        int playerRow = Mathf.FloorToInt(player.transform.position.z / rowStep);
        int targetRow = playerRow + rowsAhead;
        GenerateUntilRow(targetRow);
    }

    private void GenerateUntilRow(int targetRow)
    {
        while (lastGeneratedRow < targetRow)
        {
            lastGeneratedRow++;
            GenerateRow(lastGeneratedRow);
        }
    }

    private void GenerateRow(int rowIndex)
    {
        int guaranteedSafeLane = Random.Range(0, lanes);

        for (int laneIndex = 0; laneIndex < lanes; laneIndex++)
        {
            bool shouldUseObstacle = rowIndex > 2 &&
                                     laneIndex != guaranteedSafeLane &&
                                     obstaclePlatformPrefab != null &&
                                     Random.value < obstacleSpawnChance;

            GameObject prefab = shouldUseObstacle ? obstaclePlatformPrefab : safePlatformPrefab;
            if (prefab == null)
            {
                continue;
            }

            Vector3 position = GetWorldPosition(laneIndex, rowIndex);
            GameObject spawned = Instantiate(prefab, position, Quaternion.identity, transform);
            spawnedObjects.Add(spawned);
        }
    }

    private float GetLaneX(int laneIndex)
    {
        float centerOffset = (lanes - 1) * 0.5f;
        return (laneIndex - centerOffset) * laneOffset;
    }

    private Vector3 GetWorldPosition(int laneIndex, int rowIndex)
    {
        return new Vector3(GetLaneX(laneIndex), rowIndex * heightStep, rowIndex * rowStep);
    }
}
