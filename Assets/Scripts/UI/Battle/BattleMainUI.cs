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

    [Header("배틀 로그 타자기 연출")]
    [SerializeField] private int _logTypingIntervalMs = 10; //글자 사이 간격(ms)

    private readonly Queue<string> _pendingLogQueue = new Queue<string>();
    private int _enqueuedLogCount;
    private bool _isLogTyping;
    private CancellationTokenSource _logTypingToken;

    [Header("에너지 게이지")]
    [SerializeField] private Image[] Image_EnergySlotList;

    private const float DimmedEnergyAlpha = 0.2f;

    [Header("액션 버튼")]
    [SerializeField] private Button Button_Reinforce;
    [SerializeField] private Button Button_HealUnit;
    [SerializeField] private Button Button_ChangeUnit;
    [SerializeField] private Button Button_EndTurn;
    [SerializeField] private TextMeshProUGUI Text_ReinforceEnergyCost;
    [SerializeField] private TextMeshProUGUI Text_HealUnitEnergyCost;
    [SerializeField] private TextMeshProUGUI Text_ChangeUnitEnergyCost;

    [Header("지원 아이템 팝업")]
    [SerializeField] private SupportItemPopupUI Panel_SupportItemPopup;

    [Header("전투 결과 팝업")]
    [SerializeField] private BattleResultPopupUI Panel_BattleResultPopup;

    [Header("영웅 교체 팝업")]
    [SerializeField] private ChangeUnitPopupUI Panel_ChangeUnitPopup;

    [Header("도움말")]
    [SerializeField] private Button Button_Help;
    [SerializeField] private HelpGuideUI Panel_HelpGuide;

    private const int ReinforceEnergyCost = 1; 
    private const int ChangeUnitEnergyCost = 2; 
    private const int HealUnitEnergyCost = 2; 

    private string _selectedTargetUnitId;
    private BattleActionResult? _pendingActionResult;
    private int _pendingEnergyCost;
    private string _pendingLogMessage;

    private BattleViewModel _viewModel;

    //테스트용 적 ID 목록
    [Header("BT 라운드 테스트")]
    [SerializeField] private List<string> _testEnemyIdList = new List<string>();

    private EnemySpawner _enemySpawner;
    private BattleVfxController _battleVfxController;
    private bool _isBattleRunning;
    private CancellationTokenSource _battleLoopCts;

    //도움말 패널이 닫힌 뒤 전투 루프에 넘겨줄 대기 데이터
    private List<BattleUnitModel> _pendingTurnOrder;
    private List<BattleUnitModel> _pendingHeroList;
    private List<BattleUnitModel> _pendingEnemyList;

    //이번 전투에 한 번이라도 출전한 영웅 목록. 교체 후보에서 제외한다
    private readonly List<string> _excludedHeroIdList = new List<string>();

    private const int MaxRoundSafetyLimit = 15; //혹시 모를 무한루프 방지용 라운드 상한

    private void Start()
    {
        _enemySpawner = FindFirstObjectByType<EnemySpawner>();
        _battleVfxController = FindFirstObjectByType<BattleVfxController>();

        _viewModel = new BattleViewModel();
        BindViewModel(_viewModel);
        BindBattleUnitSpawner();
        SetActionEnergyCostTexts();
        ResetBattleView();
        OpenRoster();
    }

    private void OnEnable()
    {
        // 첫 생성 시엔 Start보다 먼저 불리므로 건너뛴다
        if (_viewModel == null)
        {
            return;
        }

        ResetBattleView();
        OpenRoster();
    }

    //배틀 리셋
    private void ResetBattleView()
    {
        CancelBattleLoop();
        Panel_BattleResultPopup.ClosePopup();
        Panel_HelpGuide.CloseGuideSilently();
        Panel_ChangeUnitPopup.ClosePopup();
        _excludedHeroIdList.Clear();
        _isBattleRunning = false;
        _selectedTargetUnitId = null;
        _pendingActionResult = null;
        _pendingEnergyCost = 0;
        _pendingLogMessage = null;

        if (_enemySpawner != null)
        {
            _enemySpawner.ClearEnemies();
        }

        BattleManager.Inst.ResetBattleState();

        ClearLogSlots();
        _viewModel.ClearBattleLog();

        SetEnergyGauge(BattleManager.Inst.GetRemainingEnergy());
        _viewModel.RefreshActionQueue();
    }

    //배틀 로그 슬롯을 전부 지운다
    private void ClearLogSlots()
    {
        CancelLogTyping();

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
        _viewModel.UnitHpChanged += OnUnitHpChanged;
        _viewModel.UnitAttackStarted += OnUnitAttackStarted;
        _viewModel.UnitHit += OnUnitHit;
        _viewModel.UnitDied += OnUnitDied;
        _viewModel.UnitHitVfxRequested += OnUnitHitVfxRequested;
        _viewModel.UnitProjectileVfxRequested += OnUnitProjectileVfxRequested;
        _viewModel.UnitHealVfxRequested += OnUnitHealVfxRequested;
        _viewModel.HeroListChanged += HandleHeroListChanged;

        Button_Reinforce.onClick.AddListener(OnClickReinforce);
        Button_HealUnit.onClick.AddListener(OnClickHealUnit);
        Button_ChangeUnit.onClick.AddListener(OnClickChangeUnit);
        Button_EndTurn.onClick.AddListener(OnClickEndTurn);
        Button_Help.onClick.AddListener(OnClickHelp);

        Panel_SupportItemPopup.OnItemApplied += HandleSupportItemApplied;
        Panel_ChangeUnitPopup.OnHeroSelected += HandleChangeHeroSelected;
        Panel_BattleResultPopup.OnConfirmed += HandleBattleResultConfirmed;
    }

    private void BindBattleUnitSpawner()
    {
        if (BattleHeroSpawner.Inst == null)
        {
            return;
        }

        BattleHeroSpawner.Inst.OnUnitClicked += OnUnitClicked_Spawner;
    }

    private void OnUnitClicked_Spawner(string unitId)
    {
        _selectedTargetUnitId = unitId;

        string unitName = GameUtil.GetUnitDisplayName(unitId);
        _viewModel.AddBattleLog($"{unitName} 유닛이 선택되었습니다.");
    }

    private void OnUnitHpChanged(BattleUnitModel unit)
    {
        if (unit == null)
        {
            return;
        }
        if (unit.IsHero)
        {
            if (BattleHeroSpawner.Inst != null)
            {
                BattleHeroSpawner.Inst.RefreshHeroView(unit);
            }
            return;
        }
        if (_enemySpawner != null)
        {
            _enemySpawner.RefreshEnemyView(unit);
        }
    }

    //애니메이션 이벤트 처리 메서드
    private void OnUnitAttackStarted(BattleUnitModel unit)
    {
        if (unit == null)
        {
            return;
        }

        if (unit.IsHero)
        {
            if (BattleHeroSpawner.Inst != null)
            {
                BattleHeroSpawner.Inst.PlayAttackAnimation(unit);
            }

            return;
        }

        if (_enemySpawner != null)
        {
            _enemySpawner.PlayAttackAnimation(unit);
        }

        if (_battleVfxController != null)
        {
            _battleVfxController.PlayEnemyMuzzleVfxAsync(unit).Forget();
        }
    }


    private void OnUnitHit(BattleUnitModel unit)
    {
        if (unit == null)
        {
            return;
        }

        if (unit.IsHero)
        {
            if (BattleHeroSpawner.Inst != null)
            {
                BattleHeroSpawner.Inst.PlayHitAnimation(unit);
            }

            return;
        }

        if (_enemySpawner != null)
        {
            _enemySpawner.PlayHitAnimation(unit);
        }
    }

    private void OnUnitHitVfxRequested(BattleUnitModel unit)
    {
        if (_battleVfxController == null)
        {
            return;
        }

        _battleVfxController.PlayCommonHitVfxAsync(unit).Forget();
    }

    private void OnUnitProjectileVfxRequested(BattleActionModel action)
    {
        if (_battleVfxController == null || action == null)
        {
            return;
        }

        _battleVfxController.PlayProjectileVfxAsync(action).Forget();
    }

    private void OnUnitHealVfxRequested(BattleUnitModel unit)
    {
        if (_battleVfxController == null || unit == null)
        {
            return;
        }

        _battleVfxController.PlayHealVfxAsync(unit).Forget();
    }

    private void OnUnitDied(BattleUnitModel unit)
    {
        if (unit == null)
        {
            return;
        }

        if (unit.IsHero)
        {
            if (BattleHeroSpawner.Inst != null)
            {
                BattleHeroSpawner.Inst.PlayDeathAnimation(unit);
            }

            return;
        }

        if (_enemySpawner != null)
        {
            _enemySpawner.PlayDeathAnimation(unit);
        }
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;
            _viewModel.UnitHpChanged -= OnUnitHpChanged;
            _viewModel.UnitAttackStarted -= OnUnitAttackStarted;
            _viewModel.UnitHit -= OnUnitHit;
            _viewModel.UnitDied -= OnUnitDied;
            _viewModel.UnitHitVfxRequested -= OnUnitHitVfxRequested;
            _viewModel.UnitProjectileVfxRequested -= OnUnitProjectileVfxRequested;
            _viewModel.UnitHealVfxRequested -= OnUnitHealVfxRequested;
            _viewModel.HeroListChanged -= HandleHeroListChanged;

            Button_Reinforce.onClick.RemoveListener(OnClickReinforce);
            Button_HealUnit.onClick.RemoveListener(OnClickHealUnit);
            Button_ChangeUnit.onClick.RemoveListener(OnClickChangeUnit);
            Button_EndTurn.onClick.RemoveListener(OnClickEndTurn);
            Button_Help.onClick.RemoveListener(OnClickHelp);

            Panel_SupportItemPopup.OnItemApplied -= HandleSupportItemApplied;
            Panel_ChangeUnitPopup.OnHeroSelected -= HandleChangeHeroSelected;
            Panel_BattleResultPopup.OnConfirmed -= HandleBattleResultConfirmed;
        }

        if (BattleHeroSpawner.Inst != null)
        {
            BattleHeroSpawner.Inst.OnUnitClicked -= OnUnitClicked_Spawner;
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
        if (action == null)
        {
            return;
        }

        if (action.IsPlayerAction == true)
        {
            CreatePlayerActionQueueSlot();
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

    //플레이어 개입 슬롯은 유닛 데이터가 없으므로 전용 프리팹만 생성한다
    private void CreatePlayerActionQueueSlot()
    {
        GameObject loadedObj = (GameObject)Resources.Load("Prefabs/UI/BattleActionSlot_Player");
        Instantiate(loadedObj, Transform_ActionQueueContent);
    }

    // 새로 늘어난 로그를 대기 큐에 넣고, 타이핑 루프가 안 돌고 있으면 시작한다
    private void AppendNewLogSlots()
    {
        for (int i = _enqueuedLogCount; i < _viewModel.BattleLogs.Count; i++)
        {
            _pendingLogQueue.Enqueue(_viewModel.BattleLogs[i]);
        }

        _enqueuedLogCount = _viewModel.BattleLogs.Count;

        if (_isLogTyping == false)
        {
            PlayLogTypingAsync().Forget();
        }
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        ScrollRect_BattleLog.verticalNormalizedPosition = 0f;
    }

    // 대기 큐를 순서대로 꺼내 한 줄씩 타자기 연출로 출력한다 (동시 실행 방지)
    private async UniTask PlayLogTypingAsync()
    {
        _isLogTyping = true;
        _logTypingToken = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

        CancellationToken token = _logTypingToken.Token;

        while (_pendingLogQueue.Count > 0)
        {
            string line = _pendingLogQueue.Dequeue();
            await TypeSingleLogLineAsync(line, token);
        }

        _isLogTyping = false;
    }

    // 로그 슬롯 하나를 만들고 maxVisibleCharacters로 한 글자씩 드러낸다 (DialogueUI.Typing 방식)
    private async UniTask TypeSingleLogLineAsync(string line, CancellationToken token)
    {
        GameObject loadedObj = (GameObject)Resources.Load("Prefabs/UI/BattleLogSlot");
        GameObject slot = Instantiate(loadedObj, Transform_LogContent);

        TextMeshProUGUI logText = slot.GetComponent<TextMeshProUGUI>();

        if (logText == null)
        {
            return;
        }

        logText.text = line;
        logText.maxVisibleCharacters = 0;
        logText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(Transform_LogContent.GetComponent<RectTransform>());
        ScrollToBottom();

        if (_logTypingIntervalMs <= 0)
        {
            logText.maxVisibleCharacters = line.Length;
            return;
        }

        for (int i = 0; i <= line.Length; i++)
        {
            logText.maxVisibleCharacters = i;
            await UniTask.Delay(_logTypingIntervalMs, cancellationToken: token);
        }
    }

    // 진행 중인 로그 타이핑을 취소하고 대기 상태를 초기화한다 (DialogueUI.CancelTyping 방식)
    private void CancelLogTyping()
    {
        if (_logTypingToken != null)
        {
            _logTypingToken.Cancel();
            _logTypingToken.Dispose();
            _logTypingToken = null;
        }

        _pendingLogQueue.Clear();
        _enqueuedLogCount = 0;
        _isLogTyping = false;
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

    //각 개입 액션 버튼에 소모 에너지를 표시한다. 상수 값을 그대로 읽으므로 밸런싱으로 수치가 바뀌어도 화면과 어긋나지 않는다
    private void SetActionEnergyCostTexts()
    {
        SetEnergyCostText(Text_ReinforceEnergyCost, ReinforceEnergyCost);
        SetEnergyCostText(Text_HealUnitEnergyCost, HealUnitEnergyCost);
        SetEnergyCostText(Text_ChangeUnitEnergyCost, ChangeUnitEnergyCost);
    }

    private void SetEnergyCostText(TextMeshProUGUI targetText, int energyCost)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.text = $"에너지 {energyCost}";
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

    private void OnClickHelp()
    {
        Panel_HelpGuide.ToggleGuide();
    }

    //개입 버튼을 눌렀을 때 대상이 선택돼 있으면 그 대상으로 바로 진행한다. 대상이 없으면 안내만 하고 끝낸다
    private void RequestInterventionAction(BattleActionResult result, int energyCost, string logMessage)
    {
        if (string.IsNullOrEmpty(_selectedTargetUnitId))
        {
            _viewModel.AddBattleLog("액션을 적용할 대상을 선택하세요.");
            return;
        }

        _pendingActionResult = result;
        _pendingEnergyCost = energyCost;
        _pendingLogMessage = logMessage;

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

        if (result == BattleActionResult.ChangeUnit)
        {
            OpenChangeUnitPopup();
            return;
        }

        ApplyInterventionAction(targetUnitId, result, _pendingEnergyCost, _pendingLogMessage, null, null);
    }

    //교체 후보가 있을 때만 팝업을 연다. 후보가 없으면 에너지를 소모하지 않고 안내만 한다
    private void OpenChangeUnitPopup()
    {
        if (Panel_ChangeUnitPopup.HasWaitingHero(_excludedHeroIdList) == false)
        {
            _viewModel.AddBattleLog("실행 실패: 교체할 수 있는 대기 영웅이 없습니다.");
            return;
        }

        Panel_ChangeUnitPopup.OpenPopup(_excludedHeroIdList);
    }

    //교체 팝업에서 영웅을 고르면 실제 교체 액션을 지정한다
    private void HandleChangeHeroSelected(string heroId)
    {
        if (_pendingActionResult.HasValue == false || string.IsNullOrEmpty(_selectedTargetUnitId))
        {
            return;
        }

        ApplyInterventionAction(_selectedTargetUnitId, _pendingActionResult.Value, _pendingEnergyCost, _pendingLogMessage, null, heroId);
    }

    //교체가 반영되면 현재 영웅 목록으로 전부 다시 스폰한다. 스포너에 개별 교체 API가 없어 전체 재스폰으로 처리
    private void HandleHeroListChanged(List<BattleUnitModel> heroList)
    {
        if (heroList == null)
        {
            return;
        }

        List<string> heroIdList = new List<string>();

        foreach (BattleUnitModel hero in heroList)
        {
            heroIdList.Add(hero.ID);

            if (_excludedHeroIdList.Contains(hero.ID) == false)
            {
                _excludedHeroIdList.Add(hero.ID);
            }
        }

        BattleHeroSpawner.Inst.SetSelectedHeroIdList(heroIdList);
        BattleHeroSpawner.Inst.SpawnHeroes();
        BattleHeroSpawner.Inst.InitializeHeroViews(heroList);
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

        ApplyInterventionAction(_selectedTargetUnitId, _pendingActionResult.Value, _pendingEnergyCost, _pendingLogMessage, itemId, null);
    }

    //결과 팝업 확인 버튼 클릭 시 타이쿤 화면으로 복귀한다
    private void HandleBattleResultConfirmed()
    {
        ObjectManager.Inst.ExitBattle();
    }

    //실제 대상에게 개입 액션을 적용한다
    private void ApplyInterventionAction(string targetUnitId, BattleActionResult result, int energyCost, string logMessage, string itemId, string changeHeroId)
    {
        ActionApplyResult applyResult = BattleManager.Inst.SetActionResult(targetUnitId, result, energyCost, itemId, changeHeroId);

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
        string appliedLogMessage = BuildInterventionAppliedLogMessage(logMessage, itemId);
        _viewModel.AddBattleLog($"{unitName} 대상 - {appliedLogMessage}");

        _selectedTargetUnitId = null;
        _pendingActionResult = null;
    }

    //개입 적용 시점 로그 문구를 만든다. 아이템을 사용한 경우 아이템 이름을 덧붙인다
    private string BuildInterventionAppliedLogMessage(string logMessage, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return logMessage;
        }

        SupportItem item = GameDataManager.Inst.GetData<SupportItem>(itemId);

        if (item == null)
        {
            return logMessage;
        }

        return $"{logMessage}, '{item.ItemName}' 아이템 사용 예정";
    }

    //[ContextMenu("실제 액션 큐 빌드 테스트 (스폰된 유닛 기준)")]
    //private void Test_BuildActionQueue()
    //{
    //    if (BattleHeroSpawner.Inst == null)
    //    {
    //        Debug.LogWarning("[BattleMainUI] BattleHeroSpawner 인스턴스 없음");
    //        return;
    //    }

    //    List<string> heroIds = BattleHeroSpawner.Inst.GetHeroIdList();
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

        if (BattleHeroSpawner.Inst == null)
        {
            Debug.LogWarning("[BattleMainUI] BattleHeroSpawner 인스턴스 없음");
            return;
        }

        List<string> heroIds = BattleHeroSpawner.Inst.GetHeroIdList();
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

    ////전투 시작 버튼 
    //private void OnClickStartBattle()
    //{
    //    if (_isBattleRunning)
    //    {
    //        Debug.LogWarning("[BattleMainUI] 이미 전투가 진행 중입니다.");
    //        return;
    //    }
    //    UIBase ui = UIManager.Inst.OpenRosterUI();
    //    RosterUI rosterUI = ui as RosterUI;
    //    if (rosterUI == null)
    //    {
    //        Debug.LogWarning("[BattleMainUI] RosterUI를 찾을 수 없습니다.");
    //        return;
    //    }
    //    rosterUI.Initialize(OnRosterConfirmed);
    //}

    //로스터에서 3명을 확정하면 그 영웅들을 스폰시키고 전투 루프를 시작한다
    private void OnRosterConfirmed(List<string> selectedHeroIds)
    {
        if (selectedHeroIds == null || selectedHeroIds.Count <= 0)
        {
            Debug.LogWarning("[BattleMainUI] 선택된 영웅이 없습니다.");
            return;
        }
        //SetSelectedHeroIdList는 저장만 하므로 SpawnHeroes로 실제 스폰까지 트리거
        BattleHeroSpawner.Inst.SetSelectedHeroIdList(selectedHeroIds);
        BattleHeroSpawner.Inst.SpawnHeroes();
        //실제 스폰된 유닛 기준으로 턴 순서 구성 (프리팹 매핑 없는 영웅은 스폰에서 빠지므로 로직도 그에 맞춤)
        List<string> spawnedHeroIds = BattleHeroSpawner.Inst.GetHeroIdList();
        _excludedHeroIdList.Clear();
        _excludedHeroIdList.AddRange(spawnedHeroIds);
        List<string> enemyIds = new List<string>(_testEnemyIdList);
        List<BattleUnitModel> turnOrder = _viewModel.GetBattleTurnOrder(spawnedHeroIds, enemyIds);
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

        if (_enemySpawner == null)
        {
            Debug.LogWarning("[BattleMainUI] EnemySpawner를 찾을 수 없습니다.");
            return;
        }

        _enemySpawner.SpawnEnemies(enemyList);
        BattleHeroSpawner.Inst.InitializeHeroViews(heroList);   //영웅 HP바 초기값 세팅


        //도움말을 먼저 띄우고, 닫히는 시점에 전투 루프를 시작한다
        _pendingTurnOrder = turnOrder;
        _pendingHeroList = heroList;
        _pendingEnemyList = enemyList;
        Panel_HelpGuide.ShowGuideOnBattleStart(StartBattleLoopAfterGuide);
    }

    //도움말이 닫힌 뒤 실제 전투 루프를 시작한다
    private void StartBattleLoopAfterGuide()
    {
        if (_pendingTurnOrder == null)
        {
            return;
        }

        CancelBattleLoop();
        _battleLoopCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _isBattleRunning = true;
        RunBattleLoopAsync(_pendingTurnOrder, _pendingHeroList, _pendingEnemyList, _battleLoopCts.Token).Forget();

        _pendingTurnOrder = null;
        _pendingHeroList = null;
        _pendingEnemyList = null;
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
                    int roundCount = BattleManager.Inst.GetCurrentRound();
                    int rewardAmount = _viewModel.ApplyBattleReward(result, roundCount);
                    _viewModel.UpdateHeroBattleParticipation(_excludedHeroIdList);
                    GameManager.Inst.Services.DayService.MarkBattleDone();
                    _viewModel.AddBattleLog(result == BattleResult.Victory ? "전투 승리!" : "전투 패배...");
                    Panel_BattleResultPopup.OpenPopup(result, rewardAmount, roundCount);

                    QuestViewModel questVM = GameManager.Inst.Services.QuestService?.GetQuestViewModel();

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





    ////배틀메인UI 나가기

    //private void OnClickExit()
    //{
    //    CancelBattleLoop();
    //    ObjectManager.Inst.ExitBattle();
    //}

    //배틀메인UI 진입시 로스터 자동 열기
    private void OpenRoster()
    {
        if (_isBattleRunning)
        {
            return;
        }
        UIBase ui = UIManager.Inst.OpenRosterUI();
        RosterUI rosterUI = ui as RosterUI;
        if (rosterUI == null)
        {
            Debug.LogWarning("[BattleMainUI] RosterUI를 찾을 수 없습니다.");
            return;
        }
        rosterUI.Initialize(OnRosterConfirmed);
    }
}