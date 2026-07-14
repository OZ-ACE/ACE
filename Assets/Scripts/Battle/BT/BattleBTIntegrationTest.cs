using System.Collections.Generic;
using UnityEngine;

public class BattleBTIntegrationTest : MonoBehaviour
{
    [SerializeField] private BattleBTExecutor BattleBTExecutor;

    private const string EnemySkillId = "enemySkill_01_01";
    private const string HeroSkillId = "heroSkill_01_01";

    private int _createdActionEventCount;

    private void OnEnable()
    {
        if (BattleBTExecutor != null)
        {
            BattleBTExecutor.BattleActionCreated += OnBattleActionCreated;
        }
    }

    private void OnDisable()
    {
        if (BattleBTExecutor != null)
        {
            BattleBTExecutor.BattleActionCreated -= OnBattleActionCreated;
        }
    }

    [ContextMenu("적 BT 액션 큐 연동 테스트")]
    private void TestEnemyBattleAction()
    {
        _createdActionEventCount = 0;

        BattleUnitModel heroUnit = new BattleUnitModel();
        heroUnit.ID = "hero_01";
        heroUnit.IsHero = true;
        heroUnit.MaxHp = 100;
        heroUnit.CurrentHp = 100;
        heroUnit.Speed = 10;
        heroUnit.AttackPower = 10;

        BattleUnitModel enemyUnit = new BattleUnitModel();
        enemyUnit.ID = "enemy_01";
        enemyUnit.IsHero = false;
        enemyUnit.MaxHp = 100;
        enemyUnit.CurrentHp = 100;
        enemyUnit.Speed = 5;
        enemyUnit.AttackPower = 9;
        enemyUnit.SkillIdList.Add("invalidSkillId");
        enemyUnit.SkillIdList.Add(EnemySkillId);

        List<BattleUnitModel> heroList = new List<BattleUnitModel>();
        heroList.Add(heroUnit);

        List<BattleUnitModel> enemyList = new List<BattleUnitModel>();
        enemyList.Add(enemyUnit);

        List<BattleUnitModel> turnOrder = new List<BattleUnitModel>();
        turnOrder.Add(heroUnit);
        turnOrder.Add(enemyUnit);

        BattleManager.Inst.BuildActionQueue(turnOrder);

        bool isExecuted = BattleBTExecutor.ExecuteBattleAction(
            enemyUnit,
            heroList,
            enemyList);

        if (isExecuted == false)
        {
            Debug.LogWarning("[BattleBTIntegrationTest] BT 실행 요청에 실패했습니다.");
        }
    }

    [ContextMenu("영웅 BT 액션 큐 연동 테스트")]
    private void TestHeroBattleAction()
    {
        _createdActionEventCount = 0;

        BattleUnitModel heroUnit = new BattleUnitModel();
        heroUnit.ID = "hero_01";
        heroUnit.IsHero = true;
        heroUnit.MaxHp = 100;
        heroUnit.CurrentHp = 100;
        heroUnit.Speed = 10;
        heroUnit.AttackPower = 10;
        heroUnit.SkillIdList.Add("invalidSkillId");
        heroUnit.SkillIdList.Add(HeroSkillId);

        BattleUnitModel enemyUnit = new BattleUnitModel();
        enemyUnit.ID = "enemy_01";
        enemyUnit.IsHero = false;
        enemyUnit.MaxHp = 100;
        enemyUnit.CurrentHp = 100;
        enemyUnit.Speed = 5;
        enemyUnit.AttackPower = 9;

        List<BattleUnitModel> heroList = new List<BattleUnitModel>();
        heroList.Add(heroUnit);

        List<BattleUnitModel> enemyList = new List<BattleUnitModel>();
        enemyList.Add(enemyUnit);

        List<BattleUnitModel> turnOrder = new List<BattleUnitModel>();
        turnOrder.Add(heroUnit);
        turnOrder.Add(enemyUnit);

        BattleManager.Inst.BuildActionQueue(turnOrder);

        bool isExecuted = BattleBTExecutor.ExecuteBattleAction(
            heroUnit,
            heroList,
            enemyList);

        if (isExecuted == false)
        {
            Debug.LogWarning("[BattleBTIntegrationTest] 영웅 BT 실행 요청에 실패했습니다.");
        }
    }

    private void OnBattleActionCreated(BattleActionModel battleAction)
    {
        _createdActionEventCount++;

        bool isApplied = BattleManager.Inst.EnqueueUnitAction(battleAction);

        Debug.Log(
            $"[BattleBTIntegrationTest] 이벤트 횟수: {_createdActionEventCount}, " +
            $"큐 반영 성공: {isApplied}, " +
            $"UnitId: {battleAction.Unit.ID}, " +
            $"SkillId: {battleAction.SkillId}, " +
            $"ActionType: {battleAction.ActionType}");
    }
}
