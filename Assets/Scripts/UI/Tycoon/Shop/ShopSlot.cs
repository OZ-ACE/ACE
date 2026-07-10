using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ShopSlot : MonoBehaviour
{
    [SerializeField] private Image Image_Icon;
    [SerializeField] private TextMeshProUGUI Text_Name;
    [SerializeField] private TextMeshProUGUI Text_Price;
    [SerializeField] private TextMeshProUGUI Text_Stock;
    [SerializeField] private Button Button_Buy;

    private string _itemID;
    private ShopViewModel _shopVM;



    public async UniTask SetSlotData(SupportItem item, ShopViewModel vm)
    {
        _itemID = item.ID;
        _shopVM = vm;

        Button_Buy.onClick.RemoveListener(OnClickBuy);
        Button_Buy.onClick.AddListener(OnClickBuy);

        Text_Name.text = item.itemName;
        Text_Price.text = $"{item.Price} G";

        UpdateState();

        Image_Icon.sprite = await ResourceManager.Inst.LoadSprite($"Image/Item[{item.ID}]");
    }

    public void UpdateState()
    {
        if (_shopVM == null)
        {
            return;
        }

        int stock = _shopVM.GetRemainStock(_itemID);
        Text_Stock.text = $"재고 {stock}";

        Button_Buy.interactable = _shopVM.IsPurchasable(_itemID);
    }

    private void OnClickBuy()
    {
        PurchaseResult result = _shopVM.TryPurchase(_itemID);
        if (result != PurchaseResult.Success)
        {
            Debug.Log($"[ShopSlot] 구매 실패: {result}");
        }
    }
}