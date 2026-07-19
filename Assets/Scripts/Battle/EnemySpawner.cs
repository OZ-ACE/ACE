using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private Transform EnemySpawnRoot;
    [SerializeField] private Transform[] SpawnPoints;

    public void SpawnEnemies()
    {
        if (EnemyPrefab == null || EnemySpawnRoot == null)
        {
            return;
        }

        for (int i = 0; i < SpawnPoints.Length; i++)
        {
            Transform spawnPoint = SpawnPoints[i];

            if (spawnPoint == null)
            {
                continue;
            }

            Instantiate(
                EnemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                EnemySpawnRoot);
        }
    }
}
