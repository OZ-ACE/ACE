using System;
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

            SkillId.Value = skillId;

            return Status.Success;
        }

        return Status.Failure;
    }
}
