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

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        _candidateIdGenerator = 0;
        _candidateDict.Clear();

        CreateWaitingHeroes();
    }

    public void CreateWaitingHeroes()
    {
        _candidateDict.Clear();

        List<HeroData> heroDatas = GameDataManager.Inst.GetAllData<HeroData>();

        if (heroDatas == null || heroDatas.Count <= 0)
        {
            Debug.LogWarning("입소 후보로 뽑을 영웅 데이터가 없습니다.");
            return;
        }

        List<HeroData> selectedHeroDatas = GetRandomHeroDatas(heroDatas, _maxWaitingHeroCount);

        for (int i = 0; i < selectedHeroDatas.Count; i++)
        {
            AddCandidate(selectedHeroDatas[i].ID);
        }
    }

    public void RefreshWaitingHeroes()
    {
        ExpireAllCandidates();
        CreateWaitingHeroes();
    }

    public bool TryAdmitHero(string heroId)
    {
        AdmissionCandidateModel candidateModel = GetCandidateByHeroId(heroId);

        if (candidateModel == null)
        {
            Debug.LogWarning($"입소 후보에 없는 영웅입니다. HeroId : {heroId}");
            return false;
        }

        if (candidateModel.IsAdmitted == true)
        {
            Debug.LogWarning($"이미 입소 처리된 영웅입니다. HeroId : {heroId}");
            return false;
        }

        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(heroId);

        if (heroData == null)
        {
            Debug.LogWarning($"HeroData 를 찾을 수 없습니다. HeroId : {heroId}");
            return false;
        }

        candidateModel.Admit();

        Debug.Log($"{heroData.HeroName} 입소 연락보냄!");
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
        List<HeroData> pool = new List<HeroData>(heroDatas);
        List<HeroData> result = new List<HeroData>();

        while (pool.Count > 0 && result.Count < count)
        {
            int randomIndex = Random.Range(0, pool.Count);
            HeroData heroData = pool[randomIndex];

            result.Add(heroData);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }
}