public class WeeklyEvaluationService
{
    private WeeklyEvaluationViewModel _weeklyEvaluationViewModel;
    private readonly DayService _dayService;
    public WeeklyEvaluationService(DayService dayService)
    {
        _dayService = dayService;
    }
    public WeeklyEvaluationViewModel CreateWeeklyEvaluationViewModel()
    {
        if (_weeklyEvaluationViewModel != null)
        {
            return _weeklyEvaluationViewModel;
        }
        _weeklyEvaluationViewModel = new WeeklyEvaluationViewModel(_dayService);
        return _weeklyEvaluationViewModel;
    }
    public WeeklyEvaluationViewModel GetWeeklyEvaluationViewModel()
    {
        return _weeklyEvaluationViewModel;
    }
}