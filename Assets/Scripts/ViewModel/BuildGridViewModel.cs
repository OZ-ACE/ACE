using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;

//Model 역할 + ViewModel 역할
public class BuildGridViewModel : ViewModelBase
{
    private readonly GridSystem _gridSystem;
    private readonly BuildGridModel _buildGridModel;
    private readonly ICurrencyService _currencyService;

    public event Action<PlacedRoomData> OnPlaceRoom;
    public event Action<PlacedRoomData> OnRemoveRoom;
    public event Action<int> OnUnlockFloor;
    public event Action OnReloadGrid;
    public event Action OnClickOffice;
    public event Action OnClickBattle;

    public GridBounds Bounds { get { return _buildGridModel.Bounds; } }

    private List<string> _buildableRoomIds = new List<string>();
    public List<string> BuildableRoomIds { get { return _buildableRoomIds; } }
    public GridSystem GridSystem { get { return _gridSystem; } }
    public BuildGridModel BuildGridModel { get { return _buildGridModel; } }

    private PlacedRoomData _pickedRoom;
    public PlacedRoomData PickedRoom { get { return _pickedRoom; } }
    public bool IsHoldingRoom { get { return _pickedRoom != null; } }
    public int UnlockedMinFloor { get { return _buildGridModel.UnlockedMinFloor; } }

    // 철거 환불 비율 (건설비의 50%)
    private const float REFUND_RATIO = 0.5f;

    // 층 해금 비용 (깊이 1당)
    private const int UNLOCK_COST_PER_FLOOR = 500;

    // 계단 설정
    private const string STAIR_ROOM_ID = "Room_Stairs";
    private const int STAIR_MIN_COLUMN = 10;   // 계단이 차지하는 시작 열

    // 사무실 방 ID
    private const string OFFICE_ROOM_ID = "Room_Office";
    private const string BATTLE_ROOM_ID = "Room_Battle";

    //뷰가 바인딩
    private bool _isBuildMode;
    private bool _isDemolishMode;
    private bool _isMoveMode;
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

