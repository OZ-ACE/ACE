using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class SaveUI : UIBase
{
    [SerializeField] private Button Button_Close;
    [SerializeField] private Transform Transform_SlotParent;
    [SerializeField] private GameObject Text_InfoText;

    private SaveViewModel _saveVM;

    private List<SaveSlot> _saveSlots = new List<SaveSlot>();

    private void Awake()
    {
        Button_Close.onClick.AddListener(OnClickClose);

        _saveVM = new SaveViewModel();
        BindViewModel(_saveVM);
    }

    public void BindViewModel(SaveViewModel saveVM)
    {
        _saveVM = saveVM;
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
                RefreshSlotViews().Forget();
                break;
        }
    }

    private async UniTask RefreshSlotViews()
    {
        int requiredSlot = _saveVM.ActiveSlotIndex.Count;

        if (_saveSlots.Count < requiredSlot)
        {
            int createCount = requiredSlot - _saveSlots.Count;

            for (int i = 0; i < createCount; i++)
            {
                GameObject prefab = await ResourceManager.Inst.InstantiateAsync("Prefabs/UI/SaveSlot", Transform_SlotParent);
                _saveSlots.Add(prefab.GetComponent<SaveSlot>());
            }
        }

        for (int i = 0; i < _saveSlots.Count; i++)
        {
            if (i < requiredSlot)
            {
                int slotIndex = _saveVM.ActiveSlotIndex[i];
                _saveSlots[i].gameObject.SetActive(true);
                _saveSlots[i].BindSlot(_saveVM, slotIndex);
            }
            else
            {
                _saveSlots[i].gameObject.SetActive(false);
            }
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