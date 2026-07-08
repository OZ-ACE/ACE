using UnityEngine;

//전투 액션 큐에 들어가는 개별 행동 정보를 저장하는 모델
public class BattleActionModel
{
    public BattleUnitModel Unit { get; set; } //행동 주체 받아옴(영웅/적), 플레이어 개입 액션은 null (playerAction.Unit = null;)

    public bool IsPlayerAction { get; set;} //true면 플레이어(요양사) 개입 액션
}
