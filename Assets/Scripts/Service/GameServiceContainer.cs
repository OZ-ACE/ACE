using UnityEngine;

public class GameServiceContainer
{
    // 전역 재화 서비스
    public ICurrencyService CurrencyService { get; private set; }

    // 상점 뷰모델 보관 서비스
    public ShopService ShopService { get; private set; }

    // 건설 뷰모델 보관 서비스
    public BuildService BuildService { get; private set; }
    public FurnitureService FurnitureService { get; private set; }
    public EpisodeService EpisodeService { get; private set; }

    // 날짜진행 뷰모델 보관 서비스
    public DayService DayService { get; private set; }


    // 일일 정산 뷰모델 보관 서비스
    public SettlementService SettlementService { get; private set; }

    //주간 정산 뷰모델 보관 서비스
    public WeeklyEvaluationService WeeklyEvaluationService { get; private set; }

    // 퀘스트 뷰모델 보관 서비스
    public QuestService QuestService { get; private set; }
    public RoomAssignmentService RoomAssignmentService { get; private set; }
    public HeroRequestService HeroRequestService { get; private set; }


    // 영웅로스터 뷰모델 보관 서비스
    public RosterService RosterService { get; private set; }

    // 튜토리얼 뷰모델 보관 서비스

    public TutorialService TutorialService { get; private set; }

    public void Initialize()
    {
        CurrencyService = new CurrencyService();
        ShopService = new ShopService(CurrencyService);
        BuildService = new BuildService(CurrencyService);
        FurnitureService = new FurnitureService();
        EpisodeService = new EpisodeService();
        DayService = new DayService();
        SettlementService = new SettlementService(CurrencyService, DayService);
        QuestService = new QuestService(CurrencyService);
        RoomAssignmentService = new RoomAssignmentService();
        RosterService = new RosterService();
        WeeklyEvaluationService = new WeeklyEvaluationService(DayService);
        TutorialService = new TutorialService();
        HeroRequestService = new HeroRequestService();

        DayService.OnChangeDay += HeroRequestService.OnChangeDay;
        DayService.OnChangeHour += HeroRequestService.OnChangeHour;
        Debug.Log("[GameServiceContainer] HeroRequestService 날짜/시간 이벤트 연결 완료");
    }

    public void Release()
    {
        //EpisodeService?.Release();
    }
}