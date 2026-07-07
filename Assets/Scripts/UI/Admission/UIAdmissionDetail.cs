using TMPro;
using UnityEngine;

public class UIAdmissionDetail : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_HeroName;
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_Condition;

    public void SetData(AdmissionData admissionData)
    {
        if (admissionData == null)
        {
            Clear();
            return;
        }

        Hero heroData = GameDataManager.Inst.GetData<Hero>(admissionData.HeroId);

        Text_HeroName.text = heroData != null ? heroData.Name : admissionData.HeroId;
        Text_Title.text = admissionData.AdmissionTitle;
        Text_Description.text = admissionData.AdmissionDescription;
        Text_Condition.text = $"필요 방 등급 : {admissionData.RequiredRoomCondition}";
    }

    public void Clear()
    {
        Text_HeroName.text = string.Empty;
        Text_Title.text = string.Empty;
        Text_Description.text = string.Empty;
        Text_Condition.text = string.Empty;
    }
}