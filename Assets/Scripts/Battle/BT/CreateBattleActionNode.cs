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