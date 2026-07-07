
//건설 뷰모델을 생성·보관
public class BuildService
{
    private BuildGridViewModel _buildGridViewModel;

    public BuildGridViewModel CreateBuildGridViewModel (GridSystem gridSystem, BuildGridModel gridModel)
    {
        if (_buildGridViewModel != null)
        {
            return _buildGridViewModel;
        }
        _buildGridViewModel = new BuildGridViewModel (gridSystem, gridModel);
        return _buildGridViewModel;
    }

    public BuildGridViewModel GetBuildGridViewModel()
    {
        return _buildGridViewModel;
    }
}
