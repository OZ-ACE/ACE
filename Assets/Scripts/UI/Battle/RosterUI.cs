using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//보유(입소) 영웅 중 3명을 골라 전투로 넘기는 로스터 팝업. 기존 HeroSlot/HeroViewModel 재사용
public class RosterUI : UIBase
{
    [Header("슬롯")]
    [SerializeField] private HeroSlot Prefab_HeroSlot;
    [SerializeField] private Transform Transform_SlotParent;
    [Header("버튼")]
    [SerializeField] private Button Button_StartBattle;
    [SerializeField] private Button Button_Close;
    private RosterViewModel _viewModel;
    private readonly List<HeroSlot> _slots = new List<HeroSlot>();
    private readonly Dictionary<string, HeroViewModel> _heroVMs = new Dictionary<string, HeroViewModel>();
    private Action<List<string>> _onConfirm;
    public void Initialize(Action<List<string>> onConfirm)
    {
        _onConfirm = onConfirm;
        _viewModel = GameManager.Inst.Services.RosterService.GetRosterViewModel();
        if (_viewModel == null)
        {
            _viewModel = GameManager.Inst.Services.RosterService.CreateRosterViewModel();
        }
        _viewModel.LoadSelection();
        Button_StartBattle.onClick.RemoveListener(OnClickStartBattle);
        Button_StartBattle.onClick.AddListener(OnClickStartBattle);
        Button_Close.onClick.RemoveListener(OnClickClose);
        Button_Close.onClick.AddListener(OnClickClose);
        BuildSlots();
        RefreshSelectionView();
    }
    private void BuildSlots()
    {
        ClearSlots();
        List<string> ownedHeroIds = _viewModel.GetOwnedHeroIds();
        foreach (string heroId in ownedHeroIds)
        {
            HeroViewModel heroVM = GetOrCreateHeroVM(heroId);
            if (heroVM == null)
            {
                continue;
            }
            HeroSlot slot = Instantiate(Prefab_HeroSlot, Transform_SlotParent);
            slot.OnSlotClick = OnClickSlot;
            slot.InitSlot(heroVM);
            _slots.Add(slot);
        }
    }
    //로스터 전용 HeroViewModel 생성 (HeroUI와 동일 방식, 인스턴스는 분리해 선택 상태 오염 방지)
    private HeroViewModel GetOrCreateHeroVM(string heroId)
    {
        if (_heroVMs.TryGetValue(heroId, out HeroViewModel cached))
        {
            return cached;
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
        HeroViewModel heroVM = new HeroViewModel();
        heroVM.Init(model);
        _heroVMs.Add(heroId, heroVM);
        return heroVM;
    }
    private void ClearSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null)
            {
                continue;
            }
            _slots[i].OnSlotClick = null;
            Destroy(_slots[i].gameObject);
        }
        _slots.Clear();
    }
    //슬롯 클릭 시 3명 규칙으로 토글하고, 모든 카드의 IsSelect를 실제 선택 상태에 맞춘다
    private void OnClickSlot(HeroViewModel heroVM)
    {
        _viewModel.ToggleSelect(heroVM.HeroID);
        RefreshSelectionView();
    }
    private void RefreshSelectionView()
    {
        foreach (KeyValuePair<string, HeroViewModel> pair in _heroVMs)
        {
            pair.Value.IsSelect = _viewModel.IsSelected(pair.Key);
        }
        Button_StartBattle.interactable = _viewModel.CanStartBattle();
    }
    private void OnClickStartBattle()
    {
        if (_viewModel.CanStartBattle() == false)
        {
            return;
        }
        List<string> selectedHeroIds = _viewModel.ConfirmSelection();
        UIManager.Inst.CloseRosterUI();
        _onConfirm?.Invoke(selectedHeroIds);
    }
    private void OnClickClose()
    {
        UIManager.Inst.CloseRosterUI();
        ObjectManager.Inst.ExitBattle();
    }
    private void OnDestroy()
    {
        Button_StartBattle.onClick.RemoveListener(OnClickStartBattle);
        Button_Close.onClick.RemoveListener(OnClickClose);
        ClearSlots();
    }
}