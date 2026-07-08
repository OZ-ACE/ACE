using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.ComponentModel;


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


    private BuildGridViewModel _viewModel;

    private Camera _mainCamera;


    private Dictionary<GridCoord, SpriteRenderer> _cellRenderers = new Dictionary<GridCoord, SpriteRenderer>();
    private bool _isOverlayCreated;

    private GridCoord _lastHoverCoord;
    private bool _hasHover;


    public void Bind(BuildGridViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

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
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(BuildGridViewModel.IsBuildMode))
        {
            ApplyBuildMode(_viewModel.IsBuildMode);
        }
    }

    private void ApplyBuildMode(bool isBuildMode)
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
        }
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

    }


    //마우스가 올라간 칸을 하이라이트
    private void UpdateHover()
    {
        if (_mainCamera == null || Mouse.current == null)
        {
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseScreen = new Vector3(mousePos.x, mousePos.y, -_mainCamera.transform.position.z);
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);

        GridCoord coord = _viewModel.GridSystem.GetCoord(mouseWorld);

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
}
