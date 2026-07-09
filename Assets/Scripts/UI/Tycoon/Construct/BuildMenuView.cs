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

        CreateRoomButtons();
        ApplyBuildMode(_viewModel.IsBuildMode); 
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
        ClearButtons();

        List<string> roomIds = _viewModel.BuildableRoomIds;
        foreach (string roomId in roomIds)
        {
            RoomData room = GameDataManager.Inst.GetData<RoomData>(roomId);
            if (room == null)
            {
                Debug.LogWarning($"[BuildMenuView] 방 데이터 없음: {roomId}");
                continue;
            }

            GameObject buttonObj = Instantiate(Prefab_RoomButton, Transform_ButtonRoot);
            buttonObj.name = $"Button_{roomId}";

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = room.Name;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                RoomButtonHandler handler = buttonObj.AddComponent<RoomButtonHandler>();
                handler.Setup(_viewModel, roomId);
                button.onClick.AddListener(handler.OnClickSelect);
            }

            _roomButtons.Add(buttonObj);
        }

        Debug.Log($"[BuildMenuView] 방 버튼 {_roomButtons.Count}개 생성");
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