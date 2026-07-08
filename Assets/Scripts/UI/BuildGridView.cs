using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.ComponentModel;
using UnityEngine.EventSystems;


public class BuildGridView : ViewBase
{
    [Header("셀 프리팹 (반투명 사각형)")]
    [SerializeField] private GameObject Prefab_Cell;

    [Header("그리드 설정")]
    [SerializeField] private int _minFloor = -10;
    [SerializeField] private int _maxFloor = 1;
    [SerializeField] private int _minColumn = 0;
    [SerializeField] private int _maxColumn = 19;

    [Header("색상")]
    [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color _hoverColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color _ghostValidColor = new Color(0f, 1f, 0f, 0.5f);   // 배치 가능(초록)
    [SerializeField] private Color _ghostInvalidColor = new Color(1f, 0f, 0f, 0.5f); // 배치 불가(빨강)
    [SerializeField] private Color _placedColor = new Color(0.3f, 0.5f, 1f, 0.8f);   // 배치된 방(파랑)


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
            _viewModel.OnPlaceRoom -= OnPlaceRoom;
            _viewModel.OnRemoveRoom -= OnRemoveRoom;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.OnPlaceRoom += OnPlaceRoom;
        _viewModel.OnRemoveRoom += OnRemoveRoom;

        _viewModel.InvokeOnceOnInit();
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
            _viewModel.OnPlaceRoom -= OnPlaceRoom;
            _viewModel.OnRemoveRoom -= OnRemoveRoom;

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
        GridSystem grid = _viewModel.GridSystem;

        for (int floor = _minFloor; floor <= _maxFloor; floor++)
        {
            for (int column = _minColumn; column <= _maxColumn; column++)
            {
                GridCoord coord = new GridCoord(floor, column);
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
        }

        _isOverlayCreated = true;
        Debug.Log($"[BuildGridView] 셀 오버레이 {_cellRenderers.Count}개 생성");
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

        if (_viewModel == null ||  _viewModel.IsBuildMode == false)
        {
            return;
        }

        UpdateHover();

        if (_viewModel.IsDemolishMode == true)
        {
            HideGhost();
            UpdateDemolishClick();
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
    private void OnPlaceRoom(PlacedRoomData placed)
    {
        RoomData room = GameDataManager.Inst.GetData<RoomData>(placed.RoomId);
        if (room == null)
        {
            return;
        }

        Vector2Int size = room.GetSize();
        GridSystem grid = _viewModel.GridSystem;
        Vector3 originWorld = grid.GetWorldPosition(placed.Origin);
        float offsetX = (size.x - 1) * 0.5f * grid.CellWidth;
        float offsetY = (size.y - 1) * 0.5f * grid.CellHeight;

        GameObject roomObj = Instantiate(Prefab_Cell, this.transform);
        roomObj.name = $"Room_{placed.RoomId}_{placed.Origin}";
        roomObj.transform.position = new Vector3(originWorld.x + offsetX, originWorld.y + offsetY, 0f);
        roomObj.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = roomObj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = _placedColor;
        }

        _placedRoomObjects[placed.Origin] = roomObj;
    }

    // 방 철거
    private void OnRemoveRoom(PlacedRoomData removed)
    {
        if (_placedRoomObjects.TryGetValue(removed.Origin, out var roomObj))
        {
            Destroy(roomObj);
            _placedRoomObjects.Remove(removed.Origin);
        }
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


}
