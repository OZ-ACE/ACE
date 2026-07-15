using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class SaveUI : UIBase
{
    [SerializeField] private Button Button_Close;
    [SerializeField] private SaveSlot Prefab_Slot;
    [SerializeField] private Transform Transform_SlotParent;
    [SerializeField] private GameObject Text_InfoText;

    private SaveViewModel _saveVM;

    private ObjectPooling<SaveSlot> _slotPool;
    private List<SaveSlot> _saveSlots = new List<SaveSlot>();

    private void Awake()
    {
        Button_Close.onClick.AddListener(OnClickClose);

        _saveVM = SaveManager.Inst.SaveVM;
        _saveVM.InvokeOnceOnInit();

        _slotPool = new ObjectPooling<SaveSlot>(Prefab_Slot, Transform_SlotParent, 5, 20);
    }

    private void OnEnable()
    {
        BindViewModel();
        RefreshSlotViews();
    }

    public void BindViewModel()
    {
        _saveVM.PropertyChanged += OnSavePropertyChanged;
        _saveVM.RefreshActiveSlots();
    }

    private void OnDestroy()
    {
        if (_saveVM != null)
        {
            _saveVM.PropertyChanged -= OnSavePropertyChanged;
        }
    }

    private void OnSavePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SaveViewModel.ActiveSlotIndex):
                RefreshSlotViews();
                break;
        }
    }

    private void RefreshSlotViews()
    {
        int requiredSlot = _saveVM.ActiveSlotIndex.Count;

        foreach (SaveSlot slot in _saveSlots)
        {
            _slotPool.Release(slot);
        }

        _saveSlots.Clear();

        for (int i = 0; i < requiredSlot; i++)
        {
            SaveSlot slot = _slotPool.Get();
            _saveSlots.Add(slot);

            int slotIndex = _saveVM.ActiveSlotIndex[i];
            slot.BindSlot(slotIndex);
        }

        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        if (_saveVM.ActiveSlotIndex.Count == 0)
        {
            Text_InfoText.SetActive(true);
        }
        else
        {
            Text_InfoText.SetActive(false);
        }
    }

    private void OnClickClose()
    {
        UIManager.Inst.CloseSaveUI();
    }
}