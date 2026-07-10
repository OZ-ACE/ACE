using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_SelectItem;
    [SerializeField] private TextMeshProUGUI Text_SelectDescription;

    [SerializeField] private Transform Transform_SlotParent;
    [SerializeField] private GameObject InventorySlot;

    private InventoryViewModel _inventoryVM;
    private List<InventorySlot> _activeSlots = new List<InventorySlot>();

    private void Start()
    {
        _activeSlots = new List<InventorySlot>();

        List<ItemModel> inventory = SaveManager.Inst.CurrentPlayerModel.Inventory;

        _inventoryVM = new InventoryViewModel();
        _inventoryVM.Init(inventory);
        BindViewModel(_inventoryVM);
    }

    private void OnEnable()
    {
        ClearSlotBackground();

        Text_SelectItem.text = string.Empty;
        Text_SelectDescription.text = string.Empty;
    }

    public void BindViewModel(InventoryViewModel vm)
    {
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
                SetInventoryList().Forget();
                break;

            case nameof(_inventoryVM.SelectedItemID):
                UpdateSelectedInfo();
                RefreshSlotBackground();
                break;
        }
    }

    private async UniTask SetInventoryList()
    {
        foreach (InventorySlot slot in _activeSlots)
        {
            Destroy(slot.gameObject);
        }

        _activeSlots.Clear();

        List<ItemModel> items = _inventoryVM.InventoryItems;

        foreach (ItemModel item in items)
        {
            GameObject prefab = await ResourceManager.Inst.InstantiateAsync("Prefabs/UI/InventorySlot", Transform_SlotParent);
            InventorySlot inventorySlot = prefab.GetComponent<InventorySlot>();
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
            slot.LoadBackground(false).Forget();
        }
    }
}
