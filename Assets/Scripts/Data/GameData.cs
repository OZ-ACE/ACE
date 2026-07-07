using System;
using UnityEngine;

[Serializable]
public class GameDataBase
{
    public string ID;
}

// 임의로 넣어두었습니다.
[Serializable]
public class Hero : GameDataBase
{
    public string Name;
    public string Description;
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
