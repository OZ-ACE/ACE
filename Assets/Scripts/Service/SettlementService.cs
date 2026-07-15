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
}
