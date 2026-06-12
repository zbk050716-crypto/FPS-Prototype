using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    public GameObject enemyPrefab;
    public int poolSize = 10;
    public Transform spawnArea;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        if (spawnArea == null)
        {
            spawnArea = GameObject.Find("Ground")?.transform;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetEnemy()
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("[EnemyPool] 对象池已耗尽，无法生成更多敌人");
            return null;
        }

        GameObject obj = pool.Dequeue();
        obj.transform.position = GetRandomSpawnPoint();
        obj.SetActive(true);
        activeEnemies.Add(obj);
        Debug.Log($"生成敌人，当前场景中敌人数: {activeEnemies.Count}");
        return obj;
    }

    public void ReturnEnemy(GameObject obj)
    {
        obj.SetActive(false);
        activeEnemies.Remove(obj);
        pool.Enqueue(obj);
        Debug.Log($"敌人被击杀，当前场景中敌人数: {activeEnemies.Count}");
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    Vector3 GetRandomSpawnPoint()
    {
        if (spawnArea == null)
            return Vector3.zero;

        Renderer groundRenderer = spawnArea.GetComponent<Renderer>();
        if (groundRenderer == null)
            return spawnArea.position;

        Bounds bounds = groundRenderer.bounds;

        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 candidate = new Vector3(x, 100f, z);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 200f, NavMesh.AllAreas))
                return hit.position;
        }

        return spawnArea.position;
    }
}
