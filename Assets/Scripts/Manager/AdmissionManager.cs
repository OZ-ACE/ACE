using System.Collections.Generic;
using UnityEngine;

public class AdmissionManager : SingletonBase<AdmissionManager>
{
    [SerializeField] private int _refreshCost = 100;
    [SerializeField] private int _maxWaitingHeroCount = 3;

    public int RefreshCost => _refreshCost;

    private int _candidateIdGenerator;

    private readonly Dictionary<int, AdmissionCandidateModel> _candidateDict = new Dictionary<int, AdmissionCandidateModel>();
    public IReadOnlyDictionary<int, AdmissionCandidateModel> CandidateDict => _candidateDict;

    private DayService _dayService;

    public bool IsInitialized { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (SaveManager.Inst.IsPlayerDataLoaded == false)
        {
            Debug.LogError("플레이어 저장 데이터 로드 전에 AdmissionManager를 초기화할 수 없음.");
            return;
        }

        if (SaveManager.Inst.IsInitialized == false)
        {
            Debug.LogError("SaveManager 초기화 전에 AdmissionManager를 초기화할 수 없음.");
            return;
        }

        _candidateIdGenerator = 0;
        _candidateDict.Clear();

        bool isRestored = RestoreWaitingHeroes();

        if (isRestored == false)
        {
            CreateWaitingHeroes();
        }

        BindDayService();
        IsInitialized = true;
    }

    public void Reload()
    {
        IsInitialized = false;
        Initialize();
    }

    public void CreateWaitingHeroes()
    {
        _candidateDict.Clear();

        List<HeroData> heroDatas = GameDataManager.Inst.GetAllData<HeroData>();

        if (heroDatas == null || heroDatas.Count <= 0)
        {
            Debug.LogWarning("입소 후보로 뽑을 영웅 데이터가 없음.");
            return;
        }

        List<HeroData> selectedHeroDatas = GetRandomHeroDatas(heroDatas, _maxWaitingHeroCount);

        for (int i = 0; i < selectedHeroDatas.Count; i++)
        {
            AddCandidate(selectedHeroDatas[i].ID);
        }

        SaveWaitingHeroes();
    }

    public void RefreshWaitingHeroes()
    {
        ExpireAllCandidates();
        CreateWaitingHeroes();
    }

    public bool TryAdmitHero(string heroId, long roomInstanceId)
    {
        AdmissionCandidateModel candidateModel = GetCandidateByHeroId(heroId);

        if (candidateModel == null)
        {
            Debug.LogWarning($"입소 후보에 없음. ID : {heroId}");
            return false;
        }

        if (candidateModel.IsAdmitted == true)
        {
            Debug.LogWarning($"이미 입소 처리됨. ID : {heroId}");
            return false;
        }

        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(heroId);

        if (heroData == null)
        {
            Debug.LogWarning($"HeroData 찾을 수 없음. ID : {heroId}");
            return false;
        }

        RoomAssignmentService roomService = GameManager.Inst.Services.RoomAssignmentService;

        if (roomService == null)
        {
            Debug.LogWarning("RoomAssignmentService 찾을 수 없음.");
            return false;
        }

        if (roomService.AssignRoom(heroId, roomInstanceId) == false)
        {
            Debug.LogWarning($"방 배정에 실패함. HeroId : {heroId}, RoomInstanceId : {roomInstanceId}");
            return false;
        }

        if (TryRegisterAdmittedHero(heroId) == false)
        {
            bool isRollbackSucceeded = roomService.UnassignRoom(heroId);

            if (isRollbackSucceeded == false)
            {
                Debug.LogError($"입소 실패 후 방 배정 롤백에도 실패함. HeroId : {heroId}");
            }

            return false;
        }

        candidateModel.Admit();
        SaveWaitingHeroes();

        return true;
    }

    public AdmissionCandidateModel GetCandidateModel(int candidateId)
    {
        if (_candidateDict.TryGetValue(candidateId, out AdmissionCandidateModel candidateModel) == true)
        {
            return candidateModel;
        }

        return null;
    }

    public List<AdmissionCandidateModel> GetCandidateModels()
    {
        return new List<AdmissionCandidateModel>(_candidateDict.Values);
    }

    private bool TryRegisterAdmittedHero(string heroId)
    {
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (playerModel == null)
        {
            return false;
        }

        if (playerModel.HeroStats == null)
        {
            playerModel.HeroStats = new List<HeroStat>();
        }

        for (int i = 0; i < playerModel.HeroStats.Count; i++)
        {
            if (playerModel.HeroStats[i].HeroID == heroId)
            {
                return true;
            }
        }

        HeroStat heroStat = new HeroStat {HeroID = heroId, Affection = 0, Satisfaction = 0};

        playerModel.HeroStats.Add(heroStat);
        return true;
    }

    private void AddCandidate(string heroId)
    {
        if (string.IsNullOrEmpty(heroId) == true)
        {
            return;
        }

        _candidateIdGenerator++;

        AdmissionCandidateModel candidateModel = new AdmissionCandidateModel(_candidateIdGenerator, heroId);
        _candidateDict.Add(candidateModel.CandidateId, candidateModel);
    }

    private AdmissionCandidateModel GetCandidateByHeroId(string heroId)
    {
        foreach (KeyValuePair<int, AdmissionCandidateModel> pair in _candidateDict)
        {
            if (pair.Value.HeroId == heroId)
            {
                return pair.Value;
            }
        }

        return null;
    }

    private void ExpireAllCandidates()
    {
        foreach (KeyValuePair<int, AdmissionCandidateModel> pair in _candidateDict)
        {
            pair.Value.Expire();
        }
    }

    private List<HeroData> GetRandomHeroDatas(List<HeroData> heroDatas, int count)
    {
        List<HeroData> pool = new List<HeroData>();
        List<HeroData> result = new List<HeroData>();

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        for (int i = 0; i < heroDatas.Count; i++)
        {
            HeroData heroData = heroDatas[i];

            if (IsAdmittedHero(playerModel, heroData.ID) == true)
            {
                continue;
            }

            pool.Add(heroData);
        }

        while (pool.Count > 0 && result.Count < count)
        {
            int randomIndex = Random.Range(0, pool.Count);
            HeroData heroData = pool[randomIndex];

            result.Add(heroData);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }

    private bool IsAdmittedHero(PlayerModel playerModel, string heroId)
    {
        if (playerModel == null || playerModel.HeroStats == null)
        {
            return false;
        }

        for (int i = 0; i < playerModel.HeroStats.Count; i++)
        {
            if (playerModel.HeroStats[i].HeroID == heroId)
            {
                return true;
            }
        }

        return false;
    }

    private bool RestoreWaitingHeroes()
    {
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (playerModel == null)
        {
            return false;
        }

        if (playerModel.AdmissionCandidates == null || playerModel.AdmissionCandidates.Count <= 0)
        {
            return false;
        }

        for (int i = 0; i < playerModel.AdmissionCandidates.Count; i++)
        {
            AdmissionCandidateSaveData saveData = playerModel.AdmissionCandidates[i];

            if (saveData == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(saveData.HeroId) == true)
            {
                continue;
            }

            AdmissionCandidateModel candidateModel = new AdmissionCandidateModel(saveData.CandidateId, saveData.HeroId, saveData.IsAdmitted);

            _candidateDict.Add(candidateModel.CandidateId, candidateModel);

            if (_candidateIdGenerator < candidateModel.CandidateId)
            {
                _candidateIdGenerator = candidateModel.CandidateId;
            }
        }

        return _candidateDict.Count > 0;
    }

    private void SaveWaitingHeroes()
    {
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (playerModel == null)
        {
            return;
        }

        if (playerModel.AdmissionCandidates == null)
        {
            playerModel.AdmissionCandidates = new List<AdmissionCandidateSaveData>();
        }

        playerModel.AdmissionCandidates.Clear();

        foreach (KeyValuePair<int, AdmissionCandidateModel> pair in _candidateDict)
        {
            AdmissionCandidateModel candidateModel = pair.Value;
            AdmissionCandidateSaveData saveData = new AdmissionCandidateSaveData {CandidateId = candidateModel.CandidateId, HeroId = candidateModel.HeroId, IsAdmitted = candidateModel.IsAdmitted};

            playerModel.AdmissionCandidates.Add(saveData);
        }

        //SaveManager.Inst.RequestSaveData(playerModel);
    }

    private void BindDayService()
    {
        DayService dayService = GameManager.Inst.Services.DayService;

        if (dayService == null)
        {
            Debug.LogWarning("DayService 를 찾을 수 없음.");
            return;
        }

        if (_dayService != null)
        {
            _dayService.OnChangeDay -= OnChangeDay;
        }

        _dayService = dayService;
        _dayService.OnChangeDay += OnChangeDay;
    }

    private void OnChangeDay(int day)
    {
        RefreshWaitingHeroes();
    }

    private void OnDestroy()
    {
        if (_dayService != null)
        {
            _dayService.OnChangeDay -= OnChangeDay;
            _dayService = null;
        }
    }
}