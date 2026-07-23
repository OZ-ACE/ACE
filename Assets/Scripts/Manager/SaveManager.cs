using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : SingletonBase<SaveManager>
{
    public PlayerModel CurrentPlayerModel { get; private set; } = new PlayerModel();
    public SaveViewModel SaveVM {  get; private set; } = new SaveViewModel();
    public int CurrentSlotIndex { get; private set; } = 0;
    public SortedSet<int> SlotIndex { get; private set; } = new SortedSet<int>();

    public bool IsInitialized { get; private set; }
    public bool IsPlayerDataLoaded { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (IsInitialized == true)
        {
            return;
        }

        SlotIndex.Clear();

        for (int i = 0; i < 100; i++)
        {
            if (HasSaveFile(i) == true)
            {
                SlotIndex.Add(i);
            }
        }

        IsInitialized = true;
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

            MigrateNewData(data);

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

    public PlayerModel LoadPreviewData(int slotIndex)
    {
        string path = GetPath(slotIndex);

        if (File.Exists(path) == false)
        {
            return null;
        }

        string json = File.ReadAllText(path);

        return JsonUtility.FromJson<PlayerModel>(json);
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
        newPlayer.Gold = 990;
        newPlayer.MemoryFragment = 0;
        newPlayer.LowGrade = 0;
        newPlayer.EndingType = EndingType.None;

        newPlayer.Inventory = SetDefaultItem();
        newPlayer.HeroStats = new List<HeroStat>();

        newPlayer.BuildGridData = new BuildGridData();
        newPlayer.QuestProgressList = SetDefaultQuest();

        CurrentPlayerModel = newPlayer;
        return newPlayer;
    }

    private List<QuestProgressModel> SetDefaultQuest()
    {
        List<QuestProgressModel> questList = new List<QuestProgressModel>();
        List<QuestData> quests = GameDataManager.Inst.GetDataList<QuestData>();

        if (quests != null)
        {
            foreach (QuestData quest in quests)
            {
                int initialState = string.IsNullOrEmpty(quest.RequiredQuestID) ? 1 : 0;

                QuestProgressModel questProgressModel = new QuestProgressModel
                {
                    QuestID = quest.ID,
                    CurrentCount = 0,
                    State = initialState
                };

                questList.Add(questProgressModel);
            }
        }

        return questList;
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

    private void MigrateNewData(PlayerModel data)
    {
        if (data == null)
        {
            return;
        }

        bool isOldData = false;

        List<SupportItem> items = GameDataManager.Inst.GetDataList<SupportItem>();
        if (items != null)
        {
            if (data.Inventory == null)
            {
                data.Inventory = new List<ItemModel>();
            }

            foreach (SupportItem item in items)
            {
                bool isExist = false;

                foreach (ItemModel itemModel in data.Inventory)
                {
                    if (itemModel.ItemID == item.ID)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (isExist == false)
                {
                    ItemModel newItem = new ItemModel
                    {
                        ItemID = item.ID,
                        ItemCount = item.StockCount
                    };
                    data.Inventory.Add(newItem);
                    isOldData = true;
                }
            }
        }

        List<QuestData> quests = GameDataManager.Inst.GetDataList<QuestData>();
        if (quests != null)
        {
            if (data.QuestProgressList == null)
            {
                data.QuestProgressList = new List<QuestProgressModel>();
            }

            foreach (QuestData quest in quests)
            {
                bool isExist = false;

                foreach (QuestProgressModel questProgress in data.QuestProgressList)
                {
                    if (questProgress.QuestID == quest.ID)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (isExist == false)
                {
                    QuestProgressModel newProgress = new QuestProgressModel
                    {
                        QuestID = quest.ID,
                        CurrentCount = 0,
                        State = 1
                    };
                    data.QuestProgressList.Add(newProgress);
                    isOldData = true;
                }
            }
        }

        if (isOldData == true)
        {
            RequestSaveData(data);
        }
    }

    public void SetCurrentSlotIndex(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;
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

    public void LoadSlot(int slotIndex)
    {
        if (IsInitialized == false)
        {
            Debug.LogError("SaveManager 초기화 전에 슬롯을 불러올 수 없음.");
            return;
        }

        CurrentSlotIndex = slotIndex;
        CurrentPlayerModel = RequestLoadData(CurrentSlotIndex);
        IsPlayerDataLoaded = true;
    }
}
