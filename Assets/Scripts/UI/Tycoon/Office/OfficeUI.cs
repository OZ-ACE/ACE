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

    [Header("[임시] 테스트 버튼")]
    [SerializeField] Button Button_AddFragment;
    [SerializeField] Button Button_ExchangeFragment;


    private void Awake()
    {
        Button_NextDay.onClick.AddListener(OnClickNextDay);
        Button_Shop.onClick.AddListener(OnClickShop);
        Button_Battle.onClick.AddListener(OnClickBattle);
        Button_Admission.onClick.AddListener(OnClickAdmission);
        Button_Close.onClick.AddListener(OnClickClose);
        Button_AddFragment.onClick.AddListener(OnClickAddFragment);
        Button_ExchangeFragment.onClick.AddListener(OnClickExchangeFragment);
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

    // [임시] 테스트용 — 파편 100 지급
    private void OnClickAddFragment()
    {
        GameManager.Inst.Services.CurrencyService.AddMemoryFragment(100);
    }

    // [임시] 테스트용 — 보유 파편 전량을 Gold로 교환
    private void OnClickExchangeFragment()
    {
        int fragment = GameManager.Inst.Services.CurrencyService.CurrentMemoryFragment;
        GameManager.Inst.Services.CurrencyService.TryExchangeFragmentToGold(fragment);
    }
}