using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : SingletonBase<SaveManager>
{
    public PlayerModel CurrentPlayerModel { get; private set; } = new PlayerModel();
    public SaveViewModel SaveVM {  get; private set; } = new SaveViewModel();
    public int CurrentSlotIndex { get; private set; } = 0;
    public SortedSet<int> SlotIndex { get; private set; } = new SortedSet<int>();

    protected override void Awake()
    {
        base.Awake();

        SlotIndex.Clear();

        for (int i = 0; i < 100; i++)
        {
            if (HasSaveFile(i))
            {
                SlotIndex.Add(i);
            }
        }
    }

    private string GetPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"Hero{slotIndex}.json");
    }

    public void RequestSaveData(PlayerModel data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(CurrentSlotIndex), json);
        SlotIndex.Add(CurrentSlotIndex);
    }

    public PlayerModel RequestLoadData(int slotIndex)
    {
        string path = GetPath(slotIndex);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerModel data = JsonUtility.FromJson<PlayerModel>(json);
            CurrentPlayerModel = data;

            return data;
        }
        else
        {
            PlayerModel data = GetDefaultData();
            RequestSaveData(data);

            return data;
        }
    }

    public bool RequestDeleteData(int slotIndex)
    {
        string path = GetPath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            SlotIndex.Remove(slotIndex);

            return true;
        }

        return false;
    }

    public PlayerModel GetDefaultData()
    {
        PlayerModel newPlayer = new PlayerModel();

        newPlayer.PlayerName = "요양보조사";
        newPlayer.Day = 1;
        newPlayer.Gold = 1000;
        newPlayer.MemoryFragment = 0;

        newPlayer.Inventory = SetDefaultItem();

        CurrentPlayerModel = newPlayer;
        return newPlayer;
    }

    private List<ItemModel> SetDefaultItem()
    {
        List<ItemModel> items = new List<ItemModel>();

        foreach (SupportItem item in GameDataManager.Inst.GetDataList<SupportItem>())
        {
            ItemModel itemModel = new ItemModel
            {
                ItemID = item.ID,
                ItemCount = item.stockCount
            };

            items.Add(itemModel);
        }

        return items;
    }

    public void SetCurrentSlotIndex(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
    }

    public void SetPlayerName(string name)
    {
        CurrentPlayerModel.PlayerName = name;
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetPath(slotIndex));
    }

    public int GetEmptySlot()
    {
        if (SlotIndex == null || SlotIndex.Count == 0)
        {
            return 0;
        }

        int nextIndex = 0;
        
        foreach (int index in SlotIndex)
        {
            if (index == nextIndex)
            {
                nextIndex++;
            }
            else if (index > nextIndex)
            {
                break;
            }
        }

        return nextIndex;
    }
}
