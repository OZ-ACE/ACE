using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : SingletonBase<SaveManager>
{
    public PlayerModel CurrentPlayerModel { get; private set; } = new PlayerModel();
    public int CurrentSlotIndex { get; private set; }
    public HashSet<int> SlotIndex { get; private set; } = new HashSet<int>();

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

    //[테스트 코드]
    private void OnEnable()
    {
        CurrentPlayerModel = RequestLoadData(0);
    }

    private string GetPath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"Hero{slotIndex}.json");
    }

    public void RequestSaveData(int slotIndex, PlayerModel data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slotIndex), json);
        SlotIndex.Add(slotIndex);
    }

    public PlayerModel RequestLoadData(int slotIndex)
    {
        string path = GetPath(slotIndex);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerModel data = JsonUtility.FromJson<PlayerModel>(json);

            return data;
        }
        else
        {
            PlayerModel data = GetDefaultData();
            RequestSaveData(slotIndex, data);

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

        newPlayer.Inventory = SetDefaultItem();

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
                ItemCount = item.StockCount
            };

            items.Add(itemModel);
        }

        return items;
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetPath(slotIndex));
    }
}
