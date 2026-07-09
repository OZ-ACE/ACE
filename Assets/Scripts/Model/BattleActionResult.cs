
//플레이어 개입 턴 이후 확정되는 영웅 행동의 처리 결과
public enum BattleActionResult
{
    None = 0,
    Success,
    Reinforce,
    ChangeUnit, //다른 영웅으로 행동 변경(에너지 많이 소모)
    Fail
}
