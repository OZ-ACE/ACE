using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class BuildMenuView : ViewBase
{
    [Header("방 버튼 프리팹")]
    [SerializeField] private GameObject Prefab_RoomButton;

    [Header("버튼이 담길 부모")]
    [SerializeField] private Transform Transform_ButtonRoot;

    private bool _isButtonsCreated;
    private BuildGridViewModel _viewModel;
    private List<GameObject> _roomButtons = new List<GameObject>();

    public void Bind(BuildGridViewModel viewModel)
    {
        _viewModel = viewModel;

        if (_isButtonsCreated == false)
        {
            CreateRoomButtons();
            _isButtonsCreated = true;
        }
    }

    private void OnEnable()
    {
        Bind(GameManager.Inst.Services.BuildService.GetBuildGridViewModel());
    }

    private void CreateRoomButtons()
    {
        List<string> roomIds = _viewModel.BuildableRoomIds;

        for (int i = 0; i < roomIds.Count; i++)
        {
            RoomData room = GameDataManager.Inst.GetData<RoomData>(roomIds[i]);
            if (room == null)
            {
                continue;
            }

            GameObject slotObj = Instantiate(Prefab_RoomButton, Transform_ButtonRoot);
            slotObj.name = $"ConstructSlot_{room.ID}";
            _roomButtons.Add(slotObj);

            ConstructSlot slot = slotObj.GetComponent<ConstructSlot>();
            if (slot != null)
            {
                slot.SetSlotData(room, _viewModel);
            }
        }

        Debug.Log($"[BuildMenuView] 방 버튼 {roomIds.Count}개 생성");
    }
}