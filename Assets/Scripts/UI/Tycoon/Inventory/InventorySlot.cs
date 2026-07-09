using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private TextMeshProUGUI Text_Count;
    [SerializeField] private Button Button_Select;

    private string _itemID;
    private InventoryViewModel _inventoryVM;

    private void Awake()
    {
        Button_Select.onClick.AddListener(OnClickSelect);
    }

    public async UniTask SetSlotData(string id, int count, InventoryViewModel vm)
    {
        _itemID = id;
        _inventoryVM = vm;

        Text_Count.text = $"{count}";
        Image_Icon.sprite = await ResourceManager.Inst.LoadSprite($"Image/Item[{id}]");

        UpdateState();
    }

    private void OnClickSelect()
    {
        _inventoryVM.SetSelectItem(_itemID);
    }

    public void UpdateState()
    {
        bool isSelected = (_inventoryVM.SelectedItemID == _itemID);

        LoadBackground(isSelected).Forget();
    }

    public async UniTask LoadBackground(bool isSelected)
    {
        string path = isSelected ? "Image/UI/Unselected" : "Image/UI/Selected";

        Image_Background.sprite = await ResourceManager.Inst.LoadSprite(path);
    }
}
