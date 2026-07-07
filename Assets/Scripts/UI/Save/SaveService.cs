using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveService : SingletonBase<SaveService>
{
    private SaveViewModel _saveVM;

    public SaveViewModel GetSaveViewModel()
    {
        if (_saveVM == null)
        {
            CreateSaveViewModel();
        }

        return _saveVM;
    }

    private SaveViewModel CreateSaveViewModel()
    {
        _saveVM = new SaveViewModel();

        RefreshActiveSlots();

        return _saveVM;
    }

    public void RefreshActiveSlots()
    {
        if (_saveVM == null)
        {
            return;
        }
        
        List<int> updatedSlot = new List<int>();

        foreach(int i in SaveManager.Inst.SlotIndex)
        {
            updatedSlot.Add(i);
        }

        _saveVM.ActiveSlotIndex = updatedSlot;
    }

    // 나중에 연결
    public void RequestConfirmSlot(int slotIndex)
    {

    }

    public void RequestDeleteSlot(int slotIndex)
    {
        string path = Path.Combine(Application.persistentDataPath, $"Hero{slotIndex}.json");

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        SaveManager.Inst.SlotIndex.Remove(slotIndex);
        SaveManager.Inst.OnClearSave?.Invoke();

        RefreshActiveSlots();
    }
}