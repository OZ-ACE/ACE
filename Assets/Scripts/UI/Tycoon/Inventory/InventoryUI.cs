using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_SelectItem;
    [SerializeField] private TextMeshProUGUI Text_SelectDescription;

    [SerializeField] private Transform Transform_SlotParent;
    [SerializeField] private InventorySlot Prefab_InventorySlot;

    [Header("판매")]
    [SerializeField] private Button Button_Sell;
    [SerializeField] private TextMeshProUGUI Text_SellPrice;

    private ShopViewModel _shopVM;
    private InventoryViewModel _inventoryVM;

    private ObjectPooling<InventorySlot> _slotPool;
    private List<InventorySlot> _activeSlots = new List<InventorySlot>();

    private void Awake()
    {
        _slotPool = new ObjectPooling<InventorySlot>(Prefab_InventorySlot, Transform_SlotParent, 12, 30);
    }

    private void OnEnable()
    {
        if (_inventoryVM == null)
        {
            _activeSlots = new List<InventorySlot>();
            _inventoryVM = GameManager.Inst.InventoryViewModel;

            if (_inventoryVM.InventoryItems == null)
            {
                _inventoryVM.Init(SaveManager.Inst.CurrentPlayerModel.Inventory);
            }

            BindViewModel(_inventoryVM);
        }
        else
        {
            _inventoryVM.InvokeOnceOnInit();
        }

        // 상점 뷰모델 확보 (판매용)
        _shopVM = GetShopViewModel();

        Button_Sell.onClick.RemoveListener(OnClickSell);
        Button_Sell.onClick.AddListener(OnClickSell);

        ClearSlotBackground();
        Text_SelectItem.text = string.Empty;
        Text_SelectDescription.text = string.Empty;
        UpdateSellButton();
    }

    private ShopViewModel GetShopViewModel()
    {
        ShopViewModel vm = GameManager.Inst.Services.ShopService.GetShopViewModel();
        if (vm == null)
        {
            vm = GameManager.Inst.Services.ShopService.CreateShopViewModel(GameManager.Inst.InventoryViewModel);
            vm.InitShop();
        }
        return vm;
    }


    public void BindViewModel(InventoryViewModel vm)
    {
        if (_inventoryVM != null)
        {
            _inventoryVM.PropertyChanged -= OnPropertyChanged_View; 
        }

        _inventoryVM = vm;
        _inventoryVM.PropertyChanged += OnPropertyChanged_View;
        _inventoryVM.InvokeOnceOnInit();
    }

    private void OnDestroy()
    {
        if (_inventoryVM != null)
        {
            _inventoryVM.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_inventoryVM.InventoryItems):
                SetInventoryList();
                UpdateSellButton();          
                break;
            case nameof(_inventoryVM.SelectedItemID):
                UpdateSelectedInfo();
                RefreshSlotBackground();
                UpdateSellButton();          
                break;
        }
    }

    private void SetInventoryList()
    {
        foreach (InventorySlot slot in _activeSlots)
        {
            if (slot != null)
            {
                _slotPool.Release(slot);
            }
        }

        _activeSlots.Clear();

        List<ItemModel> items = _inventoryVM.InventoryItems;

        foreach (ItemModel item in items)
        {
            InventorySlot inventorySlot = _slotPool.Get();
            _activeSlots.Add(inventorySlot);

            inventorySlot.SetSlotData(item.ItemID, item.ItemCount, _inventoryVM).Forget();
        }
    }

    private void UpdateSelectedInfo()
    {
        string currentSelectID = _inventoryVM.SelectedItemID;

        if (string .IsNullOrEmpty(currentSelectID))
        {
            return;
        }

        Text_SelectItem.text = GameDataManager.Inst.GetData<SupportItem>(currentSelectID).ItemName;
        Text_SelectDescription.text = GameDataManager.Inst.GetData<SupportItem>(currentSelectID).Description;
    }

    private void RefreshSlotBackground()
    {
        foreach (InventorySlot slot in _activeSlots)
        {
            slot.UpdateState();
        }
    }

    private void ClearSlotBackground()
    {
        foreach (InventorySlot slot in _activeSlots)
        {
            slot.SetBackground(false);
        }
    }

    //판매 버튼 클릭
    private void OnClickSell()
    {
        string itemID = _inventoryVM.SelectedItemID;
        if (string.IsNullOrEmpty(itemID) == true)
        {
            return;
        }

        SellResult result = _shopVM.TrySell(itemID);
        if (result != SellResult.Success)
        {
            Debug.Log($"[InventoryUI] 판매 실패: {result}");
            return;
        }

        // 판매 후 선택 해제 (아이템이 사라졌을 수 있으므로)
        if (_inventoryVM.GetItemCount(itemID) <= 0)
        {
            _inventoryVM.SetSelectItem(string.Empty);
        }

        UpdateSellButton();
    }

    private void UpdateSellButton()
    {
        string itemID = _inventoryVM.SelectedItemID;

        if (string.IsNullOrEmpty(itemID) == true || _shopVM == null)
        {
            Text_SellPrice.text = string.Empty;
            Button_Sell.interactable = false;
            return;
        }

        int sellPrice = _shopVM.GetSellPrice(itemID);
        Text_SellPrice.text = $"판매가 {sellPrice} G";
        Button_Sell.interactable = _shopVM.IsSellable(itemID);
    }
}
