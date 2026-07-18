using System.Collections.Generic;
using UnityEngine;

public class HeroUI : UIBase
{
    [SerializeField] private HeroSlot Prefab_Slot;
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private HeroHotBar HeroHotBar;

    private List<HeroSlot> _activeSlots = new List<HeroSlot>();
    private HeroViewModel _currentVM;

    private Dictionary<string, HeroViewModel> _heroVMs = new Dictionary<string, HeroViewModel>();
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
        if (_currentVM != null)
        {
            _currentVM.IsSelect = false;
            _currentVM = null;
        }

        HeroHotBar.gameObject.SetActive(false);
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

            HeroModel targetModel = null;
            var spawnedAgent = ObjectManager.Inst.GetSpawnAgent(currentHero);

            if (spawnedAgent != null)
            {
                targetModel = spawnedAgent.HeroModel;
            }

            if (!_heroVMs.TryGetValue(currentHero, out HeroViewModel heroVM))
            {
                if (targetModel == null)
                {
                    targetModel = new HeroModel();
                    targetModel.LoadHeroData(currentHero);
                }

                heroVM = new HeroViewModel();
                heroVM.Init(targetModel);
                _heroVMs.Add(currentHero, heroVM);
            }
            else
            {
                if (targetModel != null && heroVM.Model != targetModel)
                {
                    heroVM.Init(targetModel);
                }
                else if (heroVM.Model == null)
                {
                    HeroModel newModel = new HeroModel();
                    newModel.LoadHeroData(currentHero);
                    heroVM.Init(newModel);
                }
            }

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

        HeroHotBar.OpenHotBar(_currentVM);
    }
}
