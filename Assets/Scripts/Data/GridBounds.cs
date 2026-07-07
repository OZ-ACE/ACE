using System;

// 건설 가능 영역 정의


[Serializable]

public class GridBounds
{
    public int MinFloor;
    public int MaxFloor;
    public int MinColumn;
    public int MaxColumn;

    public GridBounds(int minFloor, int maxFloor, int minColumn, int maxColumn)
    {
        MinFloor = minFloor;
        MaxFloor = maxFloor;
        MinColumn = minColumn;
        MaxColumn = maxColumn;
    }

    // 건설 가능 영역 내에 있는가 판단
    public bool IsInside(GridCoord coord)
    {
        if (coord.Floor < MinFloor || coord.Floor > MaxFloor)
        {
            return false;
        }
        if (coord.Column < MinColumn || coord.Column > MaxColumn)
        {
            return false;
        }
        return true;
    }


}
