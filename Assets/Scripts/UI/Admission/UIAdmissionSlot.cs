using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdmissionSlot : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_HeroName;
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private GameObject Object_Selected;
    [SerializeField] private Button Button_Select;

    private AdmissionData _admissionData;
    private Action<string> _onClickSlot;

    public void SetData(AdmissionData admissionData, Action<string> onClickSlot)
    {
        _admissionData = admissionData;
        _onClickSlot = onClickSlot;

        Refresh();
        Button_Select.onClick.AddListener(OnClickSelect);
    }

    public void Refresh()
    {
        if (_admissionData == null)
        {
            return;
        }

        Hero heroData = GameDataManager.Inst.GetData<Hero>(_admissionData.HeroId);

        Text_HeroName.text = heroData != null ? heroData.Name : _admissionData.HeroId;
        Text_Title.text = _admissionData.AdmissionTitle;
    }

    public void ChangeSelected(bool isSelected)
    {
        Object_Selected.SetActive(isSelected);
    }

    private void OnClickSelect()
    {
        if (_admissionData == null)
        {
            return;
        }

        _onClickSlot?.Invoke(_admissionData.ID);
    }
}