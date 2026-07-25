using System;
using System.Collections.Generic;

//배치된 방 1개의 저장 정보
[Serializable]
public class PlacedRoomData
{
    public long RoomInstanceId;
    public string RoomId;    // RoomData.ID (어떤 방인지)
    public GridCoord Origin; // 방의 좌하단 기준 좌표 (어디에)

    private HashSet<string> _reserveHero = new HashSet<string>();
    private HashSet<string> _currentHero = new HashSet<string>();

    public int CurrentCount => _currentHero.Count;

    public event Action OnUserCountChanged;

    public int MaxCapacity
    {
        get
        {
            var roomData = GameDataManager.Inst.GetData<RoomData>(RoomId);
            return roomData.MaxCapacity;
        }
    }

    public bool CanUse()
    {
        return (_reserveHero.Count + _currentHero.Count) < MaxCapacity;
    }

    public bool ReserveSpot(string heroID)
    {
        if (!CanUse())
        {
            return false;
        }

        _reserveHero.Add(heroID);
        return true;
    }

    public void CancelReservation(string heroID)
    {
        _reserveHero.Remove(heroID);
    }

    public bool EnterRoom(string heroID)
    {
        _reserveHero.Remove(heroID);

        bool added = _currentHero.Add(heroID);
        if (added)
        {
            OnUserCountChanged?.Invoke();
        }
        return added;
    }

    public void LeaveRoom(string heroID)
    {
        _reserveHero.Remove(heroID);

        bool removed = _currentHero.Remove(heroID);
        if (removed)
        {
            OnUserCountChanged?.Invoke();
        }
    }
}

//기본값과 다른 칸의 상태만 저장
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


