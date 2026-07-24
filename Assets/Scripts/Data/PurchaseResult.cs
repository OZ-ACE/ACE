public enum PurchaseResult
{
    Success,          // 구매 성공
    NotEnoughGold,    // Gold 부족
    OutOfStock,       // 재고 없음
    InvalidItem,      // 존재하지 않는 아이템
    AlreadyPurchased
}