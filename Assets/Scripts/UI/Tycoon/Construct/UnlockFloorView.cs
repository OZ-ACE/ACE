using UnityEngine;
using UnityEngine.UI;
using TMPro;


// 누르면 한 층씩 아래로 해금(구매 연동은 추후)

public class UnlockFloorView : ViewBase
{
    [Header("해금 버튼")]
    [SerializeField] private Button Button_Unlock;

    [Header("라벨 (현재 열린 층 표시)")]
    [SerializeField] private TextMeshProUGUI Text_Label;

    private BuildGridViewModel _viewModel;

    public void Bind(BuildGridViewModel viewModel)
    {
        _viewModel = viewModel;

        if (Button_Unlock != null)
        {
            Button_Unlock.onClick.RemoveListener(OnClickUnlock);
            Button_Unlock.onClick.AddListener(OnClickUnlock);
        }

        GameManager.Inst.CurrencyService.OnChangeCurrency -= UpdateLabel;
        GameManager.Inst.CurrencyService.OnChangeCurrency += UpdateLabel;

        UpdateLabel();
    }

    private void OnEnable()
    {
        BuildGridViewModel viewModel = GameManager.Inst.BuildService.GetBuildGridViewModel();
        Bind(viewModel);
    }


    private void OnDestroy()
    {
        if (Button_Unlock != null)
        {
            Button_Unlock.onClick.RemoveListener(OnClickUnlock);
        }

        if (GameManager.Inst != null)
        {
            GameManager.Inst.CurrencyService.OnChangeCurrency -= UpdateLabel;
        }
    }

    private void OnClickUnlock()
    {
        _viewModel.TryUnlockNextFloor();
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (_viewModel.IsFloorRemaining() == false)
        {
            Text_Label.text = "최하층 도달";
            Button_Unlock.interactable = false;
            return;
        }

        int cost = _viewModel.GetNextUnlockCost();
        Text_Label.text = $"굴착 ({cost}G)";
        Button_Unlock.interactable = _viewModel.IsUnlockable();
    }
}