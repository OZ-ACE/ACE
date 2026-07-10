using JetBrains.Annotations;
using System;
using UnityEngine;

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

[Serializable]
public class RoomData : GameDataBase
{
    public string Name;
    public string Description;
    public int SizeW;
    public int SizeH;
    public int BuildCost;
    public string RequiredCellType;
    public string EffectType;
    public string PrefabPath;


    // W/H를 Vector2Int로 묶어 반환
    public Vector2Int GetSize()
    {
        return new Vector2Int(SizeW, SizeH);
    }

    // RequiredCellType 문자열 → CellType enum 변환
    public CellType GetRequiredCellType()
    {
        if (RequiredCellType == "Sky")
        {
            return CellType.Sky;
        }

        return CellType.Earth;
    }
}

[Serializable]
public class SupportItem : GameDataBase
{
    public string ItemName;
    public string ItemNameEn;
    public string TargetPaneltyId;
    public string TargetPaneltyName;
    public int EnergyCost;
    public int StockCount;
    public string Description;
}
