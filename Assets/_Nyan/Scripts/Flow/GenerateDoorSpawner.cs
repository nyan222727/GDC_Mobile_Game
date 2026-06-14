using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GenerateDoorSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
    [SerializeField] private int defaultSpawnCount = 5;
    [SerializeField] private float defaultSpawnInterval = 5f;
    [SerializeField] private bool useRandomEnemyPrefab = true;

    public int DefaultSpawnCount => Mathf.Max(0, defaultSpawnCount);
    public float DefaultSpawnInterval => Mathf.Max(0f, defaultSpawnInterval);
    public bool HasEnemyPrefabs => enemyPrefabs != null && enemyPrefabs.Count > 0;

    public IEnumerator SpawnWave(
        IReadOnlyList<GameObject> overrideEnemyPrefabs,
        int spawnCount,
        float spawnInterval,
        Action<GameObject> onEnemySpawned)
    {
        int count = spawnCount > 0 ? spawnCount : DefaultSpawnCount;
        float interval = spawnInterval > 0f ? spawnInterval : DefaultSpawnInterval;

        for (int i = 0; i < count; i++)
        {
            GameObject enemyPrefab = PickEnemyPrefab(overrideEnemyPrefabs);
            if (enemyPrefab == null)
            {
                Debug.LogWarning($"{nameof(GenerateDoorSpawner)} has no enemy prefab to spawn.", this);
                yield break;
            }

            GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
            onEnemySpawned?.Invoke(enemy);

            if (i + 1 < count && interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    public IEnumerator SpawnDefaultWave(Action<GameObject> onEnemySpawned)
    {
        return SpawnWave(null, DefaultSpawnCount, DefaultSpawnInterval, onEnemySpawned);
    }

    private GameObject PickEnemyPrefab(IReadOnlyList<GameObject> overrideEnemyPrefabs)
    {
        IReadOnlyList<GameObject> source = HasAnyEnemyPrefab(overrideEnemyPrefabs)
            ? overrideEnemyPrefabs
            : enemyPrefabs;

        if (!HasAnyEnemyPrefab(source))
        {
            return null;
        }

        if (!useRandomEnemyPrefab)
        {
            return FirstValidEnemyPrefab(source);
        }

        int startIndex = UnityEngine.Random.Range(0, source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            GameObject enemyPrefab = source[(startIndex + i) % source.Count];
            if (enemyPrefab != null)
            {
                return enemyPrefab;
            }
        }

        return null;
    }

    private static bool HasAnyEnemyPrefab(IReadOnlyList<GameObject> source)
    {
        if (source == null)
        {
            return false;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private static GameObject FirstValidEnemyPrefab(IReadOnlyList<GameObject> source)
    {
        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
            {
                return source[i];
            }
        }

        return null;
    }
}
