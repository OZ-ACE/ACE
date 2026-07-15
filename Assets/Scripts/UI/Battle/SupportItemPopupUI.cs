using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

// 지원 아이템 팝업 - 사용 가능한 서포트 아이템 목록을 보여주고 선택/hover를 처리한다
public class SupportItemPopupUI : UIBase
{
    [SerializeField] private Transform Transform_SlotContent;
    [SerializeField] private GameObject Object_EmptyMessage;
    [SerializeField] private GameObject Object_ItemDescription;
    [SerializeField] private TextMeshProUGUI Text_ItemDescription;

    private RectTransform _rectTransform;
    private readonly List<SupportItemSlot> _activeSlotList = new List<SupportItemSlot>();

    public event Action<string> OnItemApplied;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && IsPointerInsidePopup() == false)
        {
            ClosePopup();
        }
    }

    //팝업 박스 영역 안을 클릭했는지 판별한다
    private bool IsPointerInsidePopup()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
    }

    //표시할 아이템 목록을 받아 팝업을 열고 슬롯을 채운다. itemIdToCountMap은 itemId - 보유수량 쌍
    public void OpenPopup(Dictionary<string, int> itemIdToCountMap)
    {
        gameObject.SetActive(true);

        Object_ItemDescription.SetActive(false);

        RefreshItemSlots(itemIdToCountMap);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    private void RefreshItemSlots(Dictionary<string, int> itemIdToCountMap)
    {
        ClearSlots();

        Object_EmptyMessage.SetActive(itemIdToCountMap.Count == 0);

        foreach (KeyValuePair<string, int> entry in itemIdToCountMap)
        {
            CreateSlot(entry.Key, entry.Value);
        }
    }

    private void CreateSlot(string itemId, int count)
    {
        GameObject loadedObj = (GameObject)Resources.Load("Prefabs/UI/SupportItemSlot");
        GameObject slotObj = Instantiate(loadedObj, Transform_SlotContent);

        SupportItemSlot slot = slotObj.GetComponent<SupportItemSlot>();

        if (slot == null)
        {
            return;
        }

        slot.SetSlotData(itemId, count).Forget();

        slot.OnItemSelected += HandleItemSelected;
        slot.OnItemHoverEnter += HandleItemHoverEnter;
        slot.OnItemHoverExit += HandleItemHoverExit;

        _activeSlotList.Add(slot);
    }

    private void HandleItemSelected(string itemId)
    {
        OnItemApplied?.Invoke(itemId);
        ClosePopup();
    }

    private void HandleItemHoverEnter(string description)
    {
        Text_ItemDescription.text = description;
        Object_ItemDescription.SetActive(true);
    }

    private void HandleItemHoverExit()
    {
        Object_ItemDescription.SetActive(false);
    }

    private void ClearSlots()
    {
        foreach (SupportItemSlot slot in _activeSlotList)
        {
            if (slot == null)
            {
                continue;
            }

            slot.OnItemSelected -= HandleItemSelected;
            slot.OnItemHoverEnter -= HandleItemHoverEnter;
            slot.OnItemHoverExit -= HandleItemHoverExit;

            Destroy(slot.gameObject);
        }

        _activeSlotList.Clear();
    }

    //데모 빌드용 임시 - 지원하기 버튼 실연동되면 이 Start() 통째로 삭제
    private void Start()
    {
        Test_OpenWithDummyData();
    }

    //독립 테스트용 - 실제 필터링 로직 연결 전까지 더미 데이터로 팝업 확인, 이후 삭제 예정
    [ContextMenu("팝업 테스트 오픈 (더미 데이터)")]
    private void Test_OpenWithDummyData()
    {
        Dictionary<string, int> dummyData = new Dictionary<string, int>();
        dummyData.Add("item_01", 2);
        dummyData.Add("item_02", 3);

        OpenPopup(dummyData);
    }
}