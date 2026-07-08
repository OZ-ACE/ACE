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
            Button_Unlock.onClick.AddListener(OnClickUnlock);
        }

        UpdateLabel();
    }

    private void OnDestroy()
    {
        if (Button_Unlock != null)
        {
            Button_Unlock.onClick.RemoveListener(OnClickUnlock);
        }
    }

    private void OnClickUnlock()
    {
        if (_viewModel != null)
        {
            _viewModel.TryUnlockNextFloor();
            UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        if (Text_Label == null || _viewModel == null)
        {
            return;
        }
        Text_Label.text = $"해금 (현재 {_viewModel.UnlockedMinFloor}층)";
    }
}