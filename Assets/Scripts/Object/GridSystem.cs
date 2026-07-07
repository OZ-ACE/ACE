using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GridSystem
{
    public float CellWidth { get; private set; }
    public float CellHeight { get; private set; }
    public Vector2 Origin { get; private set; } //원점



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



