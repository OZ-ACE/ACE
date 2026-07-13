using UnityEngine;


// RoomData의 필드를 해석


public static class RoomDataExtensions
{
    // 층 제한 없음
    private const int NO_FLOOR_RESTRICTION = -999;


    // W/H를 Vector2Int로
    public static Vector2Int GetSize(this RoomData room)
    {
        return new Vector2Int(room.SizeW, room.SizeH);
    }

    // RequiredCellType 문자열 → CellType
    public static CellType GetRequiredCellType(this RoomData room)
    {
        if (room.RequiredCellType == "Sky")
        {
            return CellType.Sky;
        }
        return CellType.Earth;
    }

    // 셀 타입을 가리지 않는 방인가 (계단 등)
    public static bool IsAnyCellType(this RoomData room)
    {
        return room.RequiredCellType == "Any";
    }

    // 층 제한이 있는가
    public static bool HasFloorRestriction(this RoomData room)
    {
        return room.RequiredFloor != NO_FLOOR_RESTRICTION;
    }


}