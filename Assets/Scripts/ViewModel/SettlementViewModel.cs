//마감정산 뷰 모델
public class SettlementViewModel : ViewModelBase
{
    private readonly ICurrencyService _currencyService;
    private readonly DayService _dayService;

    public SettlementViewModel(ICurrencyService currencyService, DayService dayService)
    {
        _currencyService = currencyService;
        _dayService = dayService;
    }

    public int CurrentDay { get { return _dayService.CurrentDay; } }
    public int TodayMemoryFragment { get { return _currencyService.CurrentTodayMemoryFragment; } }
    public int CurrentGold { get { return _currencyService.CurrentGold; } }

    public int CurrentMemoryFragment { get { return _currencyService.CurrentMemoryFragment; } }

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
}