using System;
using System.Collections.Generic;

[Serializable]
public class GameDataBase
{
    public string ID;
}

[Serializable]
public class Dialogue : GameDataBase
{
    public string NextID;
    public string Speaker;
    public string SpeakerPath;
    public string Content;
    public string Background;
    public string BGM;
    public string SFX;
}

[Serializable]
public class HeroData : GameDataBase
{
    public string HeroName;
    public string HeroNameEn;
    public string Age;
    public string Remarks;
    public string MainSkillId;
    public int CandidateWeight;
    public string ProfileImage;
    public List<string> PenaltyID;
}

[Serializable]
public class EnemyData : GameDataBase
{
    public string EnemyName;
    public string EnemyNameEn;
    public string ProfileImage;
}

//영웅 전투 스탯 데이터
[Serializable]
public class HeroBattleData : GameDataBase
{
    public string Position;
    public int MaxHp;
    public int Speed;
    public int AttackPower;
    public int DefensePower;
}

//적 전투 스탯 데이터
[Serializable]
public class EnemyBattleData : GameDataBase
{
    public int MaxHp;
    public int Speed;
    public int AttackPower;
    public int DefensePower;
}

//영웅 스킬 데이터
[Serializable]
public class HeroSkill : GameDataBase
{
    public string HeroId;
    public string HeroName;
    public string SkillName;
    public string SkillNameEn;
    public string ActionType;
    public string SkillType;
    public string AttackRangeType;
    public string TargetSelectType;
    public string TargetType;
    public int TargetCount;
    public int CoolTime;
    public int Power;
    public string SkillDescription;
}

//적 스킬 데이터
[Serializable]
public class EnemySkill : GameDataBase
{
    public string EnemyId;
    public string EnemyName;
    public string SkillName;
    public string SkillNameEn;
    public string ActionType;
    public string SkillType;
    public string TargetSelectType;
    public string TargetType;
    public int TargetCount;
    public int CoolTime;
    public int Power;
    public string SkillDescription;
}

[Serializable]
public class RoomData : GameDataBase
{
    public string Name;
    public string Description;
    public int SizeW;
    public int SizeH;
    public int BuildCost;
    public string RequiredCellType;
    public int RequiredFloor;       
    public string EffectType;
    public string SimpleDescription;
    public string EffectDescription;
    public string PrefabPath;
    public int MaxCapacity;
    public string RequiredRoomID;
}

[Serializable]
public class SupportItem : GameDataBase
{
    public string ItemName;
    public string ItemNameEn;
    public string ItemCategory;
    public string TargetPenaltyId;
    public string TargetPenaltyName;
    public int HealAmount;
    public int EnergyCost;
    public int StockCount;
    public int Price;
    public string Description;
}

//전투 페널티 데이터
[Serializable]
public class Penalty : GameDataBase
{
    public string PenaltyName;
    public string PenaltyNameEn;
    public string TriggerSkillId;
    public string TriggerSkillName;
    public int TriggerCount;
    public string EffectType;
    public int EffectValue;
    public int DurationRounds;
    public string Description;
}

[Serializable]
public class Loading : GameDataBase
{
    public string Content;
}

//전투 보상/프라임레벨 계산에 쓰이는 밸런스 수치 데이터 (단일 row, ID=default)
[Serializable]
public class BattleConfig : GameDataBase
{
    public int BaseRewardAmount;
    public int DoubleBonusPercent; //200 = x2.0배
    public int HalfBonusPercent; //150 = x1.5배
    public int DoubleBonusRoundThreshold;
    public int HalfBonusRoundThreshold;
    public int ParticipateCountPerLevel;
}

//퀘스트 관련 데이터
[Serializable]
public class QuestData : GameDataBase
{
    public string QuestName;
    public string Description;
    public string ConditionType;      // enum 문자열
    public string ConditionTargetID;  // 방 ID / 아이템 ID, 빈 값이면 대상 무관
    public int ConditionCount;
    public string RewardType;         // enum 문자열
    public int RewardAmount;
    public string RequiredQuestID;    // 선행 퀘스트, 없으면 빈 문자열
    public string DialogueID;
}


[Serializable]
public class FurnitureData : GameDataBase
{
    public string FurnitureName;
    public string Description;

    public string HeroId;
    public int Price;

    public string InstallPointId;
}

[Serializable]
public class Tutorial : GameDataBase
{
    public string GroupID;       // 같은 튜토리얼 묶음 (여러 장)
    public string TutorialName;  // 페이지 제목
    public string Text;
    public string TriggerType;   // 그룹의 첫 장에만 채우면 됨
    public string TriggerValue;  // QuestReward면 퀘스트 ID
    public int Order;            // 그룹 내 페이지 순서
    public string ImagePath;
}

[Serializable]
public class HeroRequestData : GameDataBase
{
    public string HeroId;
    public string RequestName;
    public string Description;

    public ScheduleState RequiredSchedule;
    public int RequiredHour;

    public int RewardAffection;
    public int RewardSatisfaction;
}