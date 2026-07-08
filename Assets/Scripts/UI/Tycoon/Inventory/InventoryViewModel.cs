using System.Collections.Generic;

public class InventoryViewModel : ViewModelBase
{
    public List<ItemModel> InventoryItems;

    private string _selectedItemID = string.Empty;

    public string SelectedItemID
    {
        get => _selectedItemID;
        set
        {
            if (_selectedItemID != value)
            {
                _selectedItemID = value;

                OnPropertyChanged(nameof(SelectedItemID));
            }
        }
    }

    public void Init(List<ItemModel> inventoryItems)
    {
        InventoryItems = inventoryItems;
    }

    public void InvokeOnceOnInit()
    {
       OnPropertyChanged(nameof(InventoryItems));
    }

    public void UseItem(string itemID)
    {
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i].ItemID == itemID)
            {
                InventoryItems[i].ItemCount--;
            
                if (InventoryItems[i].ItemCount == 0 )
                {
                    InventoryItems.RemoveAt(i);
                }

                break;
            }
        }

        OnPropertyChanged(nameof(InventoryItems));
    }

    public void SetSelectItem(string itemID)
    {
        SelectedItemID = itemID;
    }
}
