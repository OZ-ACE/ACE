using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveViewModel : ViewModelBase
{
    private List<int> _activeSlotIndex = new List<int>();

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

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ActiveSlotIndex));
    }

    public PlayerModel GetPlayerModel(int slotIndex)
    {
        return SaveManager.Inst.RequestLoadData(slotIndex);
    }
}
