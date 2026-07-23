using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Select Hero Skill",
    story: "[Context] selects hero skill into [SkillId]",
    category: "Battle",
    id: "Battle_SelectHeroSkillNode")]
public partial class SelectHeroSkillNode : Action
{
    [SerializeReference] public BlackboardVariable<BattleBTContext> Context;
    [SerializeReference] public BlackboardVariable<string> SkillId;

    protected override Status OnStart()
    {
        if (Context == null || Context.Value == null)
        {
            return Status.Failure;
        }

        BattleBTContext context = Context.Value;
        BattleUnitModel unit = context.Unit;

        if (BattleSkillConditionChecker.CanAct(unit) == false)
        {
            return Status.Failure;
        }

        if (unit.IsHero == false)
        {
            return Status.Failure;
        }

        if (unit.SkillIdList == null || unit.SkillIdList.Count <= 0)
        {
            return Status.Failure;
        }

        List<string> availableSkillIdList = new List<string>();

        foreach (string skillId in unit.SkillIdList)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                continue;
            }

            HeroSkill heroSkill = GameDataManager.Inst.GetData<HeroSkill>(skillId);

            if (heroSkill == null)
            {
                continue;
            }

            if (Enum.TryParse(heroSkill.ActionType, out ActionType actionType) == false)
            {
                continue;
            }

            if (Enum.TryParse(
                heroSkill.TargetSelectType,
                out TargetSelectType targetSelectType) == false)
            {
                continue;
            }

            List<BattleUnitModel> targetList = BattleTargetSelector.SelectTargets(
                unit,
                context.HeroList,
                context.EnemyList,
                targetSelectType,
                heroSkill.TargetCount);

            bool canUseSkill = BattleSkillConditionChecker.CanUseSkill(
                unit,
                skillId,
                actionType,
                targetList);

            if (canUseSkill == false)
            {
                continue;
            }

            availableSkillIdList.Add(skillId);
        }

        if (availableSkillIdList.Count <= 0)
        {
            return Status.Failure;
        }

        int selectedIndex = UnityEngine.Random.Range(
            0,
            availableSkillIdList.Count);

        SkillId.Value = availableSkillIdList[selectedIndex];

        return Status.Success;
    }
}
