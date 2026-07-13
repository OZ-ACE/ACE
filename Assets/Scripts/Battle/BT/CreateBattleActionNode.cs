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
    story: "[Context] creates battle action with [SkillId]",
    category: "Battle",
    id: "Battle_CreateBattleActionNode")]
public partial class CreateBattleActionNode : Action
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
            Debug.LogWarning($"[CreateBattleActionNode] 지원하지 않는 스킬 데이터 형식입니다. SkillId: {SkillId.Value}");

            return Status.Failure;
        }

        if (Enum.TryParse(actionTypeText, out ActionType actionType) == false)
        {
            Debug.LogWarning($"[CreateBattleActionNode] ActionType 변환에 실패했습니다. Value: {actionTypeText}");

            return Status.Failure;
        }

        if (Enum.TryParse(skillTypeText, out SkillType skillType) == false)
        {
            Debug.LogWarning($"[CreateBattleActionNode] SkillType 변환에 실패했습니다. Value: {skillTypeText}");

            return Status.Failure;
        }

        if (Enum.TryParse(targetTypeText, out TargetType targetType) == false)
        {
            Debug.LogWarning($"[CreateBattleActionNode] TargetType 변환에 실패했습니다. Value: {targetTypeText}");

            return Status.Failure;
        }

        if (Enum.TryParse(targetSelectTypeText, out TargetSelectType targetSelectType) == false)
        {
            Debug.LogWarning($"[CreateBattleActionNode] TargetSelectType 변환에 실패했습니다. Value: {targetSelectTypeText}");

            return Status.Failure;
        }

        BattleActionModel battleAction;

        bool isCreated = BattleActionFactory.TryCreateSkillAction(
            context.Unit,
            SkillId.Value,
            actionType,
            skillType,
            targetType,
            targetSelectType,
            targetCount,
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