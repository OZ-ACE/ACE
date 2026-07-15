using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class HeroUI : UIBase
{
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private HeroHotBar HeroHotBar;

    private List<HeroSlot> _activeSlots = new List<HeroSlot>();
    private HeroViewModel _currentVM;

    private void OnEnable()
    {
        RefreshHeroList().Forget();
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

    private async UniTask RefreshHeroList()
    {
        var playerModel = SaveManager.Inst.CurrentPlayerModel;

        for (int i = _activeSlots.Count; i < playerModel.HeroStats.Count; i++)
        {
            string currentHeroID = playerModel.HeroStats[i].HeroID;

            HeroModel model = new HeroModel();
            model.LoadHeroData(currentHeroID);

            HeroViewModel viewModel = new HeroViewModel();
            viewModel.Init(model);

            GameObject prefab = await ResourceManager.Inst.InstantiateAsync("Prefabs/UI/HeroSlot", Transform_Content);
            HeroSlot heroSlot = prefab.GetComponent<HeroSlot>();

            heroSlot.InitSlot(viewModel);
            heroSlot.OnSlotClick = SelectHeroSlot;
            _activeSlots.Add(heroSlot);
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
