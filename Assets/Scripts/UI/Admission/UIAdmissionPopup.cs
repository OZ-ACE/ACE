using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAdmissionPopup : UIBase
{
    [SerializeField] private Transform Transform_Content;
    [SerializeField] private UIAdmissionSlot Prefab_AdmissionSlot;
    [SerializeField] private UIAdmissionDetail UI_AdmissionDetail;
    [SerializeField] private Button Button_Close;

    private readonly Dictionary<string, UIAdmissionSlot> _slotDict = new Dictionary<string, UIAdmissionSlot>();

    private AdmissionPopupViewModel _viewModel;

    public void Initialize()
    {
        _viewModel = new AdmissionPopupViewModel();
        _viewModel.Initialize();

        CreateSlots();
        UI_AdmissionDetail.Clear();
    }

    private void CreateSlots()
    {
        _slotDict.Clear();

        foreach (AdmissionData admissionData in _viewModel.AdmissionDatas)
        {
            UIAdmissionSlot slot = Instantiate(Prefab_AdmissionSlot, Transform_Content);
            slot.SetData(admissionData, SelectAdmissionSlot);
            slot.ChangeSelected(false);

            _slotDict.Add(admissionData.ID, slot);
        }
    }

    private void SelectAdmissionSlot(string id)
    {
        _viewModel.SelectAdmissionData(id);

        foreach (KeyValuePair<string, UIAdmissionSlot> pair in _slotDict)
        {
            pair.Value.ChangeSelected(pair.Key == id);
        }

        UI_AdmissionDetail.SetData(_viewModel.SelectedAdmissionData);
    }
}