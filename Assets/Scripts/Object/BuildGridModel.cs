using System.Collections.Generic;
using UnityEngine;

// 런타임 그리드 상태를 Dictionary로 보관
// 저장 데이터와 상호 변환

public class BuildGridModel
{
    private readonly Dictionary<GridCoord, CellType> _cells = new Dictionary<GridCoord, CellType>();
    private readonly Dictionary<GridCoord, PlacedRoomData> _rooms = new Dictionary<GridCoord, PlacedRoomData>();
    private GridBounds _bounds;



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
    public PlacementResult CheckPlaceable(GridCoord originCoord, Vector2Int size, CellType requiredCellType, GridSystem grid)
    {
        List<GridCoord> coords = grid.GetOccupiedCoords(originCoord, size);

        foreach (GridCoord coord in coords)
        {
            //범위 검사
            if (_bounds.IsInside(coord) == false)
            {
                return PlacementResult.OutOfRange;
            }

            //충돌 검사
            if (GetRoomAt(coord) != null)
            {
                return PlacementResult.Occupied;
            }

            //셀 타입 검사
            if (GetCellType(coord) != requiredCellType)
            {
                return PlacementResult.WrongCellType;
            }
        }
        return PlacementResult.Success;
    }



}