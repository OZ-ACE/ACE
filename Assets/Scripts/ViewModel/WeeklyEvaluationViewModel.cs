using System.Collections.Generic;
// 주간 평가 뷰모델 (일일 기록 7일치 평균 + 표 데이터)
public class WeeklyEvaluationViewModel : ViewModelBase
{
    private readonly DayService _dayService;
    private int _weekStartDay;
    private int _weekEndDay;
    private EvaluationGrade _overall;
    private EvaluationGrade _heroManage;
    private EvaluationGrade _gold;
    private EvaluationGrade _fragment;
    private List<DailyEvaluationRecord> _weekRecords = new List<DailyEvaluationRecord>();
    public WeeklyEvaluationViewModel(DayService dayService)
    {
        _dayService = dayService;
    }
    public List<DailyEvaluationRecord> WeekRecords { get { return _weekRecords; } }
    public string RangeText { get { return $"Day {_weekStartDay} ~ {_weekEndDay}"; } }
    public string OverallGradeText { get { return GradeCalculator.GetText(_overall); } }
    public string HeroManageGradeText { get { return GradeCalculator.GetText(_heroManage); } }
    public string GoldGradeText { get { return GradeCalculator.GetText(_gold); } }
    public string FragmentGradeText { get { return GradeCalculator.GetText(_fragment); } }
    // 이번 주 평균 계산 (마감 직후 열리므로 마감일 = 현재일 - 1)
    public void BuildWeekly()
    {
        _weekEndDay = _dayService.CurrentDay - 1;
        _weekStartDay = _weekEndDay - 6;
        if (_weekStartDay < 1)
        {
            _weekStartDay = 1;
        }
        _weekRecords.Clear();
        List<EvaluationGrade> overallList = new List<EvaluationGrade>();
        List<EvaluationGrade> heroList = new List<EvaluationGrade>();
        List<EvaluationGrade> goldList = new List<EvaluationGrade>();
        List<EvaluationGrade> fragmentList = new List<EvaluationGrade>();
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player != null)
        {
            for (int i = 0; i < player.DailyEvaluations.Count; i++)
            {
                DailyEvaluationRecord record = player.DailyEvaluations[i];
                if (record.Day < _weekStartDay || record.Day > _weekEndDay)
                {
                    continue;
                }
                _weekRecords.Add(record);
                overallList.Add((EvaluationGrade)record.OverallGrade);
                heroList.Add((EvaluationGrade)record.HeroManageGrade);
                goldList.Add((EvaluationGrade)record.GoldGrade);
                fragmentList.Add((EvaluationGrade)record.FragmentGrade);
            }
        }
        _overall = GradeCalculator.GetAverage(overallList);
        _heroManage = GradeCalculator.GetAverage(heroList);
        _gold = GradeCalculator.GetAverage(goldList);
        _fragment = GradeCalculator.GetAverage(fragmentList);
        OnPropertyChanged(nameof(RangeText));
        OnPropertyChanged(nameof(OverallGradeText));
        OnPropertyChanged(nameof(HeroManageGradeText));
        OnPropertyChanged(nameof(GoldGradeText));
        OnPropertyChanged(nameof(FragmentGradeText));
    }
}