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


    public void AddItem(string itemID, int count)
    {
        if (count <= 0)
        {
            return;
        }

        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i].ItemID == itemID)
            {
                InventoryItems[i].ItemCount += count;
                OnPropertyChanged(nameof(InventoryItems));
                return;
            }
        }

        ItemModel newItem = new ItemModel();
        newItem.ItemID = itemID;
        newItem.ItemCount = count;
        InventoryItems.Add(newItem);

        OnPropertyChanged(nameof(InventoryItems));
    }
    public int GetItemCount(string itemID)
    {
        foreach (ItemModel item in InventoryItems)
        {
            if (item.ItemID == itemID)
            {
                return item.ItemCount;
            }
        }
        return 0;
    }





    public void SetSelectItem(string itemID)
    {
        SelectedItemID = itemID;
    }
}
