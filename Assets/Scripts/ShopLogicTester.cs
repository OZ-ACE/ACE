using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [임시] 상점 구매 로직 검증용. 빈 오브젝트에 붙여 Play하면 콘솔에 결과 출력.
/// ※ 검증 후 삭제.
/// </summary>
public class ShopLogicTester : MonoBehaviour
{
    private void Start()
    {
        // 테스트용 Gold 지급
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        player.Gold = 250;
        Debug.Log($"[Tester] 시작 Gold: {player.Gold}");

        // 인벤토리 뷰모델 준비 (상점이 아이템 넣을 대상)
        InventoryViewModel inventoryVM = new InventoryViewModel();
        inventoryVM.Init(player.Inventory);

        // 상점 서비스 + 뷰모델 생성
        ICurrencyService currency = GameManager.Inst.CurrencyService;
        ShopService shopService = new ShopService(currency);
        ShopViewModel shopVM = shopService.CreateShopViewModel(inventoryVM);
        shopVM.InitShop();

        Debug.Log($"[Tester] 판매 목록: {shopVM.ShopItems.Count}개");

        // 1) 돋보기안경(100G) 구매 → Success 예상
        PurchaseResult r1 = shopVM.TryPurchase("item_01");
        Debug.Log($"[Tester] item_01 구매 → {r1} (Success 예상), Gold: {currency.CurrentGold}");

        // 2) 파스(150G) 구매 → Success 예상 (250-100=150 남음)
        PurchaseResult r2 = shopVM.TryPurchase("item_02");
        Debug.Log($"[Tester] item_02 구매 → {r2} (Success 예상), Gold: {currency.CurrentGold}");

        // 3) 또 구매 시도 → NotEnoughGold 예상 (Gold 0)
        PurchaseResult r3 = shopVM.TryPurchase("item_01");
        Debug.Log($"[Tester] item_01 재구매 → {r3} (NotEnoughGold 예상)");

        // 4) Gold 충전 후 재고 소진 테스트
        currency.AddGold(1000);
        PurchaseResult r4 = shopVM.TryPurchase("item_01");   // 재고 2 → 1개 남음(1번에서 1개 삼)
        Debug.Log($"[Tester] item_01 구매(재고 소진) → {r4} (Success 예상), 잔여 재고: {shopVM.GetRemainStock("item_01")}");

        PurchaseResult r5 = shopVM.TryPurchase("item_01");   // 재고 0
        Debug.Log($"[Tester] item_01 구매(재고 없음) → {r5} (OutOfStock 예상)");

        // 5) 존재하지 않는 아이템
        PurchaseResult r6 = shopVM.TryPurchase("item_99");
        Debug.Log($"[Tester] item_99 구매 → {r6} (InvalidItem 예상)");

        // 인벤토리 확인
        Debug.Log($"[Tester] 인벤토리 item_01 개수: {inventoryVM.GetItemCount("item_01")}");
        Debug.Log($"[Tester] 인벤토리 item_02 개수: {inventoryVM.GetItemCount("item_02")}");
    }
}