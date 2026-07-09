using System.Collections.Generic;

//전투 라운드의 액션 큐를 생성하고 전체 진행을 관리하는 매니저
public class BattleManager : SingletonBase<BattleManager>
{
    private const int MaxEnergyGauge = 5; //플레이어 개입 턴의 에너지 게이지 총량, 전투 전체 통틀어 처음1회만 채워짐, 라운드별 리셋 안됨
    private const int BaseRewardAmount = 100; //더미값, 추후 밸런싱(9단계)에서 데이터 테이블로 전환 예정
    private const float DoubleBonusMultiplier = 2f;
    private const float HalfBonusMultiplier = 1.5f;
    private const int DoubleBonusRoundThreshold = 2; //2라운드 이내 클리어 시 2배
    private const int HalfBonusRoundThreshold = 3; //3라운드 이내 클리어 시 1.5배

    private Queue<BattleActionModel> _actionQueue = new Queue<BattleActionModel>();
    private int _energyGauge;
    private int _currentRound;

    //턴 순서 리스트를 받아와 각 유닛을 큐에 넣고, 마지막에 플레이어 개입 액션을 추가한다
    public void BuildActionQueue(List<BattleUnitModel> turnOrder)
    {
        _currentRound++;

        _actionQueue.Clear();

        foreach (BattleUnitModel unit in turnOrder)
        {
            BattleActionModel unitAction = new BattleActionModel();
            unitAction.Unit = unit;
            unitAction.IsPlayerAction = false;

            _actionQueue.Enqueue(unitAction);
        }

        BattleActionModel playerAction = new BattleActionModel();
        playerAction.Unit = null;
        playerAction.IsPlayerAction = true;

        _actionQueue.Enqueue(playerAction);
    }

    //큐 안에서 targetUnitId를 가진 행동을 찾아 결과를 확정한다. 에너지 부족 시 선택 자체가 불가능하도록 UI에서 막을 예정이라, 여기서의 false 반환은 그 상황이 뚫렸을 때를 대비한 방어 코드이다
    public bool SetActionResult(string targetUnitId, BattleActionResult result, int energyCost)
    {
        if (_energyGauge < energyCost)
        {
            return false;
        }

        foreach (BattleActionModel action in _actionQueue)
        {
            if (action.Unit != null && action.Unit.ID == targetUnitId)
            {
                action.Result = result;
                _energyGauge -= energyCost;

                return true;
            }
        }

        return false;
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

        if (roundCount <= DoubleBonusRoundThreshold)
        {
            return (int)(BaseRewardAmount * DoubleBonusMultiplier);
        }

        if (roundCount <= HalfBonusRoundThreshold)
        {
            return (int)(BaseRewardAmount * HalfBonusMultiplier);
        }

        return BaseRewardAmount;
    }
}
