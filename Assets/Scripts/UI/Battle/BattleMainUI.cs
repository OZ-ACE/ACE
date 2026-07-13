using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 메인 UI 전체를 관리하는 스크립트 (배틀 로그 표시도 담당)
public class BattleMainUI : UIBase
{
    [Header("액션 큐")]
    [SerializeField] private Transform Transform_ActionQueueContent;

    [Header("배틀 로그")]
    [SerializeField] private ScrollRect ScrollRect_BattleLog;
    [SerializeField] private Transform Transform_LogContent;

    [Header("에너지 게이지")]
    [SerializeField] private Image[] Image_EnergySlotList;

    private const float DimmedEnergyAlpha = 0.2f;

    [Header("액션 버튼")]
    [SerializeField] private Button Button_Reinforce;
    [SerializeField] private Button Button_HealUnit;
    [SerializeField] private Button Button_ChangeUnit;

    private const int ReinforceEnergyCost = 1; //temp
    private const int ChangeUnitEnergyCost = 2; //temp
    private const int HealUnitEnergyCost = 2; //temp

    private string _selectedTargetUnitId;

    private BattleViewModel _viewModel;

    private void Start()
    {
        _viewModel = new BattleViewModel();
        BindViewModel(_viewModel);
        BindBattleUnitSpawner();

        BattleManager.Inst.ResetBattleState();
        SetEnergyGauge(BattleManager.Inst.GetRemainingEnergy());
    }

    private void BindViewModel(BattleViewModel viewModel)
    {
        _viewModel.PropertyChanged += OnPropertyChanged_View;

        Button_Reinforce.onClick.AddListener(OnClickReinforce);
        Button_HealUnit.onClick.AddListener(OnClickHealUnit);
        Button_ChangeUnit.onClick.AddListener(OnClickChangeUnit);
    }

    private void BindBattleUnitSpawner()
    {
        if (BattleUnitTestSpawner.Inst == null)
        {
            return;
        }

        BattleUnitTestSpawner.Inst.OnUnitClicked += OnUnitClicked_Spawner;
    }

    private void OnUnitClicked_Spawner(string unitId)
    {
        _selectedTargetUnitId = unitId;
        _viewModel.AddBattleLog($"{unitId} 선택됨");
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;

            Button_Reinforce.onClick.RemoveListener(OnClickReinforce);
            Button_HealUnit.onClick.RemoveListener(OnClickHealUnit);
            Button_ChangeUnit.onClick.RemoveListener(OnClickChangeUnit);
        }

        if (BattleUnitTestSpawner.Inst != null)
        {
            BattleUnitTestSpawner.Inst.OnUnitClicked -= OnUnitClicked_Spawner;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.BattleLogs):
                AppendNewLogSlots();
                break;
            case nameof(_viewModel.ActionQueue):
                RefreshActionQueue(_viewModel.ActionQueue);
                break;
        }
    }

    // 액션 큐 슬롯을 전부 지우고 넘겨받은 목록으로 새로 채운다
    public void RefreshActionQueue(List<BattleActionModel> actionList)
    {
        ClearActionQueueSlots();

        foreach (BattleActionModel action in actionList)
        {
            CreateActionQueueSlot(action);
        }
    }

    private void ClearActionQueueSlots()
    {
        for (int i = Transform_ActionQueueContent.childCount - 1; i >= 0; i--)
        {
            Destroy(Transform_ActionQueueContent.GetChild(i).gameObject);
        }
    }

    private void CreateActionQueueSlot(BattleActionModel action)
    {
        GameObject loadedObj = (GameObject)Resources.Load("Prefabs/UI/BattleActionSlot");
        GameObject slotObj = Instantiate(loadedObj, Transform_ActionQueueContent);

        BattleActionSlot slot = slotObj.GetComponent<BattleActionSlot>();

        if (slot != null)
        {
            slot.SetSlotData(action.Unit, action.ActionType);
        }
    }

    // 이미 그려진 배틀로그 슬롯 개수 이후로 늘어난 로그만 추가 생성
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
        AddDummyLog($"테스트 로그 {_dummyLogCount}번째 줄입니다. 이것은 텍스트 길이가 길어서 슬롯 안에서 두 줄 이상으로 줄바꿈되는지 확인하기 위한 테스트 로그입니다.");
    }

    //남은 에너지 수만큼 원래 색, 소모된 에너지는 흐리게 표시
    public void SetEnergyGauge(int currentEnergy)
    {
        for (int i =0; i < Image_EnergySlotList.Length; i++)
        {
            Color color = Image_EnergySlotList[i].color;
            color.a = i < currentEnergy ? 1f : DimmedEnergyAlpha;
            Image_EnergySlotList[i].color = color;
        }
    }

    private void OnClickReinforce()
    {
        ExecuteInterventionAction(BattleActionResult.Reinforce, ReinforceEnergyCost, "지원하기 실행");
    }

    private void OnClickHealUnit()
    {
        ExecuteInterventionAction(BattleActionResult.HealUnit, HealUnitEnergyCost, "영웅 회복 실행");
    }

    private void OnClickChangeUnit()
    {
        ExecuteInterventionAction(BattleActionResult.ChangeUnit, ChangeUnitEnergyCost, "영웅 교체 실행");
    }

    //선택된 대상에게 개입 액션을 실제로 적용한다. 대상 미선택 또는 에너지 부족 시 실패 처리
    private void ExecuteInterventionAction(BattleActionResult result, int energyCost, string logMessage)
    {
        if (string.IsNullOrEmpty(_selectedTargetUnitId))
        {
            _viewModel.AddBattleLog("대상을 먼저 선택해주세요");
            return;
        }

        bool isSuccess = BattleManager.Inst.SetActionResult(_selectedTargetUnitId, result, energyCost);

        if (!isSuccess)
        {
            _viewModel.AddBattleLog("실행 실패 (에너지 부족 또는 대상 없음)");
            return;
        }

        SetEnergyGauge(BattleManager.Inst.GetRemainingEnergy());
        _viewModel.AddBattleLog($"{_selectedTargetUnitId} 대상 - {logMessage}");

        _selectedTargetUnitId = null;
    }

    [ContextMenu("실제 액션 큐 빌드 테스트 (스폰된 유닛 기준)")]
    private void Test_BuildActionQueue()
    {
        if (BattleUnitTestSpawner.Inst == null)
        {
            Debug.LogWarning("[BattleMainUI] BattleUnitTestSpawner 인스턴스 없음");
            return;
        }

        List<string> heroIds = BattleUnitTestSpawner.Inst.GetHeroIdList();
        List<string> enemyIds = new List<string>();

        List<BattleUnitModel> turnOrder = _viewModel.GetBattleTurnOrder(heroIds, enemyIds);
        BattleManager.Inst.BuildActionQueue(turnOrder);
        _viewModel.RefreshActionQueue();
    }
}