using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettlementView : ViewBase
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI Text_Day;
    [SerializeField] private TextMeshProUGUI Text_TodayFragment;
    [SerializeField] private TextMeshProUGUI Text_TotalFragment;
    [SerializeField] private TextMeshProUGUI Text_Gold;

    [Header("버튼")]
    [SerializeField] private Button Button_Confirm;
    [SerializeField] private Button Button_Close;

    private SettlementViewModel _viewModel;

    /// <summary> 뷰모델 바인딩 </summary>
    public void Bind(SettlementViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.InvokeOnceOnInit();
    }

    private void OnEnable()
    {
        Button_Confirm.onClick.RemoveListener(OnClickConfirm);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
        Button_Close.onClick.RemoveListener(OnClickClose);
        Button_Close.onClick.AddListener(OnClickClose);

        if (_viewModel == null)
        {
            SettlementViewModel vm = GameManager.Inst.Services.SettlementService.GetSettlementViewModel();
            if (vm == null)
            {
                vm = GameManager.Inst.Services.SettlementService.CreateSettlementViewModel();
            }
            Bind(vm);
        }
        else
        {
            _viewModel.InvokeOnceOnInit();
        }
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateAllText();
    }

    private void UpdateAllText()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (Text_Day != null)
        {
            Text_Day.text = $"Day {_viewModel.CurrentDay}";
        }
        if (Text_TodayFragment != null)
        {
            Text_TodayFragment.text = $"{_viewModel.TodayMemoryFragment}";
        }
        if (Text_TotalFragment != null)
        {
            Text_TotalFragment.text = $"{_viewModel.CurrentMemoryFragment}";
        }
        if (Text_Gold != null)
        {
            Text_Gold.text = $"{_viewModel.CurrentGold}";
        }
    }

    private void OnClickConfirm()
    {
        if (_viewModel.TryConfirmSettlement() == false)
        {
            Debug.Log("[SettlementView] 정산 실패 - 다음날로 넘길 수 없음");
            return;
        }
        UIManager.Inst.CloseSettlementUI();
    }

    private void OnClickClose()
    {
        UIManager.Inst.CloseSettlementUI();
    }
}