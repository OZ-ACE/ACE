using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Create Wait Action",
    story: "[Context] creates wait action",
    category: "Battle",
    id: "Battle_CreateWaitActionNode")]
public partial class CreateWaitActionNode : Action
{
    [SerializeReference] public BlackboardVariable<BattleBTContext> Context;

    protected override Status OnStart()
    {
        if (Context == null || Context.Value == null)
        {
            return Status.Failure;
        }

        BattleBTContext context = Context.Value;

        if (context.Unit == null)
        {
            return Status.Failure;
        }

        BattleActionModel battleAction = BattleActionFactory.CreateWaitAction(context.Unit);

        if (battleAction == null)
        {
            return Status.Failure;
        }

        context.SetCreatedBattleAction(battleAction);

        return Status.Success;
    }
}
