using System.ComponentModel;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WeeklyEvaluationView : ViewBase
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI Text_Range;
    [SerializeField] private TextMeshProUGUI Text_OverallGrade;
    [Header("일자 슬롯")]
    [SerializeField] private WeeklyDaySlot Prefab_DaySlot;
    [SerializeField] private Transform Transform_DaySlotParent;
    [Header("버튼")]
    [SerializeField] private Button Button_Close;
    private WeeklyEvaluationViewModel _viewModel;
    private List<WeeklyDaySlot> _daySlots = new List<WeeklyDaySlot>();
    public void Bind(WeeklyEvaluationViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.BuildWeekly();
        UpdateAllText();
        RefreshDaySlots();
    }
    private void OnEnable()
    {
        Button_Close.onClick.RemoveListener(OnClickClose);
        Button_Close.onClick.AddListener(OnClickClose);
        WeeklyEvaluationViewModel vm = GameManager.Inst.Services.WeeklyEvaluationService.GetWeeklyEvaluationViewModel();
        if (vm == null)
        {
            vm = GameManager.Inst.Services.WeeklyEvaluationService.CreateWeeklyEvaluationViewModel();
        }
        Bind(vm);
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
        if (Text_Range != null)
        {
            Text_Range.text = _viewModel.RangeText;
        }
        if (Text_OverallGrade != null)
        {
            Text_OverallGrade.text = _viewModel.OverallGradeText;
        }
    }
    // 이번 주 일자 슬롯 생성
    private void RefreshDaySlots()
    {
        if (_viewModel == null || Prefab_DaySlot == null || Transform_DaySlotParent == null)
        {
            return;
        }
        for (int i = 0; i < _daySlots.Count; i++)
        {
            if (_daySlots[i] != null)
            {
                _daySlots[i].OnClickDetail -= OnClickDaySlotDetail;
                Destroy(_daySlots[i].gameObject);
            }
        }
        _daySlots.Clear();
        List<DailyEvaluationRecord> records = _viewModel.WeekRecords;
        for (int i = 0; i < records.Count; i++)
        {
            WeeklyDaySlot slot = Instantiate(Prefab_DaySlot, Transform_DaySlotParent);
            slot.SetSlot(records[i]);
            slot.OnClickDetail += OnClickDaySlotDetail;
            _daySlots.Add(slot);
        }
    }
    // 상세보기 클릭 → 그날 일일평가 스냅샷 열기
    private void OnClickDaySlotDetail(DailyEvaluationRecord record)
    {
        UIManager.Inst.OpenSettlementSnapshot(record);
    }
    private void OnClickClose()
    {
        UIManager.Inst.CloseWeeklyEvaluationUI();
    }
}