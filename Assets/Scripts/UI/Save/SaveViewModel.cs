using System.Collections.Generic;

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

    // 나중에 연결
    public void RequestConfirmSlot(int slotIndex)
    {

    }
}
