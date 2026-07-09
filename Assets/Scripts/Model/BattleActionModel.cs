using System.Collections.Generic;
using UnityEngine;

//전투 액션 큐에 들어가는 개별 행동 정보를 저장하는 모델
public class BattleActionModel
{
    public BattleUnitModel Unit { get; set; } //행동 주체 받아옴(영웅/적), 플레이어 개입 액션은 null (playerAction.Unit = null;)

    public bool IsPlayerAction { get; set; } //true면 플레이어(요양사) 개입 액션

    public string SkillId { get; set; } //BT에서 선택한 스킬 ID

    public ActionType ActionType { get; set; } = ActionType.None; //BT 행동 분류

    public SkillType SkillType { get; set; } = SkillType.None; //실제 스킬 효과 타입

    public TargetType TargetType { get; set; } = TargetType.None; //타겟 범위

    public TargetSelectType TargetSelectType { get; set; } = TargetSelectType.None; //타겟 선택 방식

    public int TargetCount { get; set; } //TargetCount가 -1이면 조건에 맞는 전체 대상을 의미

    public BattleUnitModel Target { get; set; } //단일 대상

    public List<BattleUnitModel> TargetList { get; set; } = new List<BattleUnitModel>(); //다중 대상

    public BattleActionResult Result { get; set; } = BattleActionResult.None; //개입 턴 후 처리결과. 기본값 미확정none
}
