using System.Collections.Generic;
using UnityEngine;

//전투 참가자(영웅, 적)의 턴 순서 결정에 필요한 런타임 정보를 담는 모델
public class BattleUnitModel
{
    public string ID { get; set; }
    public bool IsHero { get; set; } //영웅인지 적인지 판별
    public int Speed { get; set; } //턴 순서 1차 판별 요소
    public int AttackPower { get; set; } //Speed가 동률일 시 공격력 높은 순으로 출격, 공격력도 동률일 시 추가 로직 만들어야 함 ToDo..

    public int MaxHp { get; set; }

    public int CurrentHp { get; set; }

    public bool IsActionBlocked { get; set; } //BT에서 행동 가능 여부를 검사할 때 사용

    public List<string> SkillIdList { get; set; } = new List<string>(); //BT에서 사용 가능한 스킬 후보를 확인할 때 사용

    public string LastSkillId { get; set; } //직전에 사용한 스킬 ID (반복 카운트 기준)
    public int RepeatSkillCount { get; set; } //같은 스킬 연속 사용 횟수

    public string ActivePenaltyId { get; set; } //현재 발동 중인 페널티 ID, 없으면 null
    public int PenaltyRemainingRounds { get; set; } //페널티 지속 남은 라운드 수

    public int AttackPowerModifierPercent { get; set; } = 100; //공격력 배율(%), 100=기본 / 130=+30% / 70=-30%
    public int ModifierRemainingRounds { get; set; } //공격력 배율 지속 남은 라운드 수

    //CurrentHp를 순간순간 자동 계산해줌
    public bool IsDefeated
    {
        get
        {
            return CurrentHp <= 0;
        }
    }
}
