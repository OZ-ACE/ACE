using System.Collections.Generic;
using UnityEngine;

//전투 라운드의 액션 큐를 생성하고 전체 진행을 관리하는 매니저
public class BattleManager : SingletonBase<BattleManager>
{
    private Queue<BattleActionModel> _actionQueue = new Queue<BattleActionModel>();

    //턴 순서 리스트를 받아와 각 유닛을 큐에 넣고, 마지막에 플레이어 개입 액션을 추가한다
    public void BuildActionQueue(List<BattleUnitModel> turnOrder)
    {
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
