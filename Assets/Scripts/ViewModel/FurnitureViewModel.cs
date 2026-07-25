using System.Collections.Generic;

public class FurnitureViewModel : ViewModelBase
{
    private readonly FurnitureService _furnitureService;

    public FurnitureViewModel()
    {
        _furnitureService = GameManager.Inst.Services.FurnitureService;
    }

    public PurchaseResult PurchaseFurniture(string furnitureId)
    {
        return _furnitureService.PurchaseFurniture(furnitureId);
    }

    public bool IsPurchased(string furnitureId)
    {
        return _furnitureService.IsPurchased(furnitureId);
    }

    public List<FurnitureData> GetFurnitureList()
    {
        return GameDataManager.Inst.GetAllData<FurnitureData>();
    }

    public bool IsHeroAdmitted(string heroId)
    {
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

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

    public bool IsHeroWaitingAdmission(string heroId)
    {
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (playerModel == null || playerModel.AdmissionCandidates == null)
        {
            return false;
        }

        for (int i = 0; i < playerModel.AdmissionCandidates.Count; i++)
        {
            AdmissionCandidateSaveData candidate = playerModel.AdmissionCandidates[i];

            if (candidate.HeroId == heroId && candidate.IsAdmitted == true)
            {
                return true;
            }
        }

        return false;
    }
}