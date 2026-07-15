using System.Collections.Generic;
using System;
using UnityEngine;

public class SaveViewModel : ViewModelBase
{
    private List<int> _activeSlotIndex = new List<int>();

    public int SelectedSlotIndex { get; private set; }

    public List<int> ActiveSlotIndex
    {
        get => _activeSlotIndex;

        set
        {
            if (_activeSlotIndex != value)
            {
                _activeSlotIndex = value;
                OnPropertyChanged(nameof(ActiveSlotIndex));
            }
        }
    }

    public void PrepareNewSave(int slotIndex)
    {
        SelectedSlotIndex = slotIndex;
        SaveManager.Inst.SetCurrentSlotIndex(slotIndex);
    }

    public void CreateAndSavePlayer(string playerName)
    {
        PlayerModel newPlayer = SaveManager.Inst.GetDefaultData();
        newPlayer.PlayerName = playerName;

        SaveManager.Inst.RequestSaveData(newPlayer);

        RefreshActiveSlots();
    }

    public void InvokeOnceOnInit()
    {
        RefreshActiveSlots();
    }

    public PlayerModel GetPlayerModel(int slotIndex)
    {
        return SaveManager.Inst.RequestLoadData(slotIndex);
    }

    public void RequestDeleteSlot(int slotIndex)
    {
        SaveManager.Inst.RequestDeleteData(slotIndex);

        RefreshActiveSlots();
    }

    public void RefreshActiveSlots()
    {
        List<int> updatedSlot = new List<int>();

        foreach (int i in SaveManager.Inst.SlotIndex)
        {
            updatedSlot.Add(i);
        }

        ActiveSlotIndex = updatedSlot;
    }

    public void RequestConfirmSlot(int slotIndex)
    {
        SaveManager.Inst.SetCurrentSlotIndex(slotIndex);
        SaveManager.Inst.RequestLoadData(slotIndex);
        GameManager.Inst.Services.BuildService.GetBuildGridViewModel().ReloadGrid();
    }
}
