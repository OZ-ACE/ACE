using System.Collections.Generic;
using UnityEngine;

public class BattleBTContext : MonoBehaviour
{
    private BattleUnitModel _unit;
    private List<BattleUnitModel> _heroList;
    private List<BattleUnitModel> _enemyList;
    private BattleActionModel _createdBattleAction;

    public BattleUnitModel Unit
    {
        get { return _unit; }
    }

    public List<BattleUnitModel> HeroList
    {
        get { return _heroList; }
    }

    public List<BattleUnitModel> EnemyList
    {
        get { return _enemyList; }
    }

    public BattleActionModel CreatedBattleAction
    {
        get { return _createdBattleAction; }
    }

    public bool HasCreatedBattleAction()
    {
        return _createdBattleAction != null;
    }

    public void SetBattleContext(
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList)
    {
        _unit = unit;
        _heroList = heroList;
        _enemyList = enemyList;
        _createdBattleAction = null;
    }

    public void SetCreatedBattleAction(BattleActionModel battleAction)
    {
        _createdBattleAction = battleAction;
    }

    public void ClearCreatedBattleAction()
    {
        _createdBattleAction = null;
    }
}
