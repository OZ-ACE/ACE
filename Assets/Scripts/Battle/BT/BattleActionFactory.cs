using System.Collections.Generic;

public static class BattleActionFactory
{
    //선택된 스킬 정보와 대상 정보를 기반으로 BattleActionModel 생성을 시도
    public static bool TryCreateSkillAction(
        BattleUnitModel unit,
        string skillId,
        ActionType actionType,
        SkillType skillType,
        TargetType targetType,
        TargetSelectType targetSelectType,
        int targetCount,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        out BattleActionModel battleAction)
    {
        battleAction = null;

        if (BattleSkillConditionChecker.CanAct(unit) == false)
        {
            return false;
        }

        List<BattleUnitModel> targetList = BattleTargetSelector.SelectTargets(
            unit,
            heroList,
            enemyList,
            targetSelectType,
            targetCount);

        if (BattleSkillConditionChecker.CanUseSkill(unit, skillId, actionType, targetList) == false)
        {
            return false;
        }

        BattleUnitModel target = null;
        List<BattleUnitModel> battleTargetList = new List<BattleUnitModel>();

        if (targetType == TargetType.Single)
        {
            if (targetList.Count <= 0)
            {
                return false;
            }

            target = targetList[0];
        }
        else if (targetType == TargetType.Multi)
        {
            battleTargetList = targetList;
        }
        else
        {
            return false;
        }

        battleAction = new BattleActionModel();
        battleAction.Unit = unit;
        battleAction.IsPlayerAction = false;
        battleAction.SkillId = skillId;
        battleAction.ActionType = actionType;
        battleAction.SkillType = skillType;
        battleAction.TargetType = targetType;
        battleAction.TargetSelectType = targetSelectType;
        battleAction.TargetCount = targetCount;
        battleAction.Target = target;
        battleAction.TargetList = battleTargetList;

        return true;
    }

    //사용 가능한 스킬이 없을 때 사용할 Wait BattleActionModel을 생성
    public static BattleActionModel CreateWaitAction(BattleUnitModel unit)
    {
        if (BattleSkillConditionChecker.CanAct(unit) == false)
        {
            return null;
        }

        BattleActionModel battleAction = new BattleActionModel();
        battleAction.Unit = unit;
        battleAction.IsPlayerAction = false;
        battleAction.SkillId = string.Empty;
        battleAction.ActionType = ActionType.Wait;
        battleAction.SkillType = SkillType.None;
        battleAction.TargetType = TargetType.None;
        battleAction.TargetSelectType = TargetSelectType.None;
        battleAction.TargetCount = 0;
        battleAction.Target = null;
        battleAction.TargetList = new List<BattleUnitModel>();

        return battleAction;
    }
}
