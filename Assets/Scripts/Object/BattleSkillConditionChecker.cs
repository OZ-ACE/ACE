using System.Collections.Generic;

public static class BattleSkillConditionChecker
{
    //유닛이 기본적으로 행동 가능한 상태인지 검사한다.
    public static bool CanAct(BattleUnitModel unit)
    {
        if (unit == null)
        {
            return false;
        }

        if (unit.IsDefeated == true)
        {
            return false;
        }

        if (unit.IsActionBlocked == true)
        {
            return false;
        }

        return true;
    }

    //스킬 사용에 필요한 기본 조건을 검사
    //Wait는 스킬 데이터에 포함하지 않으므로 skillId와 targetList가 없어도 사용 가능
    public static bool CanUseSkill(
        BattleUnitModel unit,
        string skillId,
        ActionType actionType,
        List<BattleUnitModel> targetList)
    {
        if (CanAct(unit) == false)
        {
            return false;
        }

        if (actionType == ActionType.Wait)
        {
            return true;
        }

        if (string.IsNullOrEmpty(skillId) == true)
        {
            return false;
        }

        if (targetList == null || targetList.Count <= 0)
        {
            return false;
        }

        if (HasAliveTarget(targetList) == false)
        {
            return false;
        }

        return true;
    }

    //대상 목록에 살아있는 대상이 하나라도 있는지 검사
    private static bool HasAliveTarget(List<BattleUnitModel> targetList)
    {
        foreach (BattleUnitModel target in targetList)
        {
            if (target == null)
            {
                continue;
            }

            if (target.IsDefeated == true)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
