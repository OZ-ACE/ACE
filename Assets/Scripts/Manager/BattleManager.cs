using System.Collections.Generic;
using UnityEngine;

//전투 라운드의 액션 큐를 생성하고 전체 진행을 관리하는 매니저
public class BattleManager : SingletonBase<BattleManager>
{
    private const int MaxEnergyGauge = 5; //플레이어 개입 턴의 에너지 게이지 총량, 전투 전체 통틀어 처음1회만 채워짐, 라운드별 리셋 안됨
    private const string BattleConfigId = "default"; //BattleConfig 테이블의 유일한 row ID

    [SerializeField] private BattleBTExecutor Executor_Hero;
    [SerializeField] private BattleBTExecutor Executor_Enemy;

    public BattleBTExecutor HeroExecutor { get { return Executor_Hero; } }
    public BattleBTExecutor EnemyExecutor { get { return Executor_Enemy; } }

    private Queue<BattleActionModel> _actionQueue = new Queue<BattleActionModel>();
    private int _energyGauge;
    private int _currentRound;

    //라운드 증가, 페널티 지속시간 갱신, 이전 액션 큐 초기화
    public void BuildActionQueue(List<BattleUnitModel> turnOrder)
    {
        _currentRound++;
        UpdatePenaltyDuration(turnOrder);

        _actionQueue.Clear();
    }

    //BT가 생성한 완성된 유닛 행동을 액션 큐에 추가
    public bool EnqueueUnitAction(BattleActionModel createdAction)
    {
        if (createdAction == null || createdAction.Unit == null)
        {
            return false;
        }

        createdAction.IsPlayerAction = false;
        _actionQueue.Enqueue(createdAction);

        return true;
    }

    //모든 유닛 행동 생성 후 플레이어 개입 행동을 액션 큐 마지막에 추가
    public void EnqueuePlayerAction()
    {
        BattleActionModel playerAction = new BattleActionModel();
        playerAction.Unit = null;
        playerAction.IsPlayerAction = true;

        _actionQueue.Enqueue(playerAction);
    }

    //큐 안에서 targetUnitId를 가진 행동을 찾아 결과를 확정한다. 에너지 부족과 대상 없음을 구분해서 반환한다
    public ActionApplyResult SetActionResult(string targetUnitId, BattleActionResult result, int energyCost)
    {
        if (_energyGauge < energyCost)
        {
            return ActionApplyResult.InsufficientEnergy;
        }

        foreach (BattleActionModel action in _actionQueue)
        {
            if (action.Unit != null && action.Unit.ID == targetUnitId)
            {
                action.Result = result;
                _energyGauge -= energyCost;

                return ActionApplyResult.Success;
            }
        }

        return ActionApplyResult.TargetNotFound;
    }

    public int GetRemainingEnergy()
    {
        return _energyGauge;
    }

    public int GetCurrentRound()
    {
        return _currentRound;
    }

    public bool HasNextAction()
    {
        return _actionQueue.Count > 0;
    }

    public BattleActionModel GetNextAction()
    {
        if (_actionQueue.Count == 0)
        {
            return null;
        }

        return _actionQueue.Dequeue();
    }

    //현재 액션 큐 상태를 List로 복사해 반환한다. Dequeue와 달리 원본 큐는 그대로 유지되며 UI 갱신 등 조회 목적으로만 사용한다
    public List<BattleActionModel> GetActionQueueSnapshot()
    {
        return new List<BattleActionModel>(_actionQueue);
    }

    //전체 유닛 리스트를 받아 승패를 판정한다. 적이 하나도 안 남으면 승리, 영웅이 하나도 안 남으면 패배
    public BattleResult CheckBattleResult(List<BattleUnitModel> allUnits)
    {
        bool isAnyHeroAlive = false;
        bool isAnyEnemyAlive = false;

        foreach (BattleUnitModel unit in allUnits)
        {
            if (unit.IsDefeated)
            {
                continue;
            }

            if (unit.IsHero)
            {
                isAnyHeroAlive = true;
            }
            else
            {
                isAnyEnemyAlive = true;
            }
        }

        if (!isAnyEnemyAlive)
        {
            return BattleResult.Victory;
        }

        if (!isAnyHeroAlive)
        {
            return BattleResult.Defeat;
        }

        return BattleResult.Ongoing;
    }

