using System.Collections.Generic;
using UnityEngine;

public class FurniturePanelView : MonoBehaviour
{
    [Header("슬롯 프리팹 / 부모")]
    [SerializeField] private GameObject Prefab_FurnitureSlot;
    [SerializeField] private Transform Transform_SlotParent;

    private FurnitureViewModel _viewModel;
    private readonly List<FurnitureSlotView> _activeSlots = new();

    private void Awake()
    {
        _viewModel = new FurnitureViewModel();
    }

    private void OnEnable()
    {
        ShowFurnitureList();
    }

    public void ShowFurnitureList()
    {
        ClearSlots();

        List<FurnitureData> furnitureList = _viewModel.GetFurnitureList();

        foreach (FurnitureData furnitureData in furnitureList)
        {
            bool isAdmitted = _viewModel.IsHeroAdmitted(furnitureData.HeroId);
            bool isWaiting = _viewModel.IsHeroWaitingAdmission(furnitureData.HeroId);

            if (!isAdmitted && !isWaiting)
            {
                continue;
            }

            bool canPurchase = isAdmitted && !isWaiting;

            GameObject slotObject = Instantiate(Prefab_FurnitureSlot, Transform_SlotParent);

            slotObject.name = $"Slot_{furnitureData.ID}";

            FurnitureSlotView slotView = slotObject.GetComponent<FurnitureSlotView>();

            if (slotView == null)
            {
                Debug.LogWarning($"FurnitureSlotView가 없습니다: {furnitureData.ID}");

                Destroy(slotObject);
                continue;
            }

            _activeSlots.Add(slotView);
            slotView.SetData(furnitureData, _viewModel, canPurchase);
        }
    }

    private void ClearSlots()
    {
        foreach (FurnitureSlotView slotView in _activeSlots)
        {
            if (slotView != null)
            {
                Destroy(slotView.gameObject);
            }
        }

        _activeSlots.Clear();
    }
}