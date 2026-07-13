using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Create Battle Action",
    story: "[Context] creates [ActionType] [SkillType] action with [SkillId], [TargetType], [TargetSelectType], [TargetCount]",
    category: "Battle",
    id: "Battle_CreateBattleActionNode")]
public partial class CreateBattleActionNode : Action
{
    [SerializeReference] public BlackboardVariable<BattleBTContext> Context;
    [SerializeReference] public BlackboardVariable<string> SkillId;
    [SerializeReference] public BlackboardVariable<ActionType> ActionType;
    [SerializeReference] public BlackboardVariable<SkillType> SkillType;
    [SerializeReference] public BlackboardVariable<TargetType> TargetType;
    [SerializeReference] public BlackboardVariable<TargetSelectType> TargetSelectType;
    [SerializeReference] public BlackboardVariable<int> TargetCount;

    protected override Status OnStart()
    {
        if (Context == null || Context.Value == null)
        {
            return Status.Failure;
        }

        BattleBTContext context = Context.Value;

        if (SkillId == null || string.IsNullOrEmpty(SkillId.Value))
        {
            Debug.LogWarning("[CreateBattleActionNode] SkillId가 비어 있습니다.");
            return Status.Failure;
        }

        if (context.Unit == null)
        {
            Debug.LogWarning("[CreateBattleActionNode] 전투 유닛 정보가 없습니다.");
            return Status.Failure;
        }

        GameDataBase skillData;

        if (context.Unit.IsHero)
        {
            skillData = GameDataManager.Inst.GetData<HeroSkill>(SkillId.Value);
        }
        else
        {
            skillData = GameDataManager.Inst.GetData<EnemySkill>(SkillId.Value);
        }

        if (skillData == null)
        {
            Debug.LogWarning($"[CreateBattleActionNode] 스킬 데이터를 찾을 수 없습니다. SkillId: {SkillId.Value}");

            return Status.Failure;
        }

        BattleActionModel battleAction;

        bool isCreated = BattleActionFactory.TryCreateSkillAction(
            context.Unit,
            SkillId.Value,
            ActionType.Value,
            SkillType.Value,
            TargetType.Value,
            TargetSelectType.Value,
            TargetCount.Value,
            context.HeroList,
            context.EnemyList,
            out battleAction);

        if (isCreated == false)
        {
            battleAction = BattleActionFactory.CreateWaitAction(context.Unit);
        }

        if (battleAction == null)
        {
            return Status.Failure;
        }

        context.SetCreatedBattleAction(battleAction);

        return Status.Success;
    }
}