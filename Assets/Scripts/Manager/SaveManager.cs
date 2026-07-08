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
        newPlayer.Day = 1000;

        return newPlayer;
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetPath(slotIndex));
    }
}
