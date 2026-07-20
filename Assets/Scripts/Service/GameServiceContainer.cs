public class GameServiceContainer
{
    // 전역 재화 서비스
    public ICurrencyService CurrencyService { get; private set; }

    // 상점 뷰모델 보관 서비스
    public ShopService ShopService { get; private set; }

    // 건설 뷰모델 보관 서비스
    public BuildService BuildService { get; private set; }
    public EpisodeService EpisodeService { get; private set; }

    // 날짜진행 뷰모델 보관 서비스
    public DayService DayService { get; private set; }


    // 정산 뷰모델 보관 서비스
    public SettlementService SettlementService { get; private set; }

    // 퀘스트 뷰모델 보관 서비스
    public QuestService QuestService { get; private set; }


    // 영웅로스터 뷰모델 보관 서비스
    public RosterService RosterService { get; private set; }

    public void Initialize()
    {
        CurrencyService = new CurrencyService();
        ShopService = new ShopService(CurrencyService);
        BuildService = new BuildService(CurrencyService);
        EpisodeService = new EpisodeService();
        DayService = new DayService();
        SettlementService = new SettlementService(CurrencyService, DayService);
        QuestService = new QuestService(CurrencyService);
        RosterService = new RosterService();
    }

    public void Release()
    {
        //EpisodeService?.Release();
    }
}