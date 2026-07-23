using System.Collections.Generic;
using UnityEngine;
public class BuildMenuView : ViewBase
{
    [Header("방 버튼 프리팹")]
    [SerializeField] private GameObject Prefab_RoomButton;
    [Header("버튼이 담길 부모")]
    [SerializeField] private Transform Transform_ButtonRoot;
    private BuildGridViewModel _viewModel;
    private List<GameObject> _roomButtons = new List<GameObject>();
    public void Bind(BuildGridViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnPlaceRoom -= OnRoomChanged;
            _viewModel.OnRemoveRoom -= OnRoomChanged;
        }
        _viewModel = viewModel;
        _viewModel.OnPlaceRoom += OnRoomChanged;
        _viewModel.OnRemoveRoom += OnRoomChanged;
        CreateRoomButtons();
    }
    private void OnEnable()
    {
        Bind(GameManager.Inst.Services.BuildService.GetBuildGridViewModel());
    }
    private void OnDisable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnPlaceRoom -= OnRoomChanged;
            _viewModel.OnRemoveRoom -= OnRoomChanged;
        }
    }
    // 방이 지어지거나 철거되면 해금 상태가 바뀔 수 있으니 목록 갱신
    private void OnRoomChanged(PlacedRoomData room)
    {
        CreateRoomButtons();
    }
    private void CreateRoomButtons()
    {
        // 기존 버튼 정리
        for (int i = 0; i < _roomButtons.Count; i++)
        {
            if (_roomButtons[i] != null)
            {
                Destroy(_roomButtons[i]);
            }
        }
        _roomButtons.Clear();
        // 해금된 방만 버튼 생성
        List<string> roomIds = _viewModel.GetUnlockedRoomIds();
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
        Debug.Log($"[BuildMenuView] 해금된 방 버튼 {roomIds.Count}개 생성");
    }
}