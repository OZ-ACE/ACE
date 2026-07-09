using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MoveToggleView : ViewBase
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

    private void OnEnable()
    {
        BuildGridViewModel viewModel = GameManager.Inst.BuildService.GetBuildGridViewModel();
        Bind(viewModel);
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
            _viewModel.ToggleMoveMode();
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BuildGridViewModel.IsMoveMode))
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

        if (_viewModel.IsMoveMode == true)
        {
            Text_Label.text = "이동 중";
        }
        else
        {
            Text_Label.text = "이동";
        }
    }
}