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
    story: "[Unit] creates battle action with [SkillId]",
    category: "Battle",
    id: "Battle_CreateBattleActionNode")]
public partial class CreateBattleActionNode : Action
{
    [SerializeReference] public BlackboardVariable<string> SkillId;
    [SerializeReference] public BlackboardVariable<ActionType> ActionType;
    [SerializeReference] public BlackboardVariable<SkillType> SkillType;
    [SerializeReference] public BlackboardVariable<TargetType> TargetType;
    [SerializeReference] public BlackboardVariable<TargetSelectType> TargetSelectType;
    [SerializeReference] public BlackboardVariable<int> TargetCount;

    protected override Status OnStart()
    {

        return Status.Success;
    }
}