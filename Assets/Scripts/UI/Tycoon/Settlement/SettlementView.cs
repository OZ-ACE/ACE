using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    // 뷰모델 바인딩
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
            _viewModel.Evaluate(Config_Evaluation);
            RefreshHeroSlots();
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
        GameOverType gameOverType = _viewModel.CheckResult();

        if (_viewModel.TryConfirmSettlement() == false)
        {
            Debug.Log("[SettlementView] 정산 실패 - 다음날로 넘길 수 없음");
            return;
        }

        switch (gameOverType)
        {
            case GameOverType.GameOver:
                GameManager.Inst.SetDialogueID("GameOver_2_01");
                ObjectManager.Inst.ExitOffice();
                UIManager.Inst.CloseTycoonMainUI();
                UIManager.Inst.OpenDialogueUI();
                break;

            case GameOverType.Warning:
                GameManager.Inst.SetDialogueID("GameOver_1_01");
                ObjectManager.Inst.ExitOffice();
                UIManager.Inst.CloseTycoonMainUI();
                UIManager.Inst.OpenDialogueUI();
                break;

            case GameOverType.None:
                break;
        }

        UIManager.Inst.CloseSettlementUI();
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
        // 기존 슬롯 정리
        for (int i = 0; i < _heroSlots.Count; i++)
        {
            if (_heroSlots[i] != null)
            {
                Destroy(_heroSlots[i].gameObject);
            }
        }
        _heroSlots.Clear();
        // 영웅별 슬롯 생성 + 바인딩
        List<HeroEvaluation> heroes = _viewModel.HeroEvaluations;
        for (int i = 0; i < heroes.Count; i++)
        {
            SettlementHeroSlot slot = Instantiate(Prefab_HeroSlot, Transform_HeroSlotParent);
            slot.SetSlot(heroes[i]);
            _heroSlots.Add(slot);
        }
    }
}