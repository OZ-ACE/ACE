using System;
using System.Collections.Generic;

//배치된 방 1개의 저장 정보
[Serializable]
public class PlacedRoomData
{
    public string RoomId;    // RoomData.ID (어떤 방인지)
    public GridCoord Origin; // 방의 좌하단 기준 좌표 (어디에)
}

//기본값과 다른 칸의 상태만 저장 (예: 파낸 칸)
[Serializable]
public class CellStateData
{
    public GridCoord Coord;
    public CellType Type;
}

// 건설 그리드 전체 저장 데이터. SaveManager가 이 덩어리를 JSON으로 저장/복원.
[Serializable]
public class BuildGridData
{
    public List<PlacedRoomData> PlacedRooms = new List<PlacedRoomData>();
    public List<CellStateData> ChangedCells = new List<CellStateData>();
    public int UnlockedMinFloor;  
}


