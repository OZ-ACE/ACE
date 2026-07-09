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

    //CurrentHp를 순간순간 자동 계산해줌
    public bool IsDefeated
    {
        get
        {
            return CurrentHp <= 0;
        }
    }
}
