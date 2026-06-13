using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private Transform poolRoot;
    [SerializeField] private int warmInstancesPerPrefab = 32;

    private readonly Dictionary<GameObject, Stack<GameObject>> poolsByPrefab = new Dictionary<GameObject, Stack<GameObject>>();
    private readonly Dictionary<int, GameObject> prefabByInstanceId = new Dictionary<int, GameObject>();
    private readonly HashSet<GameObject> warmedPrefabs = new HashSet<GameObject>();

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            return null;
        }

        EnsureWarm(prefab);
        GameObject instance = GetOrCreate(prefab);
        prefabByInstanceId[instance.GetInstanceID()] = prefab;

        Transform parentToUse = parent != null ? parent : transform;
        instance.transform.SetParent(parentToUse, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (!prefabByInstanceId.TryGetValue(instance.GetInstanceID(), out GameObject prefab) || prefab == null)
        {
            Destroy(instance);
            return;
        }

        prefabByInstanceId.Remove(instance.GetInstanceID());
        instance.SetActive(false);
        instance.transform.SetParent(poolRoot != null ? poolRoot : transform, false);

        if (!poolsByPrefab.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            poolsByPrefab[prefab] = pool;
        }

        pool.Push(instance);
    }

    public void Warm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        if (!poolsByPrefab.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            poolsByPrefab[prefab] = pool;
        }

        while (pool.Count < count)
        {
            GameObject instance = Instantiate(prefab, poolRoot != null ? poolRoot : transform);
            instance.SetActive(false);
            prefabByInstanceId[instance.GetInstanceID()] = prefab;
            pool.Push(instance);
        }
    }

    public void Warm(GameObject prefab)
    {
        Warm(prefab, warmInstancesPerPrefab);
    }

    private void EnsureWarm(GameObject prefab)
    {
        if (prefab == null || warmedPrefabs.Contains(prefab))
        {
            return;
        }

        warmedPrefabs.Add(prefab);
        Warm(prefab, warmInstancesPerPrefab);
    }

    private GameObject GetOrCreate(GameObject prefab)
    {
        if (poolsByPrefab.TryGetValue(prefab, out Stack<GameObject> pool) && pool.Count > 0)
        {
            return pool.Pop();
        }

        return Instantiate(prefab);
    }
}
