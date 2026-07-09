using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class BuildMenuView : ViewBase
{
    [Header("방 버튼 프리팹 (Button-TMP)")]
    [SerializeField] private GameObject Prefab_RoomButton;

    [Header("버튼이 담길 부모 ")]
    [SerializeField] private Transform Transform_ButtonRoot;

    [Header("메뉴 전체 루트 (켜고 끌 대상)")]
    [SerializeField] private GameObject Object_MenuRoot;

    private bool _isButtonsCreated;

    private BuildGridViewModel _viewModel;
    private List<GameObject> _roomButtons = new List<GameObject>();


    public void Bind(BuildGridViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        if (_isButtonsCreated == false)
        {
            CreateRoomButtons();
            _isButtonsCreated = true;
        }

        ApplyBuildMode(_viewModel.IsBuildMode);
    }


    private void OnEnable()
    {
        BuildGridViewModel viewModel = GameManager.Inst.BuildService.GetBuildGridViewModel();
        Bind(viewModel);
    }
    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }




    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BuildGridViewModel.IsBuildMode))
        {
            ApplyBuildMode(_viewModel.IsBuildMode);
        }
    }

    private void ApplyBuildMode(bool isBuildMode)
    {
        if (Object_MenuRoot != null)
        {
            Object_MenuRoot.SetActive(isBuildMode);
        }
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


    private void ClearButtons()
    {
        foreach (GameObject obj in _roomButtons)
        {
            Destroy(obj);
        }
        _roomButtons.Clear();
    }
}