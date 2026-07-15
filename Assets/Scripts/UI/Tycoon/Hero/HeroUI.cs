using System.Collections.Generic;
using UnityEngine;

public class HeroUI : UIBase
{
    [SerializeField] private HeroSlot Prefab_Slot;
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private HeroHotBar HeroHotBar;

    private List<HeroSlot> _activeSlots = new List<HeroSlot>();
    private HeroViewModel _currentVM;

    private ObjectPooling<HeroSlot> _slotPool;

    private void Awake()
    {
        _slotPool = new ObjectPooling<HeroSlot>(Prefab_Slot, Transform_Content, 3, 10);
    }

    private void OnEnable()
    {
        RefreshHeroList();
    }

    private void OnDisable()
    {
        HeroHotBar.gameObject.SetActive(false);

        if (_currentVM != null)
        {
            _currentVM.IsSelect = false;
            _currentVM = null;
        }
    }

    private void RefreshHeroList()
    {
        foreach (HeroSlot slot in _activeSlots)
        {
            _slotPool.Release(slot);
        }

        _activeSlots.Clear();

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        foreach (var hero in playerModel.HeroStats)
        {
            string currentHero = hero.HeroID;

            HeroModel heroModel = new HeroModel();
            heroModel.LoadHeroData(currentHero);

            HeroViewModel heroVM = new HeroViewModel();
            heroVM.Init(heroModel);

            HeroSlot heroSlot = _slotPool.Get();
            _activeSlots.Add(heroSlot);

            heroSlot.InitSlot(heroVM);
            heroSlot.OnSlotClick = SelectHeroSlot;
        }
    }

    private void SelectHeroSlot(HeroViewModel heroVM)
    {
        if (_currentVM != null && _currentVM != heroVM)
        {
            _currentVM.IsSelect = false;
        }

        _currentVM = heroVM;
        _currentVM.IsSelect = true;

        HeroHotBar.OpenHotBar(heroVM);
    }
}
