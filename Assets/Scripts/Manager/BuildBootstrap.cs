using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 건설 시스템 MVVM 조립 담당.
/// BuildService로 뷰모델을 만들고, 씬의 BuildGridView에 바인딩한다.
/// 추후 GameManager.StartGame으로 통합 
/// </summary>
public class BuildBootstrap : MonoBehaviour
{

    [Header("씬의 건설 뷰")]
    [SerializeField] private BuildGridView Build_GridView;
    [SerializeField] private BuildMenuView Build_MenuView;
    [SerializeField] private BuildToggleView Build_ToggleView;
    [SerializeField] private DemolishToggleView Build_DemolishView;
    [SerializeField] private MoveToggleView Build_MoveView;
    [SerializeField] private UnlockFloorView Build_UnlockView;


    [Header("셀의 크기")]
    [SerializeField] private float _cellWidth = 1f;
    [SerializeField] private float _cellHeight = 1f;


    [Header("그리드 범위")]
    [SerializeField] private int _minFloor = -10;
    [SerializeField] private int _maxFloor = 1;
    [SerializeField] private int _minColumn = 0;
    [SerializeField] private int _maxColumn = 19;


    [Header("건설 가능한 방 목록")]
    [SerializeField] private List<string> _buildableRoomIds = new List<string>();


    [Header("초기 해금 최저 층")]
    [SerializeField] private int _initialMinFloor = -3;   



    private BuildService _buildService;


    private void Start()
    {
        GridSystem gridSystem = new GridSystem(_cellWidth, _cellHeight, Vector2.zero);
        BuildGridModel gridModel = new BuildGridModel();

        _buildService = new BuildService();
        BuildGridViewModel viewModel = _buildService.CreateBuildGridViewModel(gridSystem, gridModel);

        GridBounds bounds = new GridBounds(_minFloor, _maxFloor, _minColumn, _maxColumn);
        viewModel.InitGrid(bounds, _initialMinFloor);
        viewModel.SetBuildableRooms(_buildableRoomIds);


        Build_GridView.Bind(viewModel);
        Build_MenuView.Bind(viewModel);
        Build_ToggleView.Bind(viewModel);
        Build_DemolishView.Bind(viewModel);
        Build_MoveView.Bind(viewModel);
        Build_UnlockView.Bind(viewModel);

        viewModel.LoadGrid();

        Debug.Log("[BuildBootstrap] 건설 시스템 배선 완료");
    }

}
