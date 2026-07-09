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
//영웅 전투 스탯 데이터
[Serializable]
public class HeroBattleData : GameDataBase
{
    public string position;
    public int maxHp;
    public int speed;
    public int attackPower;
    public int defensePower;
}

//적 전투 스탯 데이터
[Serializable]
public class EnemyBattleData : GameDataBase
{
    public int maxHp;
    public int speed;
    public int attackPower;
    public int defensePower;
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
    public string itemName;
    public string itemNameEn;
    public string targetPaneltyId;
    public string targetPaneltyName;
    public int energyCost;
    public int stockCount;
    public int Price;
    public string Description;
}
