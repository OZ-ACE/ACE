using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BuildGridView : ViewBase
{
    [Header("셀 프리팹 (반투명 사각형)")]
    [SerializeField] private GameObject Prefab_Cell;

    [Header("그리드 설정")]
    [SerializeField] private float _cellWidth = 1f;
    [SerializeField] private float _cellHeight = 1f;
    [SerializeField] private int _minFloor = -10;
    [SerializeField] private int _maxFloor = 1;
    [SerializeField] private int _minColumn = 0;
    [SerializeField] private int _maxColumn = 19;

    [Header("색상")]
    [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color _hoverColor = new Color(1f, 1f, 0f, 0.5f);

    private GridSystem _gridSystem;
    private Camera _mainCamera;

    private Dictionary<GridCoord, SpriteRenderer> _cellRenderers = new Dictionary<GridCoord, SpriteRenderer>();

    private GridCoord _lastHoverCoord;
    private bool _hasHover;

    private void Start()
    {
        _mainCamera = Camera.main;
        _gridSystem = new GridSystem(_cellWidth, _cellHeight, Vector2.zero);

        CreateGridOverlay();
    }

    //격자 범위만큼 셀 프리팹을 깔아 오버레이 생성
    private void CreateGridOverlay()
    {
        for (int floor = _minFloor;  floor <= _maxFloor; floor++)
        {
            for (int column = _minColumn; column <= _maxColumn; column++)
            {
                GridCoord coord = new GridCoord(floor, column);
                Vector3 worldPos = _gridSystem.GetWorldPosition(coord);

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
        Debug.Log($"[BuildGridView] 셀 오버레이 {_cellRenderers.Count}개 생성");
    }

    private void Update()
    {
        UpdateHover();
    }

    //마우스가 올라간 칸을 하이라이트
    private void UpdateHover()
    {
        if (_mainCamera == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseScreen = new Vector3(mousePos.x, mousePos.y, -_mainCamera.transform.position.z);
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);

        GridCoord coord = _gridSystem.GetCoord(mouseWorld);

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
