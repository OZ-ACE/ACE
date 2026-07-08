using System.Collections.Generic;
using UnityEngine;
using System;



//Model 역할 + ViewModel 역할
public class BuildGridViewModel : ViewModelBase
{
    private readonly GridSystem _gridSystem;
    private readonly BuildGridModel _buildGridModel;
    public event Action<PlacedRoomData> OnPlaceRoom;
    public event Action<PlacedRoomData> OnRemoveRoom;
    public GridBounds Bounds { get { return _buildGridModel.Bounds; } }

    private List<string> _buildableRoomIds = new List<string>();
    public List<string> BuildableRoomIds { get { return _buildableRoomIds; } }
    public void SetBuildableRooms(List<string> roomIds)
    {
        _buildableRoomIds = roomIds;
    }

    //뷰가 바인딩
    private bool _isBuildMode;
    private bool _isDemolishMode;
    private string _selectedRoomId;

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
    public bool IsDemolishMode
    {
        get => _isDemolishMode;
        set
        {
            if (_isDemolishMode != value)
            {
                _isDemolishMode = value;
                OnPropertyChanged(nameof(IsDemolishMode));
            }
        }
    }
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
        _buildGridModel.InitCellTypes(bounds);
    }


    //뷰가 바인딩 직후 1회 호출
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(IsBuildMode));
        OnPropertyChanged(nameof(SelectedRoomId));
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



    //건설모드 토글
    public void ToggleBuildMode()
    {
        if (IsBuildMode == true)
        {
            ExitBuildMode();
        }
        else
        {
            EnterBuildMode();
        }
    }

    //철거모드 토글
    public void ToggleDemolishMode()
    {
        if (IsDemolishMode == true)
        {
            IsDemolishMode = false;
        }
        else
        {
            IsDemolishMode = true;
            SelectedRoomId = null; 
        }
    }






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
        IsDemolishMode = false;
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

        SaveGrid();
        Debug.Log($"세이브 경로: {Application.persistentDataPath}");

        Debug.Log($"[BuildGridViewModel] 방 배치 성공: {roomId} @ {originCoord}");
        return PlacementResult.Success;
    }



    //철거 실행 명령
    public bool TryDemolishRoom(GridCoord coord)
    {
        PlacedRoomData removed = _buildGridModel.RemoveRoomAt(coord, _gridSystem);
        if (removed == null)
        {
            return false;
        }

        if (OnRemoveRoom != null)
        {
            OnRemoveRoom.Invoke(removed);
        }

        // TODO: 환불 (경제 시스템 연동 후) — RoomData.BuildCost 일부 환불

        SaveGrid();  
        Debug.Log($"[BuildGridViewModel] 방 철거: {removed.RoomId} @ {removed.Origin}");
        return true;
    }











    //그리드 상태 저장
    public void SaveGrid()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            Debug.LogWarning("[BuildGridViewModel] CurrentPlayerModel 없음 - 저장 스킵");
            return;
        }

        player.BuildGridData = _buildGridModel.GetSaveData();
        SaveManager.Inst.RequestSaveData(SaveManager.Inst.CurrentSlotIndex, player);
    }

    //그리드 불러오기
    public void LoadGrid()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null || player.BuildGridData == null)
        {
            Debug.Log("[BuildGridViewModel] 저장된 그리드 없음");
            return;
        }

        _buildGridModel.LoadFromSaveData(player.BuildGridData, _gridSystem);

        foreach (PlacedRoomData room in player.BuildGridData.PlacedRooms)
        {
            if(OnPlaceRoom !=null)
            {
                OnPlaceRoom.Invoke(room);
            }
        }

        Debug.Log($"[BuildGridViewModel] 그리드 복원: 방 {player.BuildGridData.PlacedRooms.Count}개");

    }

}
