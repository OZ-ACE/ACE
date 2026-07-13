using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Select Enemy Skill",
    story: "[Context] selects enemy skill into [SkillId]",
    category: "Battle",
    id: "Battle_SelectEnemySkillNode")]
public partial class SelectEnemySkillNode : Action
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

        if (unit.IsHero)
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

            EnemySkill enemySkill = GameDataManager.Inst.GetData<EnemySkill>(skillId);

            if (enemySkill == null)
            {
                continue;
            }

            SkillId.Value = skillId;

            return Status.Success;
        }

        return Status.Failure;
    }
}
