using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class HeroUI : UIBase
{
    [SerializeField] private Transform Transform_Content;

    private List<HeroSlot> _activeSlots = new List<HeroSlot>();

    private void OnEnable()
    {
        RefreshHeroList().Forget();
    }

    private async UniTask RefreshHeroList()
    {
        for (int i = 0; i < _activeSlots.Count; i++)
        {
            if (_activeSlots[i] != null)
            {
                Destroy(_activeSlots[i]);
            }
        }

        _activeSlots.Clear();

        var playerModel = SaveManager.Inst.CurrentPlayerModel;

        for (int i = 0; i < playerModel.HeroStats.Count; i++)
        {
            string currentHeroID = playerModel.HeroStats[i].HeroID;

            HeroModel model = new HeroModel();
            model.LoadHeroData(currentHeroID);

            HeroViewModel viewModel = new HeroViewModel();
            viewModel.Init(model);

            GameObject prefab = await ResourceManager.Inst.InstantiateAsync("Prefabs/UI/HeroSlot", Transform_Content);
            HeroSlot heroSlot = prefab.GetComponent<HeroSlot>();

            heroSlot.InitSlot(viewModel);
            _activeSlots.Add(heroSlot);
        }
    }
}
