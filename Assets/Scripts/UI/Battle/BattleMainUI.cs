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
    [SerializeField] private TextMeshProUGUI Text_Round;

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
    [SerializeField] private Button Button_EndTurn;

    [Header("지원 아이템 팝업")]
    [SerializeField] private SupportItemPopupUI Panel_SupportItemPopup;

    [Header("나가기")]
    [SerializeField] private Button Button_Exit;

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
    private CancellationTokenSource _battleLoopCts;


    private const int MaxRoundSafetyLimit = 15; //혹시 모를 무한루프 방지용 라운드 상한

    private void Start()
    {
        _viewModel = new BattleViewModel();
        BindViewModel(_viewModel);
        BindBattleUnitSpawner();
        ResetBattleView();
    }

    private void OnEnable()
    {
        // 첫 생성 시엔 Start보다 먼저 불리므로 건너뛴다
        if (_viewModel == null)
        {
            return;
        }

        ResetBattleView();
    }

    //배틀 리셋
    private void ResetBattleView()
    {
        CancelBattleLoop();
        _isBattleRunning = false;
        _selectedTargetUnitId = null;
        _pendingActionResult = null;
        _pendingEnergyCost = 0;
        _pendingLogMessage = null;

        BattleManager.Inst.ResetBattleState();

        ClearLogSlots();
        _viewModel.ClearBattleLog();

        SetEnergyGauge(BattleManager.Inst.GetRemainingEnergy());
        _viewModel.RefreshActionQueue();
    }

    //배틀 로그 슬롯을 전부 지운다
    private void ClearLogSlots()
    {
        for (int i = Transform_LogContent.childCount - 1; i >= 0; i--)
        {
            Destroy(Transform_LogContent.GetChild(i).gameObject);
        }
    }

    //이전 전투 루프가 아직 살아있으면 취소한다 (재진입 시 중복 루프 방지)
    private void CancelBattleLoop()
    {
        if (_battleLoopCts == null)
        {
            return;
        }
        _battleLoopCts.Cancel();
        _battleLoopCts.Dispose();
        _battleLoopCts = null;
    }

    private void BindViewModel(BattleViewModel viewModel)
    {
        _viewModel.PropertyChanged += OnPropertyChanged_View;

        Button_Reinforce.onClick.AddListener(OnClickReinforce);
        Button_HealUnit.onClick.AddListener(OnClickHealUnit);
        Button_ChangeUnit.onClick.AddListener(OnClickChangeUnit);
        Button_EndTurn.onClick.AddListener(OnClickEndTurn);
        Button_StartBattle.onClick.AddListener(OnClickStartBattle);
        Button_Exit.onClick.AddListener(OnClickExit);

        Panel_SupportItemPopup.OnItemApplied += HandleSupportItemApplied;
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
            ResolvePendingAction(unitId, _pendingActionResult.Value);
            return;
        }

        string unitName = GameUtil.GetUnitDisplayName(unitId);
        _viewModel.AddBattleLog($"{unitName} 유닛이 선택되었습니다.");
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;

            Button_Reinforce.onClick.RemoveListener(OnClickReinforce);
            Button_HealUnit.onClick.RemoveListener(OnClickHealUnit);
            Button_ChangeUnit.onClick.RemoveListener(OnClickChangeUnit);
            Button_EndTurn.onClick.RemoveListener(OnClickEndTurn);
            Button_StartBattle.onClick.RemoveListener(OnClickStartBattle);
            Button_Exit.onClick.RemoveListener(OnClickExit);

            Panel_SupportItemPopup.OnItemApplied -= HandleSupportItemApplied;
        }

        if (BattleUnitTestSpawner.Inst != null)
        {
            BattleUnitTestSpawner.Inst.OnUnitClicked -= OnUnitClicked_Spawner;
        }
        CancelBattleLoop();
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
                UpdateRoundText();
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

    //현재 라운드 수를 화면에 표시한다
    private void UpdateRoundText()
    {
        Text_Round.text = $"라운드 {BattleManager.Inst.GetCurrentRound()}";
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
        //플레이어 개입 액션은 전용 UI 처리 전까지 일반 유닛 슬롯에서 제외
        if (action == null || action.IsPlayerAction)
        {
            return;
        }
        
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

    //개입 턴을 마치고 다음 단계(큐 실행)로 넘어가겠다는 신호를 ViewModel에 전달
    private void OnClickEndTurn()
    {
        _viewModel.NotifyInterventionEnded();
    }

    //개입 버튼을 눌렀을 때 대상이 이미 선택돼 있으면 바로 진행하고, 없으면 다음 유닛 클릭 때 진행하도록 대기시킨다
    private void RequestInterventionAction(BattleActionResult result, int energyCost, string logMessage)
    {
        _pendingActionResult = result;
        _pendingEnergyCost = energyCost;
        _pendingLogMessage = logMessage;

        if (string.IsNullOrEmpty(_selectedTargetUnitId))
        {
            _viewModel.AddBattleLog("대상을 선택하세요.");
            return;
        }

        ResolvePendingAction(_selectedTargetUnitId, result);
    }

    //보류 중이던 개입 액션을 실제로 진행한다. 아이템이 필요한 개입(지원하기/회복하기)은 팝업을 띄우고, 그 외는 바로 적용한다
    private void ResolvePendingAction(string targetUnitId, BattleActionResult result)
    {
        if (result == BattleActionResult.Reinforce || result == BattleActionResult.HealUnit)
        {
            OpenItemPopup(targetUnitId, result);
            return;
        }

        ApplyInterventionAction(targetUnitId, result, _pendingEnergyCost, _pendingLogMessage, null);
    }

    //개입 종류에 맞는 지원 아이템 팝업을 연다
    private void OpenItemPopup(string targetUnitId, BattleActionResult result)
    {
        if (result == BattleActionResult.Reinforce)
        {
            BattleUnitModel unit = BattleManager.Inst.GetQueuedUnit(targetUnitId);

            if (unit == null)
            {
                _viewModel.AddBattleLog("실행 실패: 대상 유닛 정보를 찾을 수 없습니다.");
                return;
            }

            Panel_SupportItemPopup.OpenPopupForPenalty(unit);
            return;
        }

        Panel_SupportItemPopup.OpenPopupForHeal();
    }

    //팝업에서 아이템을 선택했을 때 실제 개입 액션을 적용한다
    private void HandleSupportItemApplied(string itemId)
    {
        if (_pendingActionResult.HasValue == false || string.IsNullOrEmpty(_selectedTargetUnitId))
        {
            return;
        }

        ApplyInterventionAction(_selectedTargetUnitId, _pendingActionResult.Value, _pendingEnergyCost, _pendingLogMessage, itemId);
    }

    //실제 대상에게 개입 액션을 적용한다
    private void ApplyInterventionAction(string targetUnitId, BattleActionResult result, int energyCost, string logMessage, string itemId)
    {
        ActionApplyResult applyResult = BattleManager.Inst.SetActionResult(targetUnitId, result, energyCost, itemId);

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

        if (applyResult == ActionApplyResult.AlreadyApplied)
        {
            _viewModel.AddBattleLog("실행 실패: 이미 이번 라운드에 행동을 지정한 유닛입니다.");
            return;
        }

        if (applyResult == ActionApplyResult.NoActivePenalty)
        {
            _viewModel.AddBattleLog("실행 실패: 페널티가 없어 지원하기를 쓸 수 없습니다.");
            return;
        }

        SetEnergyGauge(BattleManager.Inst.GetRemainingEnergy());

        string unitName = GameUtil.GetUnitDisplayName(targetUnitId);
        _viewModel.AddBattleLog($"{unitName} 대상 - {logMessage}");

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

        CancelBattleLoop();
        _battleLoopCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _isBattleRunning = true;
        RunBattleLoopAsync(turnOrder, heroList, enemyList, _battleLoopCts.Token).Forget();
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
                    GameManager.Inst.Services.DayService.MarkBattleDone();
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





    //배틀메인UI 나가기

    private void OnClickExit()
    {
        CancelBattleLoop();
        ObjectManager.Inst.ExitBattle();
    }
}