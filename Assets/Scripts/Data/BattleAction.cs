using System.Collections.Generic;


//BT가 선택한 전투 행동 결과를 전투 시스템에 전달하기 위한 데이터 클래스
//Wait는 스킬 데이터에 포함하지 않고, 행동불가 상태이거나 선택 가능한 스킬이 없을 때 기본 BattleAction으로 생성
public class BattleAction
{
    //행동하는 유닛
    public BattleUnit Unit { get; private set; }

    //사용할 스킬 ID
    public string SkillId { get; private set; }

    //BT 행동 분류
    public ActionType ActionType { get; private set; }

    //실제 스킬 효과 타입
    public SkillType SkillType { get; private set; }

    //타겟 범위
    public TargetType TargetType { get; private set; }

    //타겟 선택 방식
    public TargetSelectType TargetSelectType { get; private set; }

    //타겟 수
    //TargetCount가 -1이면 조건에 맞는 전체 대상을 의미
    public int TargetCount { get; private set; }

    //단일 대상
    public BattleUnit Target { get; private set; }

    //다중 대상
    public List<BattleUnit> TargetList { get; private set; }

    //영웅 행동 여부
    public bool IsHeroAction { get; private set; }

    public BattleAction(
        BattleUnit unit,
        string skillId,
        ActionType actionType,
        SkillType skillType,
        TargetType targetType,
        TargetSelectType targetSelectType,
        int targetCount,
        BattleUnit target,
        List<BattleUnit> targetList,
        bool isHeroAction)
    {
        Unit = unit;
        SkillId = skillId;

        ActionType = actionType;
        SkillType = skillType;

        TargetType = targetType;
        TargetSelectType = targetSelectType;
        TargetCount = targetCount;

        Target = target;

        if (targetList == null)
        {
            TargetList = new List<BattleUnit>();
        }
        else
        {
            TargetList = targetList;
        }

        IsHeroAction = isHeroAction;
    }
}