    public bool IsMoveMode
    {
        get => _isMoveMode;
        set
        {
            if (_isMoveMode != value)
            {
                _isMoveMode = value;
                OnPropertyChanged(nameof(IsMoveMode));
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

    public BuildGridViewModel(GridSystem gridSystem, BuildGridModel buildGridModel, ICurrencyService currencyService)
    {
        _gridSystem = gridSystem;
        _buildGridModel = buildGridModel;
        _currencyService = currencyService;
    }

    public void InitGrid(GridBounds bounds, int initialMinFloor)
    {
        _buildGridModel.SetBounds(bounds);
        _buildGridModel.InitCellTypes(bounds);
        _buildGridModel.InitUnlock(initialMinFloor);   
    }

    //뷰가 바인딩 직후 1회 호출
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(IsBuildMode));
        OnPropertyChanged(nameof(SelectedRoomId));
    }

    public void SetBuildableRooms(List<string> roomIds)
    {
        _buildableRoomIds = roomIds;
    }

    // 현재 배치된 방 전체 (뷰가 초기 렌더링할 때 사용)
    public List<PlacedRoomData> GetPlacedRooms()
    {
        return _buildGridModel.GetAllRooms();
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

        if (IsFloorAllowed(room, originCoord) == false)
        {
            return PlacementResult.WrongCellType;
        }

        PlacementResult result = _buildGridModel.CheckPlaceable(originCoord, room.GetSize(), room.GetRequiredCellType(), _gridSystem, room.IsAnyCellType());

        if (result != PlacementResult.Success)
        {
            return result;
        }

        // 돈 부족도 배치 불가로 (고스트 빨강)
        if (_currencyService.IsAffordable(room.BuildCost) == false)
        {
            return PlacementResult.NotEnoughGold;
        }

        return PlacementResult.Success;
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
        RestorePickedRoom();
        IsMoveMode = false;

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

    //이동모드 토글
    public void ToggleMoveMode()
    {
        IsDemolishMode = false;

        if (IsMoveMode == true)
        {
            IsMoveMode = false;
            RestorePickedRoom();
        }
        else
        {
            IsMoveMode = true;
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
        RestorePickedRoom();
        IsBuildMode = false;
        SelectedRoomId = null;
        IsDemolishMode = false;
        IsMoveMode = false;   
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

        if (IsFloorAllowed(roomData, originCoord) == false)
        {
            return PlacementResult.WrongCellType;
        }

        PlacementResult result = _buildGridModel.CheckPlaceable(originCoord, size, requiredType, _gridSystem, roomData.IsAnyCellType());

        if (result != PlacementResult.Success)
        {
            return result;
        }

        if (_currencyService.IsAffordable(roomData.BuildCost) == false)
        {
            return PlacementResult.NotEnoughGold;
        }

        if (_currencyService.TrySpend(roomData.BuildCost) == false)
        {
            return PlacementResult.NotEnoughGold;
        }

        PlacedRoomData placed = new PlacedRoomData();
        placed.RoomInstanceId = GameUtil.GenerateUniqueId();
        placed.RoomId = roomId;
        placed.Origin = originCoord;

        List<GridCoord> coords = _gridSystem.GetOccupiedCoords(originCoord, size);
        _buildGridModel.AddRoom(placed, coords);

        if (OnPlaceRoom != null)
        {
            OnPlaceRoom.Invoke(placed);
        }


        GameManager.Inst.Services.QuestService.ReportProgress(QuestConditionType.BuildRoom, roomId, 1);
        SaveGrid();
        Debug.Log($"[BuildGridViewModel] 방 배치 성공: {roomId} @ {originCoord} (-{roomData.BuildCost}G)");
        return PlacementResult.Success;
    }

    //철거 실행 명령
    public bool TryDemolishRoom(GridCoord coord)
    {
        PlacedRoomData target = _buildGridModel.GetRoomAt(coord);

        if (target != null && IsStair(target.RoomId) == true)
        {
            Debug.Log("[BuildGridViewModel] 계단 철거 불가");
            return false;
        }


        PlacedRoomData removed = _buildGridModel.RemoveRoomAt(coord, _gridSystem);

        if (removed == null)
        {
            return false;
        }

        int refund = GetRefundAmount(removed.RoomId);

        if (refund > 0)
        {
            _currencyService.AddGold(refund);
        }

        if (OnRemoveRoom != null)
        {
            OnRemoveRoom.Invoke(removed);
        }

        SaveGrid();
        Debug.Log($"[BuildGridViewModel] 방 철거: {removed.RoomId} @ {removed.Origin} (+{refund}G 환불)");
        return true;
    }

    //해당 방의 철거 환불액 (건설비의 일정 비율)
    public int GetRefundAmount(string roomId)
    {
        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(roomId);

        if (roomData == null)
        {
            return 0;
        }

        return Mathf.FloorToInt(roomData.BuildCost * REFUND_RATIO);
    }

    //방 집기
    public bool TryPickRoom(GridCoord coord)
    {
        if (_pickedRoom != null)
        {
            return false;   
        }

        PlacedRoomData target = _buildGridModel.GetRoomAt(coord);

        if (target != null && IsStair(target.RoomId) == true)
        {
            Debug.Log("[BuildGridViewModel] 계단은 이동할 수 없음");
            return false;
        }

        PlacedRoomData room = _buildGridModel.RemoveRoomAt(coord, _gridSystem);

        if (room == null)
        {
            return false;   
        }

        _pickedRoom = room;

        // 뷰가 원래 방 오브젝트를 치우도록 철거 이벤트 재활용
        if (OnRemoveRoom != null)
        {
            OnRemoveRoom.Invoke(room);
        }

        Debug.Log($"[BuildGridViewModel] 방 집음: {room.RoomId} @ {room.Origin}");
        return true;
    }

    //방 놓기
    public bool TryDropRoom(GridCoord newOrigin)
    {
        if (_pickedRoom == null)
        {
            return false;
        }

        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(_pickedRoom.RoomId);

        if (roomData == null)
        {
            return false;
        }

        Vector2Int size = roomData.GetSize();
        CellType requiredType = roomData.GetRequiredCellType();

        if (IsFloorAllowed(roomData, newOrigin) == false)
        {
            return false;
        }

        PlacementResult result = _buildGridModel.CheckPlaceable(newOrigin, size, requiredType, _gridSystem, roomData.IsAnyCellType());

        if (result != PlacementResult.Success)
        {
            return false;   
        }

        PlacedRoomData moved = new PlacedRoomData();
        moved.RoomInstanceId = _pickedRoom.RoomInstanceId;
        moved.RoomId = _pickedRoom.RoomId;
        moved.Origin = newOrigin;

        List<GridCoord> coords = _gridSystem.GetOccupiedCoords(newOrigin, size);
        _buildGridModel.AddRoom(moved, coords);

        if (OnPlaceRoom != null)
        {
            OnPlaceRoom.Invoke(moved);
        }

        _pickedRoom = null;  
        SaveGrid();

        Debug.Log($"[BuildGridViewModel] 방 이동 완료: {moved.RoomId} → {newOrigin}");
        return true;
    }

    //집은 방의 판정
    public PlacementResult CheckPickedRoomPlaceable(GridCoord newOrigin)
    {
        if (_pickedRoom == null)
        {
            return PlacementResult.WrongCellType;
        }

        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(_pickedRoom.RoomId);

        if (roomData == null)
        {
            return PlacementResult.WrongCellType;
        }

        if (IsFloorAllowed(roomData, newOrigin) == false)
        {
            return PlacementResult.WrongCellType;
        }

        return _buildGridModel.CheckPlaceable(newOrigin, roomData.GetSize(), roomData.GetRequiredCellType(), _gridSystem, roomData.IsAnyCellType());
    }

    //집고 있던 방 환불
    private void RestorePickedRoom()
    {
        if (_pickedRoom == null)
        {
            return;
        }

        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(_pickedRoom.RoomId);

        if (roomData == null)
        {
            _pickedRoom = null;
            return;
        }

        List<GridCoord> coords = _gridSystem.GetOccupiedCoords(_pickedRoom.Origin, roomData.GetSize());
        _buildGridModel.AddRoom(_pickedRoom, coords);

        if (OnPlaceRoom != null)
        {
            OnPlaceRoom.Invoke(_pickedRoom);
        }

        Debug.Log($"[BuildGridViewModel] 집은 방 원위치: {_pickedRoom.RoomId} @ {_pickedRoom.Origin}");
        _pickedRoom = null;

        SaveGrid();   
    }

    //방의 층 제한 판정
    private bool IsFloorAllowed(RoomData room, GridCoord origin)
    {
        if (room.HasFloorRestriction() == false)
        {
            return true;
        }
        return origin.Floor == room.RequiredFloor;
    }

    //최저 층 노출 + 해금 명령
    public bool TryUnlockNextFloor()
    {
        if (IsFloorRemaining() == false)
        {
            Debug.Log("[BuildGridViewModel] 더 해금할 층 없음");
            return false;
        }

        int cost = GetNextUnlockCost();

        if (_currencyService.IsAffordable(cost) == false)
        {
            Debug.Log($"[BuildGridViewModel] 해금 비용 부족 (필요 {cost}G)");
            return false;
        }

        bool success = _buildGridModel.TryUnlockNextFloor();

        if (success == false)
        {
            return false;  
        }

        _currencyService.TrySpend(cost);

        PlaceStairAt(_buildGridModel.UnlockedMinFloor);

        if (OnUnlockFloor != null)
        {
            OnUnlockFloor.Invoke(_buildGridModel.UnlockedMinFloor);
        }

        SaveGrid();
        Debug.Log($"[BuildGridViewModel] 층 해금: 이제 {_buildGridModel.UnlockedMinFloor}층까지 열림 (-{cost}G)");
        return true;
    }

    //층 해금 비용
    public int GetNextUnlockCost()
    {
        int nextFloor = _buildGridModel.UnlockedMinFloor - 1;
        if (nextFloor < _buildGridModel.Bounds.MinFloor)
        {
            return 0;   
        }

        int depth = Mathf.Abs(nextFloor);
        return UNLOCK_COST_PER_FLOOR * depth;
    }
    public bool IsFloorRemaining()
    {
        return _buildGridModel.UnlockedMinFloor - 1 >= _buildGridModel.Bounds.MinFloor;
    }

    public bool IsUnlockable()
    {
        if (IsFloorRemaining() == false)
        {
            return false;
        }

        return _currencyService.IsAffordable(GetNextUnlockCost());
    }

    //계단 자동 설치
    public void EnsureStairs()
    {
        GridBounds bounds = _buildGridModel.Bounds;
        int unlockedMin = _buildGridModel.UnlockedMinFloor;

        for (int floor = unlockedMin; floor <= bounds.MaxFloor; floor++)
        {
            PlaceStairAt(floor);
        }
    }

    private void PlaceStairAt(int floor)
    {
        RoomData stairData = GameDataManager.Inst.GetData<RoomData>(STAIR_ROOM_ID);

        if (stairData == null)
        {
            Debug.LogWarning($"[BuildGridViewModel] 계단 데이터 없음 : {STAIR_ROOM_ID}");
            return;
        }

        GridCoord origin = new GridCoord(floor, STAIR_MIN_COLUMN);

        if (_buildGridModel.GetRoomAt(origin) != null )
        {
            return;
        }

        PlacedRoomData stair = new PlacedRoomData();
        stair.RoomInstanceId = GameUtil.GenerateUniqueId();
        stair.RoomId = STAIR_ROOM_ID;
        stair.Origin = origin;

        List<GridCoord> coords = _gridSystem.GetOccupiedCoords(origin, stairData.GetSize());
        _buildGridModel.AddRoom(stair, coords);

        if (OnPlaceRoom != null)
        {
            OnPlaceRoom.Invoke(stair);
        }
    }

    //계단 철거이동 금지
    private bool IsStair(string roomId)
    {
        return roomId == STAIR_ROOM_ID;
    }



    // 평소(비건설) 상태에서 그리드를 클릭했을 때 처리
    public void HandleNormalClick(GridCoord coord)
    {
        PlacedRoomData room = _buildGridModel.GetRoomAt(coord);
        if (room == null)
        {
            return;
        }

        if (room.RoomId == OFFICE_ROOM_ID)
        {
            if (OnClickOffice != null)
            {
                OnClickOffice.Invoke();
            }
        }
        else if (room.RoomId == BATTLE_ROOM_ID)
        {
            if (OnClickBattle != null)
            {
                OnClickBattle.Invoke();
            }
        }
    }

    //현재 슬롯의 그리드를 다시 불러온다 (슬롯 변경 시)
    public void ReloadGrid()
    {
        LoadGrid();
        EnsureStairs();

        if (OnReloadGrid != null)
        {
            OnReloadGrid.Invoke();
        }
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
        SaveManager.Inst.RequestSaveData(player);
    }

    //그리드 불러오기
    public void LoadGrid()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        Debug.Log(player.PlayerName);

        if (player == null || player.BuildGridData == null)
        {
            Debug.Log("[BuildGridViewModel] 저장된 그리드 없음");
            return;
        }

        _buildGridModel.LoadFromSaveData(player.BuildGridData, _gridSystem);

        Debug.Log($"[BuildGridViewModel] 그리드 복원: 방 {player.BuildGridData.PlacedRooms.Count}개");
    }
}
