using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TycoonPanelType
{
    None,
    Quest,
    Inventory,
    Pick,
    Hero,
    Construct,
    Market,
    Setting,
    Home
}

public class TycoonMainUI : UIBase
{
    [Serializable]
    public struct PanelStruct
    {
        public TycoonPanelType Type;
        public GameObject PanelObject;
    }

    [Serializable]
    public struct ButtonStruct
    {
        public TycoonPanelType Type;
        public Image BackgroundImage;
    }

    [Header("버튼")]
    [SerializeField] Button Button_Quest;
    [SerializeField] Button Button_Inventory;
    [SerializeField] Button Button_Pick;
    [SerializeField] Button Button_Hero;
    [SerializeField] Button Button_Construct;
    [SerializeField] Button Button_Market;
    [SerializeField] Button Button_Setting;
    [SerializeField] Button Button_Home;
    [SerializeField] List<ButtonStruct> ButtonList;

    [Header("패널")]
    [SerializeField] GameObject Panel_Quest;
    [SerializeField] GameObject Panel_Inventory;
    //[SerializeField] GameObject Panel_Pick;
    [SerializeField] GameObject Panel_Hero;
    [SerializeField] GameObject Panel_Construct;
    [SerializeField] GameObject Panel_Market;
    [SerializeField] List<PanelStruct> PanelList;

    [Header("텍스트")]
    [SerializeField] TextMeshProUGUI Text_Day;
    [SerializeField] TextMeshProUGUI Text_Gold;

    private void Awake()
    {
        Button_Quest.onClick.AddListener(OnClickQuest);
        Button_Inventory.onClick.AddListener(OnClickInventory);
        Button_Pick.onClick.AddListener(OnClickPick);
        Button_Hero.onClick.AddListener(OnClickHero);
        Button_Construct.onClick.AddListener(OnClickConstruct);
        Button_Market.onClick.AddListener(OnClickMarket);
        Button_Home.onClick.AddListener(OnClickHome);
    }

    private void OnEnable()
    {
        GameManager.Inst.CurrencyService.OnChangeCurrency += SetGoldText;
        SetGoldText();
        ChangePanel(TycoonPanelType.Quest);
    }

    private void OnDisable()
    {
        if (GameManager.Inst != null)
        {
            GameManager.Inst.CurrencyService.OnChangeCurrency -= SetGoldText;
        }
    }

    private void OnClickQuest()
    {
        ChangePanel(TycoonPanelType.Quest);
    }

    private void OnClickInventory()
    {
        ChangePanel(TycoonPanelType.Inventory);
    }

    private void OnClickPick()
    {
        //ChangePanel(TycoonPanelType.Pick);
    }

    private void OnClickHero()
    {
        ChangePanel(TycoonPanelType.Hero);
    }

    private void OnClickConstruct()
    {
        ChangePanel(TycoonPanelType.Construct);
    }

    private void OnClickMarket()  
    {
        ChangePanel(TycoonPanelType.Market);
    }

    private void ChangePanel(TycoonPanelType type)
    {
        foreach (PanelStruct panel in PanelList)
        {
            panel.PanelObject.SetActive(panel.Type == type);
        }

        UpdateButton(type).Forget();
    }

    private async UniTask UpdateButton(TycoonPanelType type)
    {
        foreach (ButtonStruct button in ButtonList)
        {
            if (button.Type == type)
            {
                button.BackgroundImage.sprite = await ResourceManager.Inst.LoadSprite("Image/Button/Select");
            }
            else
            {
                button.BackgroundImage.sprite = await ResourceManager.Inst.LoadSprite("Image/Button/Unselect");
            }
        }
    }

    private void OnClickHome()
    {
        UIManager.Inst.CloseTycoonMainUI();
    }

    private void SetDayText()
    {

    }

    private void SetGoldText()
    {
        if (Text_Gold == null)
        {
            Debug.LogError("[TycoonMainUI] Text_Gold 인스펙터 할당 안 됨!");
            return;
        }

        int gold = GameManager.Inst.CurrencyService.CurrentGold;
        Debug.Log($"[TycoonMainUI] SetGoldText 호출됨. Gold = {gold}");
        Text_Gold.text = $"{gold}";
    }
}
