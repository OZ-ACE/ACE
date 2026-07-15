using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private TextMeshProUGUI Text_Count;
    [SerializeField] private Button Button_Select;

    [SerializeField] private Sprite Sprite_Unselect;
    [SerializeField] private Sprite Sprite_Select;

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

        SetBackground(isSelected);
    }

    public void SetBackground(bool isSelected)
    {
        Sprite sprite = isSelected ? Sprite_Select : Sprite_Unselect;

        Image_Background.sprite = sprite;
    }
}
