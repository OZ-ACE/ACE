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
    private HashSet<int> _createdSlot = new HashSet<int>();

    private void Awake()
    {
        Button_Close.onClick.AddListener(OnClickClose);
    }

    public void BindViewModel(SaveViewModel saveVM)
    {
        _saveVM = saveVM;
        _saveVM.PropertyChanged += OnSavePropertyChanged;
        _saveVM.InvokeOnceOnInit();
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
        ClearAllSlot();

        foreach (int slotIndex in _saveVM.ActiveSlotIndex)
        {
            if (_createdSlot.Contains(slotIndex) == false)
            {
                GameObject prefab = await ResourceManager.Inst.InstantiateAsync("Prefabs/UI/SaveSlot", Transform_SlotParent);
            }
        }

        UpdateInfoText();
    }

    private void ClearAllSlot()
    {
        foreach (Transform child in Transform_SlotParent)
        {
            Destroy(child.gameObject);
        }

        _createdSlot.Clear();
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