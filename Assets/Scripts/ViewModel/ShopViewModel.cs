using System;
using System.Collections.Generic;
using UnityEngine;


public class ShopViewModel : ViewModelBase
{
    private readonly ICurrencyService _currencyService;
    private readonly InventoryViewModel _inventoryViewModel;

    public event Action<string> OnPurchaseItem;

    private List<SupportItem> _shopItems = new List<SupportItem>();
    public List<SupportItem> ShopItems { get { return _shopItems; } }

    public ShopViewModel(ICurrencyService currencyService, InventoryViewModel inventoryViewModel)
    {
        _currencyService = currencyService;
        _inventoryViewModel = inventoryViewModel;
    }

    public void InitShop()
    {
        _shopItems = GameDataManager.Inst.GetDataList<SupportItem>();
        InitStocks();
        OnPropertyChanged(nameof(ShopItems));
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ShopItems));
        OnPropertyChanged(nameof(CurrentGold));
    }

    public int CurrentGold { get { return _currencyService.CurrentGold; } }


    public bool IsPurchasable(string itemID)
    {
        return CheckPurchasable(itemID) == PurchaseResult.Success;
    }



    // ── 재고 관리 ──
    private void InitStocks()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }

        foreach (SupportItem item in _shopItems)
        {
            if (IsStockRecorded(item.ID) == false)
            {
                ShopStockData stock = new ShopStockData();
                stock.ItemID = item.ID;
                stock.RemainStock = item.stockCount;
                player.ShopStocks.Add(stock);
            }
        }
    }

    /// <summary> 해당 아이템의 재고 기록이 세이브에 있는가 </summary>
    private bool IsStockRecorded(string itemID)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        foreach (ShopStockData stock in player.ShopStocks)
        {
            if (stock.ItemID == itemID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary> 현재 남은 재고 (기록 없으면 0) </summary>
    public int GetRemainStock(string itemID)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return 0;
        }

        foreach (ShopStockData stock in player.ShopStocks)
        {
            if (stock.ItemID == itemID)
            {
                return stock.RemainStock;
            }
        }
        return 0;
    }

    /// <summary> 재고 1개 감소 </summary>
    private void DecreaseStock(string itemID)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        foreach (ShopStockData stock in player.ShopStocks)
        {
            if (stock.ItemID == itemID)
            {
                stock.RemainStock--;
                return;
            }
        }
    }

    // ── 구매 ──

    /// <summary> 구매 가능 여부 판정 (실패 이유 반환) </summary>
    public PurchaseResult CheckPurchasable(string itemID)
    {
        SupportItem item = GameDataManager.Inst.GetData<SupportItem>(itemID);
        if (item == null)
        {
            return PurchaseResult.InvalidItem;
        }

        if (GetRemainStock(itemID) <= 0)
        {
            return PurchaseResult.OutOfStock;
        }

        if (_currencyService.IsAffordable(item.Price) == false)
        {
            return PurchaseResult.NotEnoughGold;
        }

        return PurchaseResult.Success;
    }

    /// <summary> 구매 시도. 성공 시 Gold 차감 + 재고 감소 + 인벤토리 추가 + 저장 </summary>
    public PurchaseResult TryPurchase(string itemID)
    {
        PurchaseResult result = CheckPurchasable(itemID);
        if (result != PurchaseResult.Success)
        {
            return result;
        }

        SupportItem item = GameDataManager.Inst.GetData<SupportItem>(itemID);

        // 1) Gold 차감
        if (_currencyService.TrySpend(item.Price) == false)
        {
            return PurchaseResult.NotEnoughGold;
        }

        // 2) 재고 감소
        DecreaseStock(itemID);

        // 3) 인벤토리 추가
        _inventoryViewModel.AddItem(itemID, 1);

        // 4) 알림 + 저장
        if (OnPurchaseItem != null)
        {
            OnPurchaseItem.Invoke(itemID);
        }
        OnPropertyChanged(nameof(CurrentGold));

        SaveShop();

        Debug.Log($"[ShopViewModel] 구매 성공: {item.itemName} ({item.Price}G) → 잔여 재고 {GetRemainStock(itemID)}");
        return PurchaseResult.Success;
    }

    /// <summary> 상점 상태 저장 </summary>
    private void SaveShop()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }
        SaveManager.Inst.RequestSaveData(SaveManager.Inst.CurrentSlotIndex, player);
    }
}