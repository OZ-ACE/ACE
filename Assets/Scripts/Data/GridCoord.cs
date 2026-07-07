using System;

[Serializable]
public struct GridCoord : IEquatable<GridCoord>
{

    //좌표(Floor = 층, Column = 열)
    //Floor : 양수=지상, 0=1층(표면), 음수=지하
    public int Floor;
    public int Column;

    public GridCoord(int floor, int column)
    {
        Floor = floor;
        Column = column;
    }


    //동일 좌표인지 비교
    public bool Equals(GridCoord other)
    {
        return Floor == other.Floor && Column == other.Column;
    }

    public override bool Equals(object obj)
    {
        return obj is GridCoord other && Equals(other);
    }

    //IEquatable를 구현할땐 Equals + GetHashCode 한 세트로 맞춰줘야함 
    //층에 소수를 곱하고 열이랑 섞어서, 칸마다 안 겹치는 지문을 만듬
    public override int GetHashCode()
    {
        return (Floor * 397) ^ Column;
    }


    // == 를 쓸때 int나 string 같은 건 미리 정의되어 있지만 GridCoord는 없어서 정의해주는 것으로
    // 추후 (a.Equals(b)) 형태가 아닌 a == b 로 쓰고 싶어서 추가함
    public static bool operator ==(GridCoord a, GridCoord b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(GridCoord a, GridCoord b)
    {
        return !a.Equals(b);
    }

    public override string ToString()
    {
        return $"(F:{Floor}, C:{Column}";
    }


}