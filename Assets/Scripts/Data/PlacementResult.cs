

//배치 판정
public enum PlacementResult
{
    Success,          // 배치 가능
    OutOfRange,       // 그리드 범위를 벗어남
    Occupied,         // 이미 다른 방이 있음 (충돌)
    WrongCellType,    // 방이 요구하는 셀 타입과 맞지 않음
}
