using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class BattleBTExecutor : MonoBehaviour
{
    [SerializeField] private BattleBTContext BattleBTContext;
    [SerializeField] private BehaviorGraphAgent BehaviorGraphAgent;

    public event Action<BattleActionModel> BattleActionCreated;

    private bool _hasStarted;

    private void OnEnable()
    {
        if (BattleBTContext != null)
        {
            BattleBTContext.BattleActionCreated += OnBattleActionCreated;
        }
    }

    private void OnDisable()
    {
        if (BattleBTContext != null)
        {
            BattleBTContext.BattleActionCreated -= OnBattleActionCreated;
        }
    }

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

    public bool ExecuteBattleAction(
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList)
    {
        if (BattleBTContext == null ||
            BehaviorGraphAgent == null ||
            unit == null)
        {
            return false;
        }

        BattleBTContext.SetBattleContext(unit, heroList, enemyList);
        BattleBTContext.ClearCreatedBattleAction();

        bool isContextSet = BehaviorGraphAgent.SetVariableValue("BattleContext", BattleBTContext);

        if (isContextSet == false)
        {
            return false;
        }

        if (_hasStarted == false)
        {
            BehaviorGraphAgent.Start();
            _hasStarted = true;
        }
        else
        {
            BehaviorGraphAgent.Restart();
        }

            return true;
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

    private void OnBattleActionCreated(BattleActionModel battleAction)
    {
        BattleActionCreated?.Invoke(battleAction);
    }
}
