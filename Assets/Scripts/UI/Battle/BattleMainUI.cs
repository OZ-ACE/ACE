using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 메인 UI 전체를 관리하는 스크립트 (배틀 로그 표시도 담당)
public class BattleMainUI : UIBase
{
    [Header("배틀 로그")]
    [SerializeField] private ScrollRect ScrollRect_BattleLog;
    [SerializeField] private Transform Transform_LogContent;

    private BattleViewModel _viewModel;

    private void Start()
    {
        _viewModel = new BattleViewModel();
        BindViewModel(_viewModel);
    }

    private void BindViewModel(BattleViewModel viewModel)
    {
        _viewModel.PropertyChanged += OnPropertyChanged_View;
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.BattleLogs):
                AppendNewLogSlots();
                break;
        }
    }

    // 이미 그려진 슬롯 개수 이후로 늘어난 로그만 추가 생성
    private void AppendNewLogSlots()
    {
        int alreadyDrawnCount = Transform_LogContent.childCount;

        for (int i = alreadyDrawnCount; i < _viewModel.BattleLogs.Count; i++)
        {
            GameObject loadedObj = (GameObject)Resources.Load("Prefabs/UI/BattleLogSlot");
            GameObject slot = Instantiate(loadedObj, Transform_LogContent);

            TextMeshProUGUI logText = slot.GetComponent<TextMeshProUGUI>();

            if (logText != null)
            {
                logText.text = _viewModel.BattleLogs[i];
                logText.ForceMeshUpdate();
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(Transform_LogContent.GetComponent<RectTransform>());

        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        ScrollRect_BattleLog.verticalNormalizedPosition = 0f;
    }

    // 테스트용 - 실제 전투 로직 연결 전까지 더미 로그 확인용, 이후 삭제 예정
    public void AddDummyLog(string message)
    {
        _viewModel.AddBattleLog(message);
    }

    // 테스터용 - 검증 끝나면 삭제
    private int _dummyLogCount = 0;

    [ContextMenu("더미 로그 추가")]
    private void Test_AddDummyLog()
    {
        _dummyLogCount++;
        AddDummyLog($"테스트 로그 {_dummyLogCount}번째 줄입니다.");
    }
}