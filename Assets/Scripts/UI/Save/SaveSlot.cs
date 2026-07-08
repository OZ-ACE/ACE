using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : ViewBase
{
    [Header("Button")]
    [SerializeField] private Button Button_Delete;
    [SerializeField] private Button Button_Confirm;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI Text_Index;
    [SerializeField] private TextMeshProUGUI Text_PlayerName;
    [SerializeField] private TextMeshProUGUI Text_Day;
    [SerializeField] private TextMeshProUGUI Text_Gold;

    private int _slotIndex;
    private SaveViewModel _saveVM;
    private PlayerModel _playerModel;

    private void Awake()
    {
        Button_Delete.onClick.AddListener(OnClickDelete);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
    }

    public void BindSlot(SaveViewModel saveVM, int slotIndex)
    {
        _saveVM = saveVM;
        _slotIndex = slotIndex;

        _playerModel = _saveVM.GetPlayerModel(_slotIndex);

        UpdateSlotDisplay();
    }

    private void UpdateSlotDisplay()
    {
        Text_Index.text = $"Slot {_slotIndex + 1}";
        Text_PlayerName.text = _playerModel.PlayerName;
        Text_Day.text = $"Day: {_playerModel.Day}";
        Text_Gold.text = $"Gold: {_playerModel.Gold}";
    }

    private void OnClickDelete()
    {
        _saveVM.RequestDeleteSlot(_slotIndex);
    }

    private void OnClickConfirm()
    {
        _saveVM.RequestConfirmSlot(_slotIndex);
    }
}
