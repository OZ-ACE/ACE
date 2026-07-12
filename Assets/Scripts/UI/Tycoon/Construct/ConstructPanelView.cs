using UnityEngine;


public class ConstructPanelView : ViewBase
{
    private BuildGridViewModel _viewModel;

    private void OnEnable()
    {
        _viewModel = GameManager.Inst.Services.BuildService.GetBuildGridViewModel();
        Debug.Log("[ConstructPanelView] 건설모드 진입");
        _viewModel.EnterBuildMode();
    }

    private void OnDisable()
    {
        if (_viewModel != null)
        {
            _viewModel.ExitBuildMode();
        }
    }
}