using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int DeathTrigger = Animator.StringToHash("Death");

    [Header("Spawn")]
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private Transform EnemySpawnRoot;
    [SerializeField] private Transform[] SpawnPoints;

    private bool _isSpawned;

    private readonly Dictionary<BattleUnitModel, EnemyUnitView> _enemyViewMap
        = new Dictionary<BattleUnitModel, EnemyUnitView>();

    private readonly Dictionary<BattleUnitModel, Animator> _enemyAnimatorMap
        = new Dictionary<BattleUnitModel, Animator>();

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

            Animator animator = enemyObject.GetComponentInChildren<Animator>(true);

            if (animator != null)
            {
                _enemyAnimatorMap.Add(enemyUnit, animator);
            }
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

        if (enemyUnit.IsDefeated)
        {
            enemyView.HideUnitInfo();
        }

        return true;
    }

    public bool PlayAttackAnimation(BattleUnitModel enemyUnit)
    {
        return SetAnimationTrigger(enemyUnit, AttackTrigger);
    }

    public bool PlayHitAnimation(BattleUnitModel enemyUnit)
    {
        return SetAnimationTrigger(enemyUnit, HitTrigger);
    }

    public bool PlayDeathAnimation(BattleUnitModel enemyUnit)
    {
        return SetAnimationTrigger(enemyUnit, DeathTrigger);
    }

    public bool TryGetVfxPoint(BattleUnitModel enemyUnit, out Transform vfxPoint)
    {
        vfxPoint = null;

        if (enemyUnit == null)
        {
            return false;
        }

        bool hasView = _enemyViewMap.TryGetValue(enemyUnit, out EnemyUnitView enemyView);

        if (hasView == false || enemyView == null)
        {
            return false;
        }

        vfxPoint = enemyView.VfxPoint;
        return vfxPoint != null;
    }

    public bool TryGetMuzzlePoint(BattleUnitModel enemyUnit, out Transform muzzlePoint)
    {
        muzzlePoint = null;

        if (enemyUnit == null)
        {
            return false;
        }

        bool hasView = _enemyViewMap.TryGetValue(
            enemyUnit,
            out EnemyUnitView enemyView);

        if (hasView == false || enemyView == null)
        {
            return false;
        }

        muzzlePoint = enemyView.muzzlePoint;
        return muzzlePoint != null;
    }

    private bool SetAnimationTrigger(BattleUnitModel enemyUnit, int triggerHash)
    {
        if (enemyUnit == null)
        {
            return false;
        }

        bool hasAnimator = _enemyAnimatorMap.TryGetValue(
            enemyUnit,
            out Animator animator);

        if (hasAnimator == false || animator == null)
        {
            return false;
        }

        animator.SetTrigger(triggerHash);
        return true;
    }

    public void ClearEnemies()
    {
        foreach (KeyValuePair<BattleUnitModel, EnemyUnitView> pair in _enemyViewMap)
        {
            EnemyUnitView enemyView = pair.Value;

            if (enemyView == null)
            {
                continue;
            }

            Destroy(enemyView.gameObject);
        }

        _enemyViewMap.Clear();
        _enemyAnimatorMap.Clear();
        _isSpawned = false;
    }
}
