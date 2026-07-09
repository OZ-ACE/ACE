using System.Collections.Generic;

//전투 참가자들의 턴 순서를 정하는 매니저
public class TurnManager : SingletonBase<TurnManager>
{
    private List<BattleUnitModel> _turnOrder = new List<BattleUnitModel>();

    public List<BattleUnitModel> GetTurnOrder(List<BattleUnitModel> participants)
    {
        _turnOrder.Clear();
        _turnOrder.AddRange(participants);

        _turnOrder.Sort(CompareBattleUnit);

        return _turnOrder;
    }

    private int CompareBattleUnit(BattleUnitModel a, BattleUnitModel b)
    {
        if (a.Speed != b.Speed)
        {
            return b.Speed - a.Speed; //양수를 반환하면 b가 더 빠르므로 먼저 출격함
        }

        return b.AttackPower - a.AttackPower; //스피드 동률 시 공격력 순 정렬, 공격력도 같을 시 추가 로직 만들어야 함...ToDo...
    }
}
