using System.Collections.Generic;
using UnityEngine;

public class BattleBTExecutor : MonoBehaviour
{
    [SerializeField] private BattleBTContext BattleBTContext;

    public void SetBattleContext(
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList)
    {
        if (BattleBTContext == null)
        {
            return;
        }

        BattleBTContext.SetBattleContext(unit, heroList, enemyList);
    }

    public bool TryGetCreatedBattleAction(out BattleActionModel battleAction)
    {
        battleAction = null;

        if (BattleBTContext == null)
        {
            return false;
        }

        return BattleBTContext.TryGetCreatedBattleAction(out battleAction);
    }

    public void ClearCreatedBattleAction()
    {
        if (BattleBTContext == null)
        {
            return;
        }

        BattleBTContext.ClearCreatedBattleAction();
    }
}
