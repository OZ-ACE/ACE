using System.ComponentModel;
using System.Collections.Generic;
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
    [Header("업무평가")]
    [SerializeField] private WorkEvaluationConfig Config_Evaluation;
    [SerializeField] private TextMeshProUGUI Text_OverallGrade;
    [SerializeField] private TextMeshProUGUI Text_HeroManageGrade;
    [SerializeField] private TextMeshProUGUI Text_GoldGrade;
    [SerializeField] private TextMeshProUGUI Text_FragmentGrade;
    [Header("영웅별 평가 슬롯")]
    [SerializeField] private SettlementHeroSlot Prefab_HeroSlot;
    [SerializeField] private Transform Transform_HeroSlotParent;
    private List<SettlementHeroSlot> _heroSlots = new List<SettlementHeroSlot>();
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
        _viewModel.Evaluate(Config_Evaluation);
        RefreshHeroSlots();
    }
    private void OnEnable()
    {
        Button_Confirm.onClick.RemoveListener(OnClickConfirm);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
        Button_Close.onClick.RemoveListener(OnClickClose);
        Button_Close.onClick.AddListener(OnClickClose);
        // 항상 오늘(라이브) 정산으로 초기화
        BindLive();
    }
    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
    // 오늘 정산(라이브) 바인딩
    private void BindLive()
    {
        SettlementViewModel vm = GameManager.Inst.Services.SettlementService.GetSettlementViewModel();
        if (vm == null)
        {
            vm = GameManager.Inst.Services.SettlementService.CreateSettlementViewModel();
        }
        Bind(vm);
        if (Button_Confirm != null)
        {
            Button_Confirm.gameObject.SetActive(true);
        }
    }
    // 과거 일일평가 스냅샷 표시 (주간창 상세보기)
    public void ShowSnapshot(DailyEvaluationRecord record)
    {
        SettlementViewModel vm = GameManager.Inst.Services.SettlementService.CreateSnapshotViewModel();
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        _viewModel = vm;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.LoadFromRecord(record);
        UpdateAllText();
        RefreshHeroSlots();
        if (Button_Confirm != null)
        {
            Button_Confirm.gameObject.SetActive(false);
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
        if (Text_OverallGrade != null)
        {
            Text_OverallGrade.text = _viewModel.OverallGradeText;
        }
        if (Text_HeroManageGrade != null)
        {
            Text_HeroManageGrade.text = _viewModel.HeroManageGradeText;
        }
        if (Text_GoldGrade != null)
        {
            Text_GoldGrade.text = _viewModel.GoldGradeText;
        }
        if (Text_FragmentGrade != null)
        {
            Text_FragmentGrade.text = _viewModel.FragmentGradeText;
        }
    }
    private void OnClickConfirm()
    {
        if (_viewModel.TryConfirmSettlement() == false)
        {
            Debug.Log("[SettlementView] 정산 실패 - 다음날로 넘길 수 없음");
            return;
        }
        // 일일 종합 등급으로 게임오버 판정 (D 이하 2일 연속)
        GameOverType result = _viewModel.CheckDailyResult();
        bool weeklyDue = _viewModel.IsWeeklyReviewDue;
        UIManager.Inst.CloseSettlementUI();
        if (result == GameOverType.GameOver)
        {
            UIManager.Inst.CloseWeeklyEvaluationUI();
            UIManager.Inst.CloseTycoonMainUI();
            UIManager.Inst.OpenGameOver(EndingType.GameOver);
            return;
        }
        if (weeklyDue == true)
        {
            WeeklyEvaluationViewModel weeklyVm = GameManager.Inst.Services.WeeklyEvaluationService.GetWeeklyEvaluationViewModel();
            if (weeklyVm == null)
            {
                weeklyVm = GameManager.Inst.Services.WeeklyEvaluationService.CreateWeeklyEvaluationViewModel();
            }
            weeklyVm.BuildWeekly();
            UIManager.Inst.OpenWeeklyEvaluationUI();
        }
    }
    private void OnClickClose()
    {
        UIManager.Inst.CloseSettlementUI();
    }
    // 영웅 수만큼 슬롯을 복제해 채운다
    private void RefreshHeroSlots()
    {
        if (_viewModel == null || Prefab_HeroSlot == null || Transform_HeroSlotParent == null)
        {
            return;
        }
        for (int i = 0; i < _heroSlots.Count; i++)
        {
            if (_heroSlots[i] != null)
            {
                Destroy(_heroSlots[i].gameObject);
            }
        }
        _heroSlots.Clear();
        List<HeroEvaluation> heroes = _viewModel.HeroEvaluations;
        for (int i = 0; i < heroes.Count; i++)
        {
            SettlementHeroSlot slot = Instantiate(Prefab_HeroSlot, Transform_HeroSlotParent);
            slot.SetSlot(heroes[i]);
            _heroSlots.Add(slot);
        }
    }


    //평가 D 이하 처리
    private void HandleGameOverResult(GameOverType result)
    {
        if (result == GameOverType.Warning)
        {
            UIManager.Inst.OpenNoticePopup("경고! 이번 주 평가가 D 이하입니다. 다음 주에도 D 이하면 게임오버됩니다.");
        }
        else if (result == GameOverType.GameOver)
        {
            UIManager.Inst.CloseWeeklyEvaluationUI();
            UIManager.Inst.CloseTycoonMainUI();
            UIManager.Inst.OpenGameOver(EndingType.GameOver);
        }
    }
}