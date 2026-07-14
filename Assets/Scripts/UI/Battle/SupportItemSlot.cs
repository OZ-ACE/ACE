using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 지원 아이템 팝업 안에서 개별 아이템 하나를 표시하는 슬롯
public class SupportItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image Image_Item;
    [SerializeField] private TextMeshProUGUI Text_ItemName;
    [SerializeField] private TextMeshProUGUI Text_ItemCount;

    private Button _button;
    private string _itemId;
    private string _itemDescription;

    public event Action<string> OnItemSelected;
    public event Action<string> OnItemHoverEnter;
    public event Action OnItemHoverExit;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClickSelect);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClickSelect);
    }

    //슬롯 표시 데이터를 설정한다. itemId는 SupportItem 데이터 테이블 ID, count는 보유 수량
    public async UniTask SetSlotData(string itemId, int count)
    {
        _itemId = itemId;

        SupportItem itemData = GameDataManager.Inst.GetData<SupportItem>(itemId);

        if (itemData == null)
        {
            Debug.LogWarning($"[SupportItemSlot] 아이템 데이터를 찾을 수 없습니다. ID : {itemId}");
            return;
        }

        Text_ItemName.text = itemData.ItemName;
        Text_ItemCount.text = $"{count}";
        _itemDescription = itemData.Description;

        Image_Item.sprite = await ResourceManager.Inst.LoadSprite($"Image/Item[{itemId}]");
    }

    private void OnClickSelect()
    {
        OnItemSelected?.Invoke(_itemId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnItemHoverEnter?.Invoke(_itemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnItemHoverExit?.Invoke();
    }
}