public class GameServiceContainer
{
    // 전역 재화 서비스
    public ICurrencyService CurrencyService { get; private set; }
    // 상점 뷰모델 보관 서비스
    public ShopService ShopService { get; private set; }

    // 건설 뷰모델 보관 서비스
    public BuildService BuildService { get; private set; }
    public EpisodeService EpisodeService { get; private set; }

    public void Initialize()
    {
        CurrencyService = new CurrencyService();
        ShopService = new ShopService(CurrencyService);
        BuildService = new BuildService(CurrencyService);
        EpisodeService = new EpisodeService();
    }

    public void Release()
    {
        //EpisodeService?.Release();
    }
}