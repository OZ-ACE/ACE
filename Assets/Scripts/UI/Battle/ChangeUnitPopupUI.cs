using System;
using System.Collections.Generic;
using UnityEngine;

// 전투 중 교체 투입할 대기 영웅을 고르는 팝업
public class ChangeUnitPopupUI : UIBase
{
    [SerializeField] private HeroSlot Prefab_HeroSlot;
    [SerializeField] private Transform Transform_SlotParent;
    [SerializeField] private GameObject Object_NoHeroMessage;

    private RectTransform _rectTransform;
    private readonly List<HeroSlot> _slotList = new List<HeroSlot>();
    private readonly Dictionary<string, HeroViewModel> _heroViewModelMap = new Dictionary<string, HeroViewModel>();

    public event Action<string> OnHeroSelected;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && IsPointerInsidePopup() == false)
        {
            ClosePopup();
        }
    }

    //팝업 박스 영역 안을 클릭했는지 판별한다
    private bool IsPointerInsidePopup()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
    }

    //교체 후보로 세울 대기 영웅이 한 명이라도 있는지 검사한다. 에너지를 소모하기 전에 호출한다
    public bool HasWaitingHero(List<string> excludedHeroIdList)
    {
        return BuildCandidateHeroIdList(excludedHeroIdList).Count > 0;
    }

    //제외 목록에 없는 보유 영웅을 후보로 띄운다
    public void OpenPopup(List<string> excludedHeroIdList)
    {
        gameObject.SetActive(true);

        RefreshHeroSlots(BuildCandidateHeroIdList(excludedHeroIdList));
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    //보유 영웅 중 이번 전투에 이미 출전했던 영웅을 뺀 목록을 만든다
    private List<string> BuildCandidateHeroIdList(List<string> excludedHeroIdList)
    {
        List<string> candidateHeroIdList = new List<string>();
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null || player.HeroStats == null)
        {
            return candidateHeroIdList;
        }

        foreach (HeroStat heroStat in player.HeroStats)
        {
            if (excludedHeroIdList != null && excludedHeroIdList.Contains(heroStat.HeroID) == true)
            {
                continue;
            }

            candidateHeroIdList.Add(heroStat.HeroID);
        }

        return candidateHeroIdList;
    }

    private void RefreshHeroSlots(List<string> candidateHeroIdList)
    {
        ClearSlots();

        Object_NoHeroMessage.SetActive(candidateHeroIdList.Count == 0);

        foreach (string heroId in candidateHeroIdList)
        {
            CreateSlot(heroId);
        }
    }

    private void CreateSlot(string heroId)
    {
        HeroViewModel heroViewModel = GetOrCreateHeroViewModel(heroId);

        if (heroViewModel == null)
        {
            return;
        }

        HeroSlot slot = Instantiate(Prefab_HeroSlot, Transform_SlotParent);

        slot.OnSlotClick = HandleSlotClicked;
        slot.InitSlot(heroViewModel);

        _slotList.Add(slot);
    }

    //로스터와 동일한 방식으로 표시용 뷰모델을 만든다. 인스턴스를 분리해 로스터 선택 상태와 섞이지 않게 한다
    private HeroViewModel GetOrCreateHeroViewModel(string heroId)
    {
        if (_heroViewModelMap.TryGetValue(heroId, out HeroViewModel cachedViewModel) == true)
        {
            return cachedViewModel;
        }

        HeroModel model = null;
        HeroMovingAgent agent = ObjectManager.Inst.GetSpawnAgent(heroId);

        if (agent != null)
        {
            model = agent.HeroModel;
        }

        if (model == null)
        {
            model = new HeroModel();
            model.LoadHeroData(heroId);
        }

        HeroViewModel heroViewModel = new HeroViewModel();
        heroViewModel.Init(model);

        _heroViewModelMap.Add(heroId, heroViewModel);

        return heroViewModel;
    }

    private void HandleSlotClicked(HeroViewModel heroViewModel)
    {
        if (heroViewModel == null)
        {
            return;
        }

        OnHeroSelected?.Invoke(heroViewModel.HeroID);
        ClosePopup();
    }

    private void ClearSlots()
    {
        foreach (HeroSlot slot in _slotList)
        {
            if (slot == null)
            {
                continue;
            }

            slot.OnSlotClick = null;
            Destroy(slot.gameObject);
        }

        _slotList.Clear();
    }
}