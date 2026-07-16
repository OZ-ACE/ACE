using System;
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

    //스킬 ID만으로 스킬 데이터를 조회해서 BattleActionModel 생성을 시도한다 (지원하기 등 스킬 재구성이 필요한 경우 사용)
    public static bool TryCreateSkillActionFromId(
        BattleUnitModel unit,
        string skillId,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        out BattleActionModel battleAction)
    {
        battleAction = null;

        if (unit == null || string.IsNullOrEmpty(skillId))
        {
            return false;
        }

        GameDataBase skillData;

        if (unit.IsHero)
        {
            skillData = GameDataManager.Inst.GetData<HeroSkill>(skillId);
        }
        else
        {
            skillData = GameDataManager.Inst.GetData<EnemySkill>(skillId);
        }

        if (skillData == null)
        {
            return false;
        }

        string actionTypeText;
        string skillTypeText;
        string targetTypeText;
        string targetSelectTypeText;
        int targetCount;

        if (skillData is HeroSkill heroSkill)
        {
            actionTypeText = heroSkill.ActionType;
            skillTypeText = heroSkill.SkillType;
            targetTypeText = heroSkill.TargetType;
            targetSelectTypeText = heroSkill.TargetSelectType;
            targetCount = heroSkill.TargetCount;
        }
        else if (skillData is EnemySkill enemySkill)
        {
            actionTypeText = enemySkill.ActionType;
            skillTypeText = enemySkill.SkillType;
            targetTypeText = enemySkill.TargetType;
            targetSelectTypeText = enemySkill.TargetSelectType;
            targetCount = enemySkill.TargetCount;
        }
        else
        {
            return false;
        }

        if (Enum.TryParse(actionTypeText, out ActionType actionType) == false)
        {
            return false;
        }

        if (Enum.TryParse(skillTypeText, out SkillType skillType) == false)
        {
            return false;
        }

        if (Enum.TryParse(targetTypeText, out TargetType targetType) == false)
        {
            return false;
        }

        if (Enum.TryParse(targetSelectTypeText, out TargetSelectType targetSelectType) == false)
        {
            return false;
        }

        return TryCreateSkillAction(
            unit,
            skillId,
            actionType,
            skillType,
            targetType,
            targetSelectType,
            targetCount,
            heroList,
            enemyList,
            out battleAction);
    }
}
