using System.Collections.Generic;

//전투 라운드의 액션 큐를 생성하고 전체 진행을 관리하는 매니저
public class BattleManager : SingletonBase<BattleManager>
{
    private const int MaxEnergyGauge = 3; //플레이어 개입 턴의 에너지 게이지 총량

    private Queue<BattleActionModel> _actionQueue = new Queue<BattleActionModel>();
    private int _energyGauge;

    //턴 순서 리스트를 받아와 각 유닛을 큐에 넣고, 마지막에 플레이어 개입 액션을 추가한다
    public void BuildActionQueue(List<BattleUnitModel> turnOrder)
    {
        _actionQueue.Clear();
        _energyGauge = MaxEnergyGauge;

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
}
