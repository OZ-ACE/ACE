using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;


public class ShopView : ViewBase
{
    [Header("슬롯 프리팹 / 부모")]
    [SerializeField] private GameObject Prefab_ShopSlot;
    [SerializeField] private Transform Transform_SlotParent;

    [Header("Gold 표시")]
    [SerializeField] private TextMeshProUGUI Text_Gold;

    private ShopViewModel _viewModel;
    private List<ShopSlot> _activeSlots = new List<ShopSlot>();

    private bool _isCreatingSlots;

    public void Bind(ShopViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OnPurchaseItem -= OnPurchaseItem;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.OnPurchaseItem += OnPurchaseItem;

        _viewModel.InvokeOnceOnInit();
    }

    private void OnEnable()
    {
        if (_viewModel == null)
        {
            ShopViewModel vm = GameManager.Inst.Services.ShopService.GetShopViewModel();
            if (vm == null)
            {
                InventoryViewModel inventoryVM = GetInventoryViewModel();
                vm = GameManager.Inst.Services.ShopService.CreateShopViewModel(inventoryVM);
                vm.InitShop();
            }
            Bind(vm);
        }
        else
        {
            _viewModel.InvokeOnceOnInit();
        }
    }

    /// <summary> 공유 인벤토리 뷰모델 확보 (없으면 초기화) </summary>
    private InventoryViewModel GetInventoryViewModel()
    {
        InventoryViewModel inventoryVM = GameManager.Inst.InventoryViewModel;
        if (inventoryVM.InventoryItems == null)
        {
            inventoryVM.Init(SaveManager.Inst.CurrentPlayerModel.Inventory);
        }
        return inventoryVM;
    }




    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OnPurchaseItem -= OnPurchaseItem;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShopViewModel.ShopItems))
        {
            CreateSlots().Forget();
        }
        else if (e.PropertyName == nameof(ShopViewModel.CurrentGold))
        {
            UpdateGoldText();
        }
    }

    private async UniTask CreateSlots()
    {
        if (_isCreatingSlots == true)
        {
            return;   // 이미 생성 중이면 무시
        }
        _isCreatingSlots = true;

        ClearSlots();

        List<SupportItem> items = _viewModel.ShopItems;
        foreach (SupportItem item in items)
        {
            GameObject slotObj = Instantiate(Prefab_ShopSlot, Transform_SlotParent);
            slotObj.name = $"Slot_{item.ID}";

            ShopSlot slot = slotObj.GetComponent<ShopSlot>();
            _activeSlots.Add(slot);

            await slot.SetSlotData(item, _viewModel);
        }

        UpdateGoldText();
        _isCreatingSlots = false;
        Debug.Log($"[ShopView] 상점 슬롯 {_activeSlots.Count}개 생성");
    }

    private void ClearSlots()
    {
        foreach (ShopSlot slot in _activeSlots)
        {
            if (slot != null)   // ★ 파괴된 오브젝트 건너뛰기
            {
                Destroy(slot.gameObject);
            }
        }
        _activeSlots.Clear();
    }


    /// <summary> 구매 발생 시 모든 슬롯 갱신 (재고·버튼 상태) </summary>
    private void OnPurchaseItem(string itemID)
    {
        RefreshAllSlots();
    }

    /// <summary> 모든 슬롯의 재고·버튼 상태 갱신 </summary>
    private void RefreshAllSlots()
    {
        foreach (ShopSlot slot in _activeSlots)
        {
            slot.UpdateState();
        }
    }

    private void UpdateGoldText()
    {
        if (Text_Gold == null || _viewModel == null)
        {
            return;
        }
        Text_Gold.text = $"{_viewModel.CurrentGold} G";
    }
}