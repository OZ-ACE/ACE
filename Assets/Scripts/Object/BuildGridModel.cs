using System.Collections.Generic;
using UnityEngine;

// 런타임 그리드 상태를 Dictionary로 보관
// 저장 데이터와 상호 변환

public class BuildGridModel
{
    private readonly Dictionary<GridCoord, CellType> _cells = new Dictionary<GridCoord, CellType>();
    private readonly Dictionary<GridCoord, PlacedRoomData> _rooms = new Dictionary<GridCoord, PlacedRoomData>();
    private GridBounds _bounds;
    private int _unlockedMinFloor;

    //현재 배치된 방 전체
    public List<PlacedRoomData> GetAllRooms()
    {
        List<PlacedRoomData> result = new List<PlacedRoomData>();
        HashSet<PlacedRoomData> added = new HashSet<PlacedRoomData>();

        foreach (PlacedRoomData room in _rooms.Values)
        {
            if (added.Add(room) == true)
            {
                result.Add(room);
            }
        }
        return result;
    }



    public GridBounds Bounds { get { return _bounds; } }
    public int UnlockedMinFloor { get { return _unlockedMinFloor; } }

    //지상 지하 셀타입 초기화
    public void InitCellTypes(GridBounds bounds)
    {
        for (int floor = bounds.MinFloor; floor <= bounds.MaxFloor; floor++)
        {
            for (int column = bounds.MinColumn; column <= bounds.MaxColumn; column++)
            {
                GridCoord coord = new GridCoord(floor, column);


                if (floor >= 0)
                {
                    _cells[coord] = CellType.Sky;
                }
                else
                {
                    _cells[coord] = CellType.Earth;
                }
            }
        }
    }


    //지하층 해금상태 초기화
    public void InitUnlock(int initialMinFloor)
    {
        _unlockedMinFloor = initialMinFloor;
    }


    //칸의 상태 조회
    public CellType GetCellType(GridCoord coord)
    {
        if (_cells.TryGetValue(coord, out var type))
        {
            return type;
        }
        return CellType.Earth;
    }

    //칸의 상태 변경
    public void SetCellType(GridCoord coord, CellType type)
    {
        _cells[coord] = type;
    }


    //해당 칸을 차지한 방 조회
    public PlacedRoomData GetRoomAt(GridCoord coord)
    {
        if ( _rooms.TryGetValue(coord,out var room))
        {
            return room;
        }
        return null;
    }


    //방을 그리드에 등록
    public void AddRoom(PlacedRoomData room, List<GridCoord> occupiedCoords)
    {
        foreach (var coord in occupiedCoords)
        {
            _rooms[coord] = room;
        }
    }


    //저장 데이터로 변환
    public BuildGridData GetSaveData()
    {
        var data = new BuildGridData();

        data.UnlockedMinFloor = _unlockedMinFloor;

        foreach (var pair in _cells)
        {
            data.ChangedCells.Add(new CellStateData { Coord = pair.Key, Type = pair.Value });
        }

        var added = new HashSet<PlacedRoomData>();

        foreach (var room in _rooms.Values)
        {
            if (added.Add(room))
            {
                data.PlacedRooms.Add(room);
            }
        }
        return data;
    }


    //저장 데이터 불러오기
    public void LoadFromSaveData(BuildGridData data, GridSystem grid)
    {
        _cells.Clear();
        _rooms.Clear();

        InitCellTypes(_bounds);

        if (data.UnlockedMinFloor < 0)
        {
            _unlockedMinFloor = data.UnlockedMinFloor;
        }

        foreach (var cell in data.ChangedCells)
        {
            _cells[cell.Coord] = cell.Type;
        }

        foreach (var room in data.PlacedRooms)
        {
            RoomData roomData = GameDataManager.Inst.GetData<RoomData>(room.RoomId);
            Vector2Int size = (roomData != null) ? roomData.GetSize() : Vector2Int.one;
            AddRoom(room, grid.GetOccupiedCoords(room.Origin, size));
        }
    }


    //그리드 범위 설정
    public void SetBounds(GridBounds bounds)
    {
        _bounds = bounds;
    }


    //건설가능 판정
    public bool IsPlaceable(GridCoord originCoord, Vector2Int size, CellType requiredCellType, GridSystem grid)
    {
        return CheckPlaceable(originCoord, size, requiredCellType, grid) == PlacementResult.Success;
    }
    public PlacementResult CheckPlaceable(GridCoord originCoord, Vector2Int size, CellType requiredCellType, GridSystem grid, bool isAnyCellType = false)
    {
        List<GridCoord> coords = grid.GetOccupiedCoords(originCoord, size);

        foreach (GridCoord coord in coords)
        {
            //범위 검사
            if (_bounds.IsInside(coord) == false)
            {
                return PlacementResult.OutOfRange;
            }

            // 잠긴 층 검사
            if (IsFloorUnlocked(coord.Floor) == false)
            {
                return PlacementResult.Locked;
            }

            //충돌 검사
            if (GetRoomAt(coord) != null)
            {
                return PlacementResult.Occupied;
            }

            //셀 타입 검사(Any면 건너뜀)
            if (isAnyCellType == false && GetCellType(coord) != requiredCellType)
            {
                return PlacementResult.WrongCellType;
            }
        }
        return PlacementResult.Success;
    }

    //방 제거
    public PlacedRoomData RemoveRoomAt(GridCoord coord, GridSystem grid)
    {
        PlacedRoomData room = GetRoomAt(coord);
        if (room == null)
        {
            return null;
        }

        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(room.RoomId);
        Vector2Int size = (roomData != null) ? roomData.GetSize() : Vector2Int.one;
        List<GridCoord> coords = grid.GetOccupiedCoords(room.Origin, size);

        foreach (GridCoord c in coords)
        {
            _rooms.Remove(c);
        }

        return room;
    }




    //지하층 해금(한층씩)
    public bool TryUnlockNextFloor()
    {
        int next = _unlockedMinFloor - 1;  
        if (next < _bounds.MinFloor)
        {
            return false;  
        }

        _unlockedMinFloor = next;
        return true;
    }

    //지하층 언락 여부 확인
    public bool IsFloorUnlocked(int floor)
    {
        return floor >= _unlockedMinFloor;
    }


}