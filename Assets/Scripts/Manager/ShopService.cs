// 상점 뷰모델을 생성·보관
public class ShopService
{
    private ShopViewModel _shopViewModel;

    private readonly ICurrencyService _currencyService;

    public ShopService(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    public ShopViewModel CreateShopViewModel(InventoryViewModel inventoryViewModel)
    {
        if (_shopViewModel != null)
        {
            return _shopViewModel;
        }

        _shopViewModel = new ShopViewModel(_currencyService, inventoryViewModel);
        return _shopViewModel;
    }

    public ShopViewModel GetShopViewModel()
    {
        return _shopViewModel;
    }
}