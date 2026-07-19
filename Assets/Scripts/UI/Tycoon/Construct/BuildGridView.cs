using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.ComponentModel;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class BuildGridView : ViewBase
{
    [Header("셀 프리팹 (반투명 사각형)")]
    [SerializeField] private GameObject Prefab_Cell;

    [Header("색상")]
    [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color _hoverColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color _ghostValidColor = new Color(0f, 1f, 0f, 0.5f);   // 배치 가능(초록)
    [SerializeField] private Color _ghostInvalidColor = new Color(1f, 0f, 0f, 0.5f); // 배치 불가(빨강)
    [SerializeField] private Color _placedColor = new Color(0.3f, 0.5f, 1f, 0.8f);   // 배치된 방(파랑)

    [Header("방 프리팹 캐시")]
    private Dictionary<string, GameObject> _roomPrefabCache = new Dictionary<string, GameObject>();

    private BuildGridViewModel _viewModel;
    private Camera _mainCamera;

    private Dictionary<GridCoord, SpriteRenderer> _cellRenderers = new Dictionary<GridCoord, SpriteRenderer>();
    private Dictionary<GridCoord, GameObject> _placedRoomObjects = new Dictionary<GridCoord, GameObject>();
    private bool _isOverlayCreated;

    private GridCoord _lastHoverCoord;
    private bool _hasHover;

    private GameObject _ghostObject;
    private SpriteRenderer _ghostRenderer;

    public void Bind(BuildGridViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OnPlaceRoom -= BindOnPlaceRoom;
            _viewModel.OnRemoveRoom -= OnRemoveRoom;
            _viewModel.OnUnlockFloor -= OnUnlockFloor;
            _viewModel.OnReloadGrid -= RefreshAllRooms;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.OnPlaceRoom += BindOnPlaceRoom;
        _viewModel.OnRemoveRoom += OnRemoveRoom;
        _viewModel.OnUnlockFloor += OnUnlockFloor;
        _viewModel.OnReloadGrid += RefreshAllRooms;

        _viewModel.LoadGrid();
        _viewModel.EnsureStairs();
        RefreshAllRooms();
        _viewModel.InvokeOnceOnInit();
    }

    private void BindOnPlaceRoom(PlacedRoomData data)
    {
        OnPlaceRoom(data).Forget();
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        if (_viewModel != null )
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OnPlaceRoom -= BindOnPlaceRoom;
            _viewModel.OnRemoveRoom -= OnRemoveRoom;
            _viewModel.OnUnlockFloor -= OnUnlockFloor;
            _viewModel.OnReloadGrid -= RefreshAllRooms;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(BuildGridViewModel.IsBuildMode))
        {
            ApplyBuildMode(_viewModel.IsBuildMode);
        }
        else if (e.PropertyName == nameof(BuildGridViewModel.SelectedRoomId))
        {
            ApplySelectedRoom();
        }
    }

    private void ApplyBuildMode (bool isBuildMode)
    {
        Debug.Log($"[BuildGridView] ApplyBuildMode({isBuildMode}), 오버레이생성됨={_isOverlayCreated}");

        if (isBuildMode == true)
        {
            if (_isOverlayCreated == false)
            {
                CreateGridOverlay();
            }
            SetOverlayActive(true);
        }
        else
        {
            SetOverlayActive(false);
            HideGhost();
        }
    }

    private void ApplySelectedRoom()
    {
        RoomData room = _viewModel.GetSelectedRoomData();
        if (room == null)
        {
            HideGhost();
            return;
        }
        PrepareGhost(room);
    }

    //격자 범위만큼 셀 프리팹을 깔아 오버레이 생성
    private void CreateGridOverlay()
    {
        GridBounds bounds = _viewModel.Bounds;
        int unlockedMin = _viewModel.UnlockedMinFloor;   

        for (int floor = unlockedMin; floor <= bounds.MaxFloor; floor++)
        {
            for (int column = bounds.MinColumn; column <= bounds.MaxColumn; column++)
            {
                CreateCell(new GridCoord(floor, column));
            }
        }

        _isOverlayCreated = true;
        Debug.Log($"[BuildGridView] 셀 오버레이 {_cellRenderers.Count}개 생성 (열린 최저층: {unlockedMin})");
    }

    private void CreateCell(GridCoord coord)
    {
        if (_cellRenderers.ContainsKey(coord) == true)
        {
            return;   
        }

        GridSystem grid = _viewModel.GridSystem;
        Vector3 worldPos = grid.GetWorldPosition(coord);

        GameObject cell = Instantiate(Prefab_Cell, worldPos, Quaternion.identity, this.transform);
        cell.name = $"Cell_{coord}";

        SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = _normalColor;
            _cellRenderers[coord] = renderer;
        }
    }

    //해금된 층 셀 생성
    private void OnUnlockFloor(int newUnlockedMin)
    {
        GridBounds bounds = _viewModel.Bounds;

        for (int column = bounds.MinColumn; column <= bounds.MaxColumn; column++)
        {
            CreateCell(new GridCoord(newUnlockedMin, column));
        }

        SoundManager.Inst.PlaySFX("Construct");

        Debug.Log($"[BuildGridView] {newUnlockedMin}층 셀 추가 생성");
    }

    private void SetOverlayActive(bool isActive)
    {
        foreach (var pair  in _cellRenderers)
        {
            pair.Value.gameObject.SetActive(isActive);
        }
    }

    private void Update()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (_viewModel.IsBuildMode == false)
        {
            HandleNormalClick();
            return;
        }

        UpdateHover();

        if (_viewModel.IsDemolishMode == true)
        {
            HideGhost();
            UpdateDemolishClick();
        }
        else if (_viewModel.IsMoveMode == true)
        {
            UpdateMove();
        }
        else
        {
            UpdateGhost();
            UpdateClick();
        }
    }

    //마우스가 올라간 칸을 하이라이트
    private void UpdateHover()
    {
        GridCoord coord;
        if (TryGetMouseCoord(out coord) == false)
        {
            return;
        }

        if (_hasHover && coord == _lastHoverCoord)
        {
            return;
        }

        ClearHover();

        if (_cellRenderers.TryGetValue(coord, out var renderer))
        {
            renderer.color = _hoverColor;
            _lastHoverCoord = coord;
            _hasHover = true;
        }
    }

    // 하이라이트를 원래 색으로 되돌림
    private void ClearHover()
    {
        if (_hasHover && _cellRenderers.TryGetValue(_lastHoverCoord, out var renderer))
        {
            renderer.color = _normalColor;
        }
        _hasHover = false;
    }

    // 고스트 준비/갱신/숨김
    private void PrepareGhost(RoomData room)
    {
        if (_ghostObject == null)
        {
            _ghostObject = Instantiate(Prefab_Cell, Vector3.zero, Quaternion.identity, this.transform);
            _ghostObject.name = "Ghost";
            _ghostRenderer = _ghostObject.GetComponent<SpriteRenderer>();
        }

        Vector2Int size = room.GetSize();
        _ghostObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        _ghostObject.SetActive(true);
    }

    private void UpdateGhost()
    {
        if (_ghostObject == null || _ghostObject.activeSelf == false)
        {
            return;
        }

        GridCoord coord;
        if (TryGetMouseCoord(out coord) == false)
        {
            return;
        }

        RoomData room = _viewModel.GetSelectedRoomData();
        if (room == null)
        {
            HideGhost();
            return;
        }

        Vector2Int size = room.GetSize();
        GridSystem grid = _viewModel.GridSystem;
        Vector3 originWorld = grid.GetWorldPosition(coord);
        float offsetX = (size.x - 1) * 0.5f * grid.CellWidth;
        float offsetY = (size.y - 1) * 0.5f * grid.CellHeight;
        _ghostObject.transform.position = new Vector3(originWorld.x + offsetX, originWorld.y + offsetY, 0f);

        PlacementResult result = _viewModel.CheckSelectedRoomPlaceable(coord);
        if (_ghostRenderer != null)
        {
            _ghostRenderer.color = (result == PlacementResult.Success) ? _ghostValidColor : _ghostInvalidColor;
        }
    }

    private void HideGhost()
    {
        if (_ghostObject != null)
        {
            _ghostObject.SetActive(false);
        }
    }

    // 클릭 배치
    private void UpdateClick()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame == false)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }


        if (string.IsNullOrEmpty(_viewModel.SelectedRoomId) == true)
        {
            return;
        }

        GridCoord coord;
        if (TryGetMouseCoord(out coord) == false)
        {
            return;
        }

        PlacementResult result = _viewModel.TryPlaceRoom(_viewModel.SelectedRoomId, coord);
        if (result != PlacementResult.Success)
        {
            Debug.Log($"[BuildGridView] 배치 실패: {result}");
        }
        else
        {
            SoundManager.Inst.PlaySFX("Construct");
        }
    }

    // 방 철거 클릭
    private void UpdateDemolishClick()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame == false)
        {
            return;
        }

        // UI 위 클릭은 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        GridCoord coord;
        if (TryGetMouseCoord(out coord) == false)
        {
            return;
        }

        bool success = _viewModel.TryDemolishRoom(coord);

        if (success == false)
        {
            Debug.Log($"[BuildGridView] 철거할 방 없음 @ {coord}");
        }
    }

    // 방 건설
    private async UniTask OnPlaceRoom(PlacedRoomData placed)
    {
        RoomData room = GameDataManager.Inst.GetData<RoomData>(placed.RoomId);
        if (room == null)
        {
            return;
        }

        int currentFloor = Mathf.Abs(placed.Origin.Floor);
        string finalPrefabPath = room.PrefabPath;

        if (room.ID.Contains("Stair") || room.PrefabPath.Contains("Stair"))
        {
            if (currentFloor % 2 == 0)
            {
                finalPrefabPath = $"{room.PrefabPath}_Left";
            }
            else
            {
                finalPrefabPath = $"{room.PrefabPath}_Right";
            }
        }

        GameObject prefab = await LoadRoomPrefab(room.ID, finalPrefabPath);

        if (prefab == null)
        {
            return;
        }

        Vector3 worldPos = GetRoomCenterPosition(placed.Origin, room.GetSize());

        GameObject roomObj = Instantiate(prefab, worldPos, Quaternion.identity, this.transform);
        roomObj.name = $"Room_{placed.RoomId}_{placed.Origin}";

        _placedRoomObjects[placed.Origin] = roomObj;

        NavMeshManager.Inst.UpdateNavMesh();
    }

    // 방의 중앙 월드 좌표
    private Vector3 GetRoomCenterPosition(GridCoord origin, Vector2Int size)
    {
        GridSystem grid = _viewModel.GridSystem;
        Vector3 originWorld = grid.GetWorldPosition(origin);

        float offsetX = (size.x - 1) * 0.5f * grid.CellWidth;
        float offsetY = (size.y - 1) * 0.5f * grid.CellHeight;

        return new Vector3(originWorld.x + offsetX, originWorld.y + offsetY, 0f);
    }

    // 방 철거
    private void OnRemoveRoom(PlacedRoomData removed)
    {
        if (_placedRoomObjects.TryGetValue(removed.Origin, out var roomObj))
        {
            RoomData room = GameDataManager.Inst.GetData<RoomData>(removed.RoomId);

            roomObj.gameObject.SetActive(false);
            Destroy(roomObj);
            _placedRoomObjects.Remove(removed.Origin);

            NavMeshManager.Inst.UpdateNavMesh();
            SoundManager.Inst.PlaySFX("Construct");
        }
    }

    // 방 생성
    private void RefreshAllRooms()
    {
        foreach (KeyValuePair<GridCoord, GameObject> pair in _placedRoomObjects)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }

        _placedRoomObjects.Clear();

        foreach (KeyValuePair<GridCoord, SpriteRenderer> pair in _cellRenderers)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }

        _cellRenderers.Clear();
        _isOverlayCreated = false;

        // 방 다시 그리기
        List<PlacedRoomData> rooms = _viewModel.GetPlacedRooms();
        for (int i = 0; i < rooms.Count; i++)
        {
            OnPlaceRoom(rooms[i]).Forget();
        }

        NavMeshManager.Inst.BuildNavMesh();
        Debug.Log($"[BuildGridView] 방 {rooms.Count}개 재생성");
    }

    // 방 이동
    private void UpdateMove()
    {
        if (_viewModel.IsHoldingRoom == false)
        {
            HideGhost();

            if (IsClickedThisFrame() == false)
            {
                return;
            }

            GridCoord coord;
            if (TryGetMouseCoord(out coord) == false)
            {
                return;
            }

            _viewModel.TryPickRoom(coord);
            return;
        }

        UpdatePickedGhost();

        if (IsClickedThisFrame() == true)
        {
            GridCoord coord;
            if (TryGetMouseCoord(out coord) == false)
            {
                return;
            }

            bool dropped = _viewModel.TryDropRoom(coord);
            if (dropped == true)
            {
                HideGhost();
            }
            else
            {
                Debug.Log("[BuildGridView] 그 자리엔 놓을 수 없음 (계속 들고 있음)");
            }
        }
    }

    //집은 방의 고스트 표시
    private void UpdatePickedGhost()
    {
        PlacedRoomData picked = _viewModel.PickedRoom;
        if (picked == null)
        {
            HideGhost();
            return;
        }

        RoomData room = GameDataManager.Inst.GetData<RoomData>(picked.RoomId);
        if (room == null)
        {
            HideGhost();
            return;
        }

        GridCoord coord;
        if (TryGetMouseCoord(out coord) == false)
        {
            return;
        }

        if (_ghostObject == null)
        {
            _ghostObject = Instantiate(Prefab_Cell, Vector3.zero, Quaternion.identity, this.transform);
            _ghostObject.name = "Ghost";
            _ghostRenderer = _ghostObject.GetComponent<SpriteRenderer>();
        }

        Vector2Int size = room.GetSize();
        _ghostObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        _ghostObject.SetActive(true);

        GridSystem grid = _viewModel.GridSystem;
        Vector3 originWorld = grid.GetWorldPosition(coord);
        float offsetX = (size.x - 1) * 0.5f * grid.CellWidth;
        float offsetY = (size.y - 1) * 0.5f * grid.CellHeight;
        _ghostObject.transform.position = new Vector3(originWorld.x + offsetX, originWorld.y + offsetY, 0f);

        PlacementResult result = _viewModel.CheckPickedRoomPlaceable(coord);
        if (_ghostRenderer != null)
        {
            _ghostRenderer.color = (result == PlacementResult.Success) ? _ghostValidColor : _ghostInvalidColor;
        }
    }

    //왼쪽 클릭 여부 및 UI 위 클릭 판정
    private bool IsClickedThisFrame()
    {
        if (Mouse.current == null)
        {
            return false;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame == false)
        {
            return false;
        }
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return false;
        }
        return true;
    }


    //평소 상태에서 방을 클릭하면 뷰모델에 전달
    private void HandleNormalClick()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame == false)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        if (TryGetMouseCoord(out GridCoord coord) == false)
        {
            return;
        }

        _viewModel.HandleNormalClick(coord);
    }



    // 공통: 마우스 셀 좌표
    private bool TryGetMouseCoord(out GridCoord coord)
    {
        coord = default;
        if (_mainCamera == null || Mouse.current == null)
        {
            return false;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseScreen = new Vector3(mousePos.x, mousePos.y, -_mainCamera.transform.position.z);
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);

        coord = _viewModel.GridSystem.GetCoord(mouseWorld);
        return true;
    }

    // RoomData.PrefabPath로 방 프리팹을 로드
    private async UniTask<GameObject> LoadRoomPrefab(string roomId, string prefabPath)
    {
        string cacheKey = $"{roomId}_{prefabPath}";

        if (_roomPrefabCache.TryGetValue(cacheKey, out GameObject cached))
        {
            return cached;
        }

        GameObject prefab = await ResourceManager.Inst.LoadAsset<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogWarning($"[BuildGridView] 방 프리팹 없음: {prefabPath}");
            return null;
        }

        _roomPrefabCache[cacheKey] = prefab;
        return prefab;
    }
}