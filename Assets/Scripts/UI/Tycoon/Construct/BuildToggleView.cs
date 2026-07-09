using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 건설 모드 진입/종료 토글 버튼 뷰. ViewBase 상속.
/// 클릭 시 건설모드를 토글하고, 상태에 따라 버튼 라벨을 갱신
/// </summary>
public class BuildToggleView : ViewBase
{
    [Header("토글 버튼")]
    [SerializeField] private Button Button_Toggle;

    [Header("버튼 라벨")]
    [SerializeField] private TextMeshProUGUI Text_Label;

    private BuildGridViewModel _viewModel;

    public void Bind(BuildGridViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        if (Button_Toggle != null)
        {
            Button_Toggle.onClick.AddListener(OnClickToggle);
        }

        UpdateLabel();
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        if (Button_Toggle != null)
        {
            Button_Toggle.onClick.RemoveListener(OnClickToggle);
        }
    }

    private void OnClickToggle()
    {
        if (_viewModel != null)
        {
            _viewModel.ToggleBuildMode();
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BuildGridViewModel.IsBuildMode))
        {
            UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        if (Text_Label == null || _viewModel == null)
        {
            return;
        }

        if (_viewModel.IsBuildMode == true)
        {
            Text_Label.text = "완료";
        }
        else
        {
            Text_Label.text = "건설";
        }
    }
}