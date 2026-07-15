using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

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
    private BattleActionResult? _pendingActionResult;
    private int _pendingEnergyCost;
    private string _pendingLogMessage;

    private BattleViewModel _viewModel;

    //테스트용 적 ID 목록
    [Header("BT 라운드 테스트")]
    [SerializeField] private List<string> _testEnemyIdList = new List<string>();

    [Header("전투 시작")]
    [SerializeField] private Button Button_StartBattle;
    private bool _isBattleRunning;

    private const int MaxRoundSafetyLimit = 15; //혹시 모를 무한루프 방지용 라운드 상한

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
        Button_StartBattle.onClick.AddListener(OnClickStartBattle);
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

        if (_pendingActionResult.HasValue)
        {
            ApplyInterventionAction(unitId, _pendingActionResult.Value, _pendingEnergyCost, _pendingLogMessage);
            return;
        }

        string heroName = GetHeroDisplayName(unitId);
        _viewModel.AddBattleLog($"{heroName} 유닛이 선택되었습니다.");
    }

    //유닛 ID로 표시용 영웅 이름을 가져온다. 데이터가 없으면 ID를 그대로 반환
    private string GetHeroDisplayName(string unitId)
    {
        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(unitId);

        if (heroData == null)
        {
            return unitId;
        }

        return heroData.HeroName;
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;

            Button_Reinforce.onClick.RemoveListener(OnClickReinforce);
            Button_HealUnit.onClick.RemoveListener(OnClickHealUnit);
            Button_ChangeUnit.onClick.RemoveListener(OnClickChangeUnit);
            Button_StartBattle.onClick.RemoveListener(OnClickStartBattle);
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
        RequestInterventionAction(BattleActionResult.Reinforce, ReinforceEnergyCost, "지원하기 실행");
    }

    private void OnClickHealUnit()
    {
        RequestInterventionAction(BattleActionResult.HealUnit, HealUnitEnergyCost, "영웅 회복 실행");
    }

    private void OnClickChangeUnit()
    {
        RequestInterventionAction(BattleActionResult.ChangeUnit, ChangeUnitEnergyCost, "영웅 교체 실행");
    }

    //개입 버튼을 눌렀을 때 대상이 이미 선택돼 있으면 바로 적용하고, 없으면 다음 유닛 클릭 때 적용하도록 대기시킨다
    private void RequestInterventionAction(BattleActionResult result, int energyCost, string logMessage)
    {
        if (string.IsNullOrEmpty(_selectedTargetUnitId))
        {
            _pendingActionResult = result;
            _pendingEnergyCost = energyCost;
            _pendingLogMessage = logMessage;

            _viewModel.AddBattleLog("대상을 선택하세요");
            return;
        }

        ApplyInterventionAction(_selectedTargetUnitId, result, energyCost, logMessage);
    }

    //실제 대상에게 개입 액션을 적용한다
    private void ApplyInterventionAction(string targetUnitId, BattleActionResult result, int energyCost, string logMessage)
    {
        ActionApplyResult applyResult = BattleManager.Inst.SetActionResult(targetUnitId, result, energyCost);

        if (applyResult == ActionApplyResult.InsufficientEnergy)
        {
            _viewModel.AddBattleLog("실행 실패: 에너지가 부족합니다.");
            return;
        }

        if (applyResult == ActionApplyResult.TargetNotFound)
        {
            _viewModel.AddBattleLog("실행 실패: 대상이 액션 큐에 없습니다.");
            return;
        }

        SetEnergyGauge(BattleManager.Inst.GetRemainingEnergy());

        string heroName = GetHeroDisplayName(targetUnitId);
        _viewModel.AddBattleLog($"{heroName} 대상 - {logMessage}");

        _selectedTargetUnitId = null;
        _pendingActionResult = null;
    }

    //[ContextMenu("실제 액션 큐 빌드 테스트 (스폰된 유닛 기준)")]
    //private void Test_BuildActionQueue()
    //{
    //    if (BattleUnitTestSpawner.Inst == null)
    //    {
    //        Debug.LogWarning("[BattleMainUI] BattleUnitTestSpawner 인스턴스 없음");
    //        return;
    //    }

    //    List<string> heroIds = BattleUnitTestSpawner.Inst.GetHeroIdList();
    //    List<string> enemyIds = new List<string>();

    //    List<BattleUnitModel> turnOrder = _viewModel.GetBattleTurnOrder(heroIds, enemyIds);
    //    BattleManager.Inst.BuildActionQueue(turnOrder);
    //    _viewModel.RefreshActionQueue();
    //}

    [ContextMenu("BT 라운드 액션 큐 통합 테스트")]
    private async void Test_RunRoundActionQueue()
    {
        if (_viewModel == null)
        {
            Debug.LogWarning("[BattleMainUI] BattleViewModel이 생성되지 않았습니다.");
            return;
        }

        if (BattleUnitTestSpawner.Inst == null)
        {
            Debug.LogWarning("[BattleMainUI] BattleUnitTestSpawner 인스턴스 없음");
            return;
        }

        List<string> heroIds = BattleUnitTestSpawner.Inst.GetHeroIdList();
        List<string> enemyIds = new List<string>(_testEnemyIdList);

        List<BattleUnitModel> turnOrder = _viewModel.GetBattleTurnOrder(heroIds, enemyIds);

        if (turnOrder == null || turnOrder.Count <= 0)
        {
            Debug.LogWarning("[BattleMainUI] 전투에 참여할 유닛이 없습니다.");
            return;
        }

        List<BattleUnitModel> heroList = new List<BattleUnitModel>();
        List<BattleUnitModel> enemyList = new List<BattleUnitModel>();

        foreach (BattleUnitModel unit in turnOrder)
        {
            if (unit.IsHero)
            {
                heroList.Add(unit);
            }
            else
            {
                enemyList.Add(unit);
            }
        }

        await _viewModel.RunRoundAsync(
            turnOrder,
            heroList,
            enemyList,
            this.GetCancellationTokenOnDestroy());
    }

    //전투 시작 버튼 - 스폰된 히어로와 테스트 적 목록으로 자동 진행 루프를 시작한다
    private void OnClickStartBattle()
    {
        if (_isBattleRunning)
        {
            Debug.LogWarning("[BattleMainUI] 이미 전투가 진행 중입니다.");
            return;
        }

        if (BattleUnitTestSpawner.Inst == null)
        {
            Debug.LogWarning("[BattleMainUI] BattleUnitTestSpawner 인스턴스 없음");
            return;
        }

        List<string> heroIds = BattleUnitTestSpawner.Inst.GetHeroIdList();
        List<string> enemyIds = new List<string>(_testEnemyIdList);

        List<BattleUnitModel> turnOrder = _viewModel.GetBattleTurnOrder(heroIds, enemyIds);

        if (turnOrder == null || turnOrder.Count <= 0)
        {
            Debug.LogWarning("[BattleMainUI] 전투에 참여할 유닛이 없습니다.");
            return;
        }

        List<BattleUnitModel> heroList = new List<BattleUnitModel>();
        List<BattleUnitModel> enemyList = new List<BattleUnitModel>();

        foreach (BattleUnitModel unit in turnOrder)
        {
            if (unit.IsHero)
            {
                heroList.Add(unit);
            }
            else
            {
                enemyList.Add(unit);
            }
        }

        _isBattleRunning = true;
        RunBattleLoopAsync(turnOrder, heroList, enemyList, this.GetCancellationTokenOnDestroy()).Forget();
    }

    //라운드를 자동 반복 진행하다가 승패가 나면 보상 지급 후 종료한다
    private async UniTaskVoid RunBattleLoopAsync(
        List<BattleUnitModel> turnOrder,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        CancellationToken token)
    {
        try
        {
            BattleManager.Inst.ResetBattleState();

            while (token.IsCancellationRequested == false)
            {
                await _viewModel.RunRoundAsync(turnOrder, heroList, enemyList, token);

                BattleResult result = BattleManager.Inst.CheckBattleResult(turnOrder);

                if (result != BattleResult.Ongoing)
                {
                    _viewModel.ApplyBattleReward(result, BattleManager.Inst.GetCurrentRound());
                    _viewModel.AddBattleLog(result == BattleResult.Victory ? "전투 승리!" : "전투 패배...");
                    return;
                }

                if (BattleManager.Inst.GetCurrentRound() >= MaxRoundSafetyLimit)
                {
                    _viewModel.AddBattleLog("전투 라운드 상한 도달, 강제 종료합니다.");
                    return;
                }
            }
        }
        finally
        {
            _isBattleRunning = false;
        }
    }
}