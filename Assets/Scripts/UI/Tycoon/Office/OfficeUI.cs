using UnityEngine;
using UnityEngine.UI;

public class OfficeUI : UIBase
{
    [Header("버튼")]
    [SerializeField] Button Button_NextDay;
    [SerializeField] Button Button_Shop;
    [SerializeField] Button Button_Battle;
    [SerializeField] Button Button_Admission;
    [SerializeField] Button Button_Close;

    private void Awake()
    {
        Button_NextDay.onClick.AddListener(OnClickNextDay);
        Button_Shop.onClick.AddListener(OnClickShop);
        Button_Battle.onClick.AddListener(OnClickBattle);
        Button_Admission.onClick.AddListener(OnClickAdmission);
        Button_Close.onClick.AddListener(OnClickClose);
    }

    private void OnClickNextDay()
    {
        bool success = GameManager.Inst.Services.DayService.TryAdvanceDay();
        if (success == false)
        {
            return;
        }
        // TODO: 마감 정산 UI 표시
    }

    private void OnClickShop()
    {
        UIManager.Inst.OpenShopUI();
    }

    private void OnClickBattle()
    {
        GameManager.Inst.Services.DayService.MarkBattleDone();
    }

    private void OnClickAdmission()
    {
        UIManager.Inst.OpenAdmissionPopup();
    }

    private void OnClickClose()
    {
        UIManager.Inst.CloseOfficeUI();
    }
}