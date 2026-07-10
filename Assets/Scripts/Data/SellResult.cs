/// <summary>
/// 아이템 판매(되팔기) 시도 결과.
/// </summary>
public enum SellResult
{
    Success,       // 판매 성공
    NoItem,        // 인벤토리에 해당 아이템 없음
    InvalidItem,   // 존재하지 않는 아이템
}