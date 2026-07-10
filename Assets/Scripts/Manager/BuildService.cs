using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 건설 시스템 조립·보관 담당. GameManager가 생성해 들고 있는다.
/// </summary>
public class BuildService
{
    // ===== 그리드 설정 =====
    private const float CELL_WIDTH = 1f;
    private const float CELL_HEIGHT = 1f;

    private const int MIN_FLOOR = -10;
    private const int MAX_FLOOR = 1;
    private const int MIN_COLUMN = 0;
    private const int MAX_COLUMN = 19;

    private const int INITIAL_MIN_FLOOR = -3;

    private readonly ICurrencyService _currencyService;

    private BuildGridViewModel _buildGridViewModel;

    public BuildService(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
        InitBuildSystem();
    }

    /// <summary> 그리드·모델·뷰모델 조립 후 저장 데이터 복원 </summary>
    private void InitBuildSystem()
    {
        GridSystem gridSystem = new GridSystem(CELL_WIDTH, CELL_HEIGHT, Vector2.zero);
        BuildGridModel gridModel = new BuildGridModel();

        _buildGridViewModel = new BuildGridViewModel(gridSystem, gridModel, _currencyService);

        GridBounds bounds = new GridBounds(MIN_FLOOR, MAX_FLOOR, MIN_COLUMN, MAX_COLUMN);

        _buildGridViewModel.InitGrid(bounds, INITIAL_MIN_FLOOR);
        _buildGridViewModel.SetBuildableRooms(GetAllRoomIds());
        _buildGridViewModel.LoadGrid();

        Debug.Log("[BuildService] 건설 시스템 조립 완료");
    }

    /// <summary> 건설 가능한 방 ID 전체 (Room 테이블 기준) </summary>
    private List<string> GetAllRoomIds()
    {
        List<string> roomIds = new List<string>();

        List<RoomData> roomList = GameDataManager.Inst.GetDataList<RoomData>();
        if (roomList == null)
        {
            Debug.LogWarning("[BuildService] Room 데이터 없음");
            return roomIds;
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            roomIds.Add(roomList[i].ID);
        }
        return roomIds;
    }

    /// <summary> 조립된 뷰모델 반환 </summary>
    public BuildGridViewModel GetBuildGridViewModel()
    {
        return _buildGridViewModel;
    }
}