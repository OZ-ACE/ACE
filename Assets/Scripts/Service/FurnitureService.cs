using System.Collections.Generic;

public class FurnitureService
{
    private PlayerModel PlayerModel => SaveManager.Inst.CurrentPlayerModel;

    public bool IsPurchased(string furnitureDataId)
    {
        if (string.IsNullOrEmpty(furnitureDataId))
        {
            return false;
        }

        List<FurnitureProgressModel> progressList = PlayerModel.FurnitureProgressList;

        if (progressList == null)
        {
            return false;
        }

        foreach (FurnitureProgressModel progress in progressList)
        {
            if (progress.FurnitureDataId == furnitureDataId)
            {
                return true;
            }
        }

        return false;
    }

    public PurchaseResult PurchaseFurniture(string furnitureId)
    {
        FurnitureData furnitureData = GameDataManager.Inst.GetData<FurnitureData>(furnitureId);

        if (furnitureData == null)
        {
            return PurchaseResult.InvalidItem;
        }

        if (IsPurchased(furnitureId))
        {
            return PurchaseResult.AlreadyPurchased;
        }

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (PlayerModel.Gold < furnitureData.Price)
        {
            return PurchaseResult.NotEnoughGold;
        }

        PlayerModel.Gold -= furnitureData.Price;

        PlayerModel.FurnitureProgressList.Add(new FurnitureProgressModel(){FurnitureDataId = furnitureId});

        SaveManager.Inst.RequestSaveData(PlayerModel);

        return PurchaseResult.Success;
    }
}