using System.Collections.Generic;
using UnityEngine;



//마감정산 뷰 모델
public class SettlementViewModel : ViewModelBase
{
    private readonly ICurrencyService _currencyService;
    private readonly DayService _dayService;

    // 업무평가 결과
    private WorkEvaluationResult _evaluationResult;
    public string OverallGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.Overall : EvaluationGrade.F); } }
    public string HeroManageGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.HeroManageGrade : EvaluationGrade.F); } }
    public string GoldGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.GoldGrade : EvaluationGrade.F); } }
    public string FragmentGradeText { get { return GetGradeText(_evaluationResult != null ? _evaluationResult.FragmentGrade : EvaluationGrade.F); } }

    public SettlementViewModel(ICurrencyService currencyService, DayService dayService)
    {
        _currencyService = currencyService;
        _dayService = dayService;
    }

    public int CurrentDay { get { return _dayService.CurrentDay; } }
    public int TodayMemoryFragment { get { return _currencyService.CurrentTodayMemoryFragment; } }
    public int CurrentGold { get { return _currencyService.CurrentGold; } }

    public int CurrentMemoryFragment { get { return _currencyService.CurrentMemoryFragment; } }

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

    // 마감정산 확정
    public bool TryConfirmSettlement()
    {
        _currencyService.ResetTodayMemoryFragment();

        bool isSuccess = _dayService.TryAdvanceDay();

        if (isSuccess == false)
        {
            return false;
        }

        OnPropertyChanged(nameof(CurrentDay));
        OnPropertyChanged(nameof(TodayMemoryFragment));
        OnPropertyChanged(nameof(CurrentMemoryFragment));
        OnPropertyChanged(nameof(CurrentGold));

        return true;
    }

    // 등급 문자 변환 헬퍼
    private string GetGradeText(EvaluationGrade grade)
    {
        return GradeCalculator.GetText(grade);
    }
    // 업무평가 실행 (뷰가 config 넘겨 호출)
    public void Evaluate(WorkEvaluationConfig config)
    {
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

}
