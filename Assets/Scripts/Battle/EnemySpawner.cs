using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private Transform EnemySpawnRoot;
    [SerializeField] private Transform[] SpawnPoints;

    private bool _isSpawned;

    private readonly Dictionary<BattleUnitModel, EnemyUnitView> _enemyViewMap
        = new Dictionary<BattleUnitModel, EnemyUnitView>();

    public bool SpawnEnemies(List<BattleUnitModel> enemyList)
    {
        if (_isSpawned)
        {
            return false;
        }
        
        if (EnemyPrefab == null ||
            EnemySpawnRoot == null || 
            SpawnPoints == null ||
            SpawnPoints.Length == 0 ||
            enemyList == null || 
            enemyList.Count == 0)
        {
            return false;
        }

        int spawnCount = Mathf.Min(enemyList.Count, SpawnPoints.Length);
        
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = SpawnPoints[i];
            BattleUnitModel enemyUnit = enemyList[i];

            if (spawnPoint == null || enemyUnit == null)
            {
                continue;
            }

            GameObject enemyObject = Instantiate(
                EnemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                EnemySpawnRoot);

            EnemyUnitView enemyView = enemyObject.GetComponent<EnemyUnitView>();

            if (enemyView == null)
            {
                continue;
            }

            enemyView.Initialize(
                GameUtil.GetUnitDisplayName(enemyUnit.ID),
                enemyUnit.CurrentHp,
                enemyUnit.MaxHp);

            _enemyViewMap.Add(enemyUnit, enemyView);
        }

        _isSpawned = true;
        return true;
    }

    public bool RefreshEnemyView(BattleUnitModel enemyUnit)
    {
        if (enemyUnit == null)
        {
            return false;
        }

        bool hasView = _enemyViewMap.TryGetValue(enemyUnit, out EnemyUnitView enemyView);

        if (hasView == false || enemyView == null)
        {
            return false;
        }

        enemyView.RefreshHp(
            enemyUnit.CurrentHp,
            enemyUnit.MaxHp);

        return true;
    }
}