    //새 전투 시작 시 라운드와 에너지 게이지를 초기화한다. 실제 전투 시작 지점(씬 진입 로직)에서 반드시 1회 호출되어야 함 — 아직 호출부 없음(M3에서 연동 예정)
    public void ResetBattleState()
    {
        _currentRound = 0;
        _energyGauge = MaxEnergyGauge;
    }

    //전투 결과와 소요 라운드 수를 받아 지급할 기억의파편 수량을 계산한다. 승리가 아니면 0 반환
    //실제 PlayerModel 반영은 M3에서 ViewModel을 통해 연동 예정, 여기서는 계산 값만 반환
    public int CalculateReward(BattleResult result, int roundCount)
    {
        if (result != BattleResult.Victory)
        {
            return 0;
        }

        BattleConfig config = GameDataManager.Inst.GetData<BattleConfig>(BattleConfigId);

        if (config == null)
        {
            return 0;
        }

        if (roundCount <= config.DoubleBonusRoundThreshold)
        {
            return config.BaseRewardAmount * config.DoubleBonusPercent / 100;
        }

        if (roundCount <= config.HalfBonusRoundThreshold)
        {
            return config.BaseRewardAmount * config.HalfBonusPercent / 100;
        }

        return config.BaseRewardAmount;
    }

    //누적 전투 참여 횟수를 받아 현재 프라임 레벨을 계산한다. 실제 HeroProgressModel 갱신은 M3에서 ViewModel을 통해 연동 예정
    public int CalculatePrimeLevel(int battleParticipateCount)
    {
        BattleConfig config = GameDataManager.Inst.GetData<BattleConfig>(BattleConfigId);

        if (config == null || config.ParticipateCountPerLevel <= 0)
        {
            return 0;
        }

        return battleParticipateCount / config.ParticipateCountPerLevel;
    }

    //유닛이 스킬을 사용했을 때 페널티 게이지를 갱신한다. 이미 페널티가 발동 중인 유닛은 중첩 방지를 위해 게이지 갱신을 건너뛴다
    public void UpdatePenaltyGauge(BattleUnitModel unit, string usedSkillId)
    {
        if (!string.IsNullOrEmpty(unit.ActivePenaltyId))
        {
            return;
        }

        if (unit.LastSkillId == usedSkillId)
        {
            unit.RepeatSkillCount++;
        }
        else
        {
            unit.LastSkillId = usedSkillId;
            unit.RepeatSkillCount = 1;
        }

        foreach (Penalty penalty in GameDataManager.Inst.GetDataList<Penalty>())
        {
            if (penalty.TriggerSkillId != usedSkillId)
            {
                continue;
            }

            if (unit.RepeatSkillCount >= penalty.TriggerCount)
            {
                unit.ActivePenaltyId = penalty.ID;
                unit.PenaltyRemainingRounds = penalty.DurationRounds;
                unit.RepeatSkillCount = 0;
            }

            break;
        }
    }

    //해당 유닛이 지금 skillId를 사용할 수 있는지(페널티로 막혀있는지) 검사한다. BT의 스킬 선택 단계에서 CanUseSkill과 함께 호출되어야 함
    public bool IsSkillBlockedByPenalty(BattleUnitModel unit, string skillId)
    {
        if (string.IsNullOrEmpty(unit.ActivePenaltyId))
        {
            return false;
        }

        Penalty penalty = GameDataManager.Inst.GetData<Penalty>(unit.ActivePenaltyId);

        if (penalty == null)
        {
            return false;
        }

        return penalty.TriggerSkillId == skillId;
    }

    //지원 아이템 적용 성공 시 호출, 대상 유닛의 활성 페널티를 완전히 제거한다 >> 일단 이렇게 구현해뒀습니다..
    public void RemovePenalty(BattleUnitModel unit)
    {
        unit.ActivePenaltyId = null;
        unit.PenaltyRemainingRounds = 0;
    }

    //라운드 시작 시 각 유닛의 페널티 지속 라운드를 감소시키고, 0이 되면 해제한다
    private void UpdatePenaltyDuration(List<BattleUnitModel> turnOrder)
    {
        foreach (BattleUnitModel unit in turnOrder)
        {
            if (string.IsNullOrEmpty(unit.ActivePenaltyId))
            {
                continue;
            }

            unit.PenaltyRemainingRounds--;

            if (unit.PenaltyRemainingRounds <= 0)
            {
                unit.ActivePenaltyId = null;
                unit.PenaltyRemainingRounds = 0;
            }
        }
    }
}
