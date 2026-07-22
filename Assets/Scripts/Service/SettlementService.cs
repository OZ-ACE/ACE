using UnityEngine;
public class SettlementService
{
    private SettlementViewModel _settlementViewModel;
    private readonly ICurrencyService _currencyService;
    private readonly DayService _dayService;
    public SettlementService(ICurrencyService currencyService, DayService dayService)
    {
        _currencyService = currencyService;
        _dayService = dayService;
    }
    public SettlementViewModel CreateSettlementViewModel()
    {
        if (_settlementViewModel != null)
        {
            return _settlementViewModel;
        }
        _settlementViewModel = new SettlementViewModel(_currencyService, _dayService);
        return _settlementViewModel;
    }
    public SettlementViewModel GetSettlementViewModel()
    {
        return _settlementViewModel;
    }
    // 상세보기용 스냅샷 뷰모델 (매번 새 인스턴스)
    public SettlementViewModel CreateSnapshotViewModel()
    {
        return new SettlementViewModel(_currencyService, _dayService);
    }
}