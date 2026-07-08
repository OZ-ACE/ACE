using UnityEngine;


//Model 역할 + ViewModel 역할
public class BuildGridViewModel : ViewModelBase
{
    private readonly GridSystem _gridSystem;
    private readonly BuildGridModel _buildGridModel;

    public GridSystem GridSystem { get { return _gridSystem; } }
    public BuildGridModel BuildGridModel { get { return _buildGridModel; } }

    public BuildGridViewModel(GridSystem gridSystem, BuildGridModel buildGridModel)
    {
        _gridSystem = gridSystem;
        _buildGridModel = buildGridModel;
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


}
