using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//전투 참가자 데이터를 BattleUnitModel로 변환하고 턴 순서를 조립하는 뷰모델
public class BattleViewModel : ViewModelBase
{
    public List<string> BattleLogs = new List<string>();

    public List<BattleUnitModel> GetBattleTurnOrder(List<string> heroIds, List<string> enemyIds)
    {
        List<BattleUnitModel> participats = new List<BattleUnitModel>();

        foreach (string heroId in heroIds)
        {
            HeroBattleData data = GameDataManager.Inst.GetData<HeroBattleData>(heroId);

            if (data == null)
            {
                continue;
            }

            BattleUnitModel unit = new BattleUnitModel();
            unit.ID = data.ID;
            unit.IsHero = true;
            unit.Speed = data.speed;
            unit.AttackPower = data.attackPower;

            participats.Add(unit);
        }

        foreach (string enemyId in enemyIds)
        {
            EnemyBattleData data = GameDataManager.Inst.GetData<EnemyBattleData>(enemyId);

            if (data == null)
            {
                continue;
            }

            BattleUnitModel unit = new BattleUnitModel();
            unit.ID = data.ID;
            unit.IsHero = false;
            unit.Speed = data.speed;
            unit.AttackPower = data.attackPower;

            participats.Add(unit);
        }

        return TurnManager.Inst.GetTurnOrder(participats);
    }

    //전투 로그 추가
    public void AddBattleLog(string message)
    {
        BattleLogs.Add(message);

        OnPropertyChanged(nameof(BattleLogs));
    }
}
