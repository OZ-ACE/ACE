
//BT가 행동을 고를 때 사용하는 최상위 행동 분류
public enum ActionType
{
    None = 0,
    Attack,
    Defend,
    Support,
    Wait
}

//실제 스킬의 효과 타입
public enum SkillType
{
    None = 0,
    Attack,
    Buff,
    Debuff
}

//타겟의 범위
public enum TargetType
{
    None = 0,
    Single,
    Multi
}

//타겟 선택 방식
public enum TargetSelectType
{
    None = 0,
    RandomEnemy,
    LowestHpEnemy,
    RandomFriendly,
    LowestHpFriendly,
    Self
}

//SetActionResult 처리 결과 - 실패 사유를 구분 용도
public enum ActionApplyResult
{
    Success = 0,
    InsufficientEnergy,
    TargetNotFound
}
