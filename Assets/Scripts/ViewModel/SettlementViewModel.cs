using UnityEngine;
using System.Collections.Generic;

public enum GameOverType
{
    None,
    Warning,
    GameOver
}

//마감정산 뷰 모델
public class SettlementViewModel : ViewModelBase
{
    private readonly ICurrencyService _currencyService;
    private readonly DayService _dayService;
    // 업무평가 결과
    private WorkEvaluationResult _evaluationResult;
    // 이번 마감이 주간평가(7일차) 시점인지
    private bool _isWeeklyReviewDue;
    // 스냅샷(과거 일일평가 재열람) 상태
    private bool _isSnapshot;
    private int _snapshotDay;
    private int _snapshotGold;
    private int _snapshotTodayFragment;
    private int _snapshotTotalFragment;
    public SettlementViewModel(ICurrencyService currencyService, DayService dayService)
    {
        _currencyService = currencyService;
        _dayService = dayService;
    }
    public bool IsWeeklyReviewDue { get { return _isWeeklyReviewDue; } }
    public bool IsSnapshot { get { return _isSnapshot; } }
    public int CurrentDay { get { if (_isSnapshot == true) { return _snapshotDay; } return _dayService.CurrentDay; } }
    public int TodayMemoryFragment { get { if (_isSnapshot == true) { return _snapshotTodayFragment; } return _currencyService.CurrentTodayMemoryFragment; } }
    public int CurrentGold { get { if (_isSnapshot == true) { return _snapshotGold; } return _currencyService.CurrentGold; } }
    public int CurrentMemoryFragment { get { if (_isSnapshot == true) { return _snapshotTotalFragment; } return _currencyService.CurrentMemoryFragment; } }
    // 업무평가 등급 텍스트
    public string OverallGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.Overall : EvaluationGrade.F); } }
    public string HeroManageGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.HeroManageGrade : EvaluationGrade.F); } }
    public string GoldGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.GoldGrade : EvaluationGrade.F); } }
    public string FragmentGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.FragmentGrade : EvaluationGrade.F); } }
    // 영웅별 평가 목록 (뷰가 슬롯 만들 때 사용)
    public List<HeroEvaluation> HeroEvaluations
    {
        get
        {
            if (_evaluationResult == null)
            {
                return new List<HeroEvaluation>();
            }
            return _evaluationResult.Heroes;
        }
    }

    // 뷰가 켜질때 화면 갱신
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(TodayMemoryFragment));
        OnPropertyChanged(nameof(CurrentMemoryFragment));
        OnPropertyChanged(nameof(CurrentGold));
    }
    // 등급 문자 변환 헬퍼
    private string GetGradeText(EvaluationGrade grade)
    {
        return GradeCalculator.GetText(grade);
    }

    // 업무평가 실행 (뷰가 config 넘겨 호출)
    public void Evaluate(WorkEvaluationConfig config)
    {
        _isSnapshot = false;
        if (config == null)
        {
            Debug.LogWarning("[SettlementViewModel] WorkEvaluationConfig 없음 - 평가 스킵");
            return;
        }

        WorkEvaluationResult result = new WorkEvaluationResult();

        // 돈 / 파편
        result.GoldGrade = config.Gold.GetGrade(_currencyService.CurrentGold);
        result.FragmentGrade = config.Fragment.GetGrade(_currencyService.CurrentMemoryFragment);

        // 영웅관리 (호감도 + 만족도 전체 평균)
        result.Heroes = BuildHeroEvaluations(config);
        List<EvaluationGrade> heroGrades = new List<EvaluationGrade>();

        for (int i = 0; i < result.Heroes.Count; i++)
        {
            heroGrades.Add(result.Heroes[i].Affection);
            heroGrades.Add(result.Heroes[i].Satisfaction);
        }

        result.HeroManageGrade = GradeCalculator.GetAverage(heroGrades);

        // 종합 (영웅관리 / 돈 / 파편 평균)
        List<EvaluationGrade> categoryGrades = new List<EvaluationGrade>();
        categoryGrades.Add(result.HeroManageGrade);
        categoryGrades.Add(result.GoldGrade);
        categoryGrades.Add(result.FragmentGrade);
        result.Overall = GradeCalculator.GetAverage(categoryGrades);
        _evaluationResult = result;
        OnPropertyChanged(nameof(OverallGradeText));
        OnPropertyChanged(nameof(HeroManageGradeText));
        OnPropertyChanged(nameof(GoldGradeText));
        OnPropertyChanged(nameof(FragmentGradeText));
    }
    // 영웅별 호감도/만족도 등급 산출
    private List<HeroEvaluation> BuildHeroEvaluations(WorkEvaluationConfig config)
    {
        List<HeroEvaluation> list = new List<HeroEvaluation>();
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null || player.HeroStats == null)
        {
            return list;
        }

        for (int i = 0; i < player.HeroStats.Count; i++)
        {
            HeroStat stat = player.HeroStats[i];

            if (stat == null)
            {
                continue;
            }

            HeroEvaluation eval = new HeroEvaluation();
            eval.HeroId = stat.HeroID;
            eval.Affection = config.Affection.GetGrade(stat.Affection);
            eval.Satisfaction = config.Satisfaction.GetGrade(stat.Satisfaction);
            list.Add(eval);
        }

        return list;
    }
    // 저장된 하루치 스냅샷을 그대로 표시
    public void LoadFromRecord(DailyEvaluationRecord record)
    {
        if (record == null)
        {
            return;
        }
        _isSnapshot = true;
        _snapshotDay = record.Day;
        _snapshotGold = record.Gold;
        _snapshotTodayFragment = record.TodayFragment;
        _snapshotTotalFragment = record.TotalFragment;
        WorkEvaluationResult result = new WorkEvaluationResult();
        result.Overall = (EvaluationGrade)record.OverallGrade;
        result.HeroManageGrade = (EvaluationGrade)record.HeroManageGrade;
        result.GoldGrade = (EvaluationGrade)record.GoldGrade;
        result.FragmentGrade = (EvaluationGrade)record.FragmentGrade;
        for (int i = 0; i < record.Heroes.Count; i++)
        {
            HeroDailyEvaluation h = record.Heroes[i];
            HeroEvaluation eval = new HeroEvaluation();
            eval.HeroId = h.HeroId;
            eval.Affection = (EvaluationGrade)h.AffectionGrade;
            eval.Satisfaction = (EvaluationGrade)h.SatisfactionGrade;
            result.Heroes.Add(eval);
        }
        _evaluationResult = result;
        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(TodayMemoryFragment));
        OnPropertyChanged(nameof(CurrentMemoryFragment));
        OnPropertyChanged(nameof(CurrentGold));
        OnPropertyChanged(nameof(OverallGradeText));
        OnPropertyChanged(nameof(HeroManageGradeText));
        OnPropertyChanged(nameof(GoldGradeText));
        OnPropertyChanged(nameof(FragmentGradeText));
    }
    // 마감정산 확정
    public bool TryConfirmSettlement()
    {
        if(SaveManager.Inst.CurrentPlayerModel.Day != 1 && _dayService.IsAdvanceable() == false)
        {
            return false;
        }
        // 날 넘기기 직전, 그날 등급 스냅샷 기록
        int settledDay = _dayService.CurrentDay;
        RecordDailyEvaluation(settledDay);
        _isWeeklyReviewDue = (settledDay % 7 == 0);
        _currencyService.ResetTodayMemoryFragment();
        if (_dayService.TryAdvanceDay() == false)
        {
            return false;
        }
        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(TodayMemoryFragment));
        OnPropertyChanged(nameof(CurrentMemoryFragment));
        return true;
    }
    // 오늘 평가를 스냅샷으로 기록 (같은 날 중복 방지)
    private void RecordDailyEvaluation(int day)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null || _evaluationResult == null)
        {
            return;
        }
        for (int i = 0; i < player.DailyEvaluations.Count; i++)
        {
            if (player.DailyEvaluations[i].Day == day)
            {
                player.DailyEvaluations.RemoveAt(i);
                break;
            }
        }
        DailyEvaluationRecord record = new DailyEvaluationRecord();
        record.Day = day;
        record.Gold = _currencyService.CurrentGold;
        record.TodayFragment = _currencyService.CurrentTodayMemoryFragment;
        record.TotalFragment = _currencyService.CurrentMemoryFragment;
        record.OverallGrade = (int)_evaluationResult.Overall;
        record.HeroManageGrade = (int)_evaluationResult.HeroManageGrade;
        record.GoldGrade = (int)_evaluationResult.GoldGrade;
        record.FragmentGrade = (int)_evaluationResult.FragmentGrade;
        for (int i = 0; i < _evaluationResult.Heroes.Count; i++)
        {
            HeroEvaluation h = _evaluationResult.Heroes[i];
            HeroDailyEvaluation hero = new HeroDailyEvaluation();
            hero.HeroId = h.HeroId;
            hero.AffectionGrade = (int)h.Affection;
            hero.SatisfactionGrade = (int)h.Satisfaction;
            record.Heroes.Add(hero);
        }
        player.DailyEvaluations.Add(record);
    }


    public bool IsLowGrade()
    {
        return _evaluationResult.Overall >= EvaluationGrade.F;
    }

    public GameOverType CheckResult()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (IsLowGrade())
        {
            player.LowGrade++;
        }
        
        if (player.LowGrade == 1)
        {
            return GameOverType.Warning;
        }
        else if (player.LowGrade == 2)
        {
            return GameOverType.GameOver;
        }

        return GameOverType.None;
    }
}
