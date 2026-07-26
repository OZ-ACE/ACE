using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    public float CellWidth { get; private set; }
    public float CellHeight { get; private set; }
    public Vector2 Origin { get; private set; } //원점

    public bool IsConnect(GridCoord target, Vector2Int roomSize, List<PlacedRoomData> roomData)
    {
        if (roomData == null || roomData.Count == 0)
        {
            return true;
        }

        List<GridCoord> targetCoord = GetOccupiedCoords(target, roomSize);

        Dictionary<GridCoord, PlacedRoomData> occupiedMap = new Dictionary<GridCoord, PlacedRoomData>();

        foreach (PlacedRoomData room in roomData)
        {
            Vector2Int size = room.GetSize();
            List<GridCoord> roomCoords = GetOccupiedCoords(room.Origin, size);

            foreach (var coord in roomCoords)
            {
                if (!occupiedMap.ContainsKey(coord))
                {
                    occupiedMap.Add(coord, room);
                }
            }
        }

        foreach (var coord in targetCoord)
        {
            GridCoord leftCoord = new GridCoord(coord.Floor, coord.Column - 1);
            GridCoord rightCoord = new GridCoord(coord.Floor, coord.Column + 1);

            if (occupiedMap.ContainsKey(leftCoord) || occupiedMap.ContainsKey(rightCoord))
            {
                return true;
            }

            if (occupiedMap.TryGetValue(coord, out var currentRoom) && IsStairs(currentRoom))
            {
                return true;
            }

            GridCoord belowCoord = new GridCoord(coord.Floor - 1, coord.Column);
            if (occupiedMap.TryGetValue(belowCoord, out var belowRoom) && IsStairs(belowRoom))
            {
                return true;
            }
        }

        return false;
    }

    public bool CanDemolishRoom(PlacedRoomData targetRoom, List<PlacedRoomData> currentRooms)
    {
        if (targetRoom == null || currentRooms == null || currentRooms.Count <= 1)
        {
            return true;
        }

        Dictionary<GridCoord, PlacedRoomData> occupiedMap = new Dictionary<GridCoord, PlacedRoomData>();

        foreach (var room in currentRooms)
        {
            if (room == null)
            {
                continue;
            }

            Vector2Int size = room.GetSize();
            List<GridCoord> coords = GetOccupiedCoords(room.Origin, size);

            foreach (var c in coords)
            {
                if (!occupiedMap.ContainsKey(c))
                {
                    occupiedMap.Add(c, room);
                }
            }
        }

        List<GridCoord> targetCoords = GetOccupiedCoords(targetRoom.Origin, targetRoom.GetSize());

        int minColumn = int.MaxValue;
        int maxColumn = int.MinValue;
        int floor = targetRoom.Origin.Floor;

        foreach (var coord in targetCoords)
        {
            if (coord.Column < minColumn)
            {
                minColumn = coord.Column;
            }

            if (coord.Column > maxColumn)
            {
                maxColumn = coord.Column;
            }
        }

        GridCoord leftOuterTile = new GridCoord(floor, minColumn - 1);
        GridCoord rightOuterTile = new GridCoord(floor, maxColumn + 1);

        bool hasLeftNeighbor = occupiedMap.TryGetValue(leftOuterTile, out var leftRoom) && leftRoom.RoomInstanceId != targetRoom.RoomInstanceId;
        bool hasRightNeighbor = occupiedMap.TryGetValue(rightOuterTile, out var rightRoom) && rightRoom.RoomInstanceId != targetRoom.RoomInstanceId;

        if (hasLeftNeighbor && hasRightNeighbor)
        {
            return false;
        }

        return true;
    }

    private bool IsStairs(PlacedRoomData roomData)
    {
        if (roomData == null)
        {
            return false;
        }

        return roomData.RoomId.Contains("Stair") || roomData.RoomId.Contains("Stairs");
    }

    //그리드 시스템
    public GridSystem(float cellWidth, float cellHeight, Vector2 origin)
    {
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Origin = origin;
    }

    //원점과 비교한 상대적인 좌표, 월드좌표라고 임시 명명
    public Vector3 GetWorldPosition(GridCoord coord)
    {
        float x = Origin.x + coord.Column * CellWidth;
        float y = Origin.y + coord.Floor * CellHeight;
        return new Vector3(x, y, 0f);
    }

    // 월드좌표에서 상대적으로 가장 가까운 셀 좌표, 마우스 클릭 시 몇층, 몇칸인지 변환해주기 위함
    public GridCoord GetCoord(Vector3 world)
    {
        int column = Mathf.RoundToInt((world.x - Origin.x)/CellWidth);
        int floor = Mathf.RoundToInt((world.y - Origin.y) / CellHeight);
        return new GridCoord(floor, column);
    }

    public List<GridCoord> GetOccupiedCoords(GridCoord originCoord, Vector2Int size)
    {
        var result = new List<GridCoord>();
        for (int c = 0; c < size.x; c++)
        {

            for (int f = 0; f < size.y; f++)
            {
                result.Add(new GridCoord(originCoord.Floor + f, originCoord.Column + c));
            }
        }
        return result;
    }
}