public class GameServiceContainer
{
    // 전역 재화 서비스
    public ICurrencyService CurrencyService { get; private set; }

    // 상점 뷰모델 보관 서비스
    public ShopService ShopService { get; private set; }

    // 건설 뷰모델 보관 서비스
    public BuildService BuildService { get; private set; }
    public DialogueService DialogueService { get; private set; }
    public EpisodeService EpisodeService { get; private set; }

    // 날짜진행 뷰모델 보관 서비스
    public DayService DayService { get; private set; }


    // 정산 뷰모델 보관 서비스
    public SettlementService SettlementService { get; private set; }

    public void Initialize()
    {
        CurrencyService = new CurrencyService();
        ShopService = new ShopService(CurrencyService);
        BuildService = new BuildService(CurrencyService);
        DialogueService = new DialogueService();
        EpisodeService = new EpisodeService();
        DayService = new DayService();
        SettlementService = new SettlementService(CurrencyService, DayService);
    }

    public void InitializeAfterLoad()
    {
        EpisodeService.Initialize();
    }

    public void Release()
    {
        DialogueService?.Release();
        EpisodeService?.Release();
    }
}