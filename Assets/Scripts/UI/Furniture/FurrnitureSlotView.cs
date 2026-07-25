using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureSlotView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text Text_Name;
    [SerializeField] private TMP_Text Text_Description;
    [SerializeField] private TMP_Text Text_Cost;

    [SerializeField] private Image Image_Furniture;

    [SerializeField] private Button Button_Buy;

    private FurnitureData _furnitureData;
    private FurnitureViewModel _viewModel;

    private void Awake()
    {
        Button_Buy.onClick.AddListener(OnClickBuy);
    }

    public void SetData(FurnitureData furnitureData, FurnitureViewModel viewModel, bool canPurchase)
    {
        _furnitureData = furnitureData;
        _viewModel = viewModel;

        Text_Name.text = furnitureData.FurnitureName;
        Text_Description.text = furnitureData.Description;
        Text_Cost.text = furnitureData.Price.ToString();

        bool isPurchased = _viewModel.IsPurchased(_furnitureData.ID);

        Button_Buy.interactable = canPurchase && !isPurchased;

        LoadFurnitureImage().Forget();
    }

    private void OnClickBuy()
    {
        PurchaseResult result = _viewModel.PurchaseFurniture(_furnitureData.ID);

        if (result == PurchaseResult.Success)
        {
            Button_Buy.interactable = false;

            FurnitureSpawner furnitureSpawner = FindFirstObjectByType<FurnitureSpawner>();

            if (furnitureSpawner != null)
            {
                furnitureSpawner.SpawnFurniture(_furnitureData.ID).Forget();
            }
        }
    }

    private async UniTaskVoid LoadFurnitureImage()
    {
        string address = $"Image/Furniture/{_furnitureData.ID}";

        Sprite sprite = await ResourceManager.Inst.LoadSprite(address);

        if (sprite == null)
        {
            Debug.LogWarning($"가구 아이콘 로드 실패 : {address}");
            return;
        }

        Image_Furniture.sprite = sprite;
    }
}