using System.Collections.Generic;
using UnityEngine;
using System;



//Model 역할 + ViewModel 역할
public class BuildGridViewModel : ViewModelBase
{
    private readonly GridSystem _gridSystem;
    private readonly BuildGridModel _buildGridModel;
    public event Action<PlacedRoomData> OnPlaceRoom;

    public GridSystem GridSystem { get { return _gridSystem; } }
    public BuildGridModel BuildGridModel { get { return _buildGridModel; } }

    public BuildGridViewModel(GridSystem gridSystem, BuildGridModel buildGridModel)
    {
        _gridSystem = gridSystem;
        _buildGridModel = buildGridModel;
    }

    public void InitGrid(GridBounds bounds)
    {
        _buildGridModel.SetBounds(bounds);
    }


    //뷰가 바인딩 직후 1회 호출
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(IsBuildMode));
        OnPropertyChanged(nameof(SelectedRoomId));
    }

    //뷰가 바인딩
    private bool _isBuildMode;
    public bool IsBuildMode
    {
        get => _isBuildMode;
        set
        {
            if (_isBuildMode != value)
            {
                _isBuildMode = value;
                OnPropertyChanged(nameof(IsBuildMode));
            }
        }
    }

    private string _selectedRoomId;
    public string SelectedRoomId
    {
        get => _selectedRoomId;
        set
        {
            if (_selectedRoomId != value)
            {
                _selectedRoomId = value;
                OnPropertyChanged(nameof(SelectedRoomId));
            }
        }
    }



    //방 크기·타입 조회 헬퍼
    public RoomData GetSelectedRoomData()
    {
        if (string.IsNullOrEmpty(_selectedRoomId) == true)
        {
            return null;
        }
        return GameDataManager.Inst.GetData<RoomData>(SelectedRoomId);
    }
    public PlacementResult CheckSelectedRoomPlaceable(GridCoord originCoord)
    {
        RoomData room = GetSelectedRoomData();
        if (room == null)
        {
            return PlacementResult.WrongCellType;
        }
        return _buildGridModel.CheckPlaceable(originCoord, room.GetSize(), room.GetRequiredCellType(), _gridSystem);
    }





    //------
    //뷰가 호출하는 명령들
    //------

    //건설모드 진입
    public void EnterBuildMode()
    {
        IsBuildMode = true;
    }

    //건설모드 종료
    public void ExitBuildMode()
    {
        IsBuildMode = false;
        SelectedRoomId = null;
    }

    //건설메뉴에서 방 선택
    public void SelectRoom(string roomId)
    {
        SelectedRoomId = roomId;
    }

    //방 배치 시도 및 결과 반환
    public PlacementResult TryPlaceRoom(string roomId, GridCoord originCoord)
    {
        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(roomId);
        if (roomData == null)
        {
            Debug.LogWarning($"[BuildGridViewModel] 유효하지 않은 방 데이터: {roomId}");
            return PlacementResult.WrongCellType;
        }

        Vector2Int size = roomData.GetSize();
        CellType requiredType = roomData.GetRequiredCellType();

        PlacementResult result = _buildGridModel.CheckPlaceable(originCoord, size, requiredType, _gridSystem);
        if (result != PlacementResult.Success)
        {
            return result;
        }

        PlacedRoomData placed = new PlacedRoomData();
        placed.RoomId = roomId;
        placed.Origin = originCoord;

        List<GridCoord> coords = _gridSystem.GetOccupiedCoords(originCoord, size);
        _buildGridModel.AddRoom(placed, coords);

        if (OnPlaceRoom != null)
        {
            OnPlaceRoom.Invoke(placed);
        }

        Debug.Log($"[BuildGridViewModel] 방 배치 성공: {roomId} @ {originCoord}");
        return PlacementResult.Success;
    }






}
