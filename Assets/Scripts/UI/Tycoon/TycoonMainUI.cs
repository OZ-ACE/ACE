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
    Hero,
    Construct,
    Furniture,
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
    [SerializeField] Button Button_Hero;
    [SerializeField] Button Button_Construct;
    [SerializeField] Button Button_Furniture;
    [SerializeField] Button Button_Setting;
    [SerializeField] Button Button_Home;
    [SerializeField] List<ButtonStruct> ButtonList;

    [Header("패널")]
    [SerializeField] GameObject Panel_Quest;
    [SerializeField] GameObject Panel_Inventory;
    [SerializeField] GameObject Panel_Hero;
    [SerializeField] GameObject Panel_Construct;
    [SerializeField] GameObject Panel_Furniture;
    [SerializeField] List<PanelStruct> PanelList;

    [Header("텍스트")]
    [SerializeField] TextMeshProUGUI Text_Day;
    [SerializeField] TextMeshProUGUI Text_Time;
    [SerializeField] TextMeshProUGUI Text_Gold;
    [SerializeField] TextMeshProUGUI Text_Memory;

    [Header("이미지")]
    [SerializeField] private Sprite Sprite_Select;
    [SerializeField] private Sprite Sprite_Unselect;

    public Action OnCloseSetting;

    private void Awake()
    {
        Button_Quest.onClick.AddListener(OnClickQuest);
        Button_Inventory.onClick.AddListener(OnClickInventory);
        Button_Hero.onClick.AddListener(OnClickHero);
        Button_Construct.onClick.AddListener(OnClickConstruct);
        Button_Furniture.onClick.AddListener(OnClickFurniture);
        Button_Setting.onClick.AddListener(OnClickSetting);
        Button_Home.onClick.AddListener(OnClickHome);

        OnCloseSetting += OnClickQuest;
    }

    private void OnDestroy()
    {
        OnCloseSetting -= OnClickQuest;
    }

    private void OnEnable()
    {
        GameManager.Inst.Services.CurrencyService.OnChangeCurrency += SetGoldText;
        GameManager.Inst.Services.CurrencyService.OnChangeCurrency += SetMemory;
        GameManager.Inst.Services.DayService.OnChangeDay += OnChangeDay;
        GameManager.Inst.Services.DayService.OnChangeHour += OnChangeHour;
        SetGoldText();
        SetMemory();
        SetDayText();
        ChangePanel(TycoonPanelType.Quest);

        GameManager.Inst.Services.DayService.StartTimer();
    }

    private void OnDisable()
    {
        if (GameManager.Inst != null)
        {
            GameManager.Inst.Services.CurrencyService.OnChangeCurrency -= SetGoldText;
            GameManager.Inst.Services.CurrencyService.OnChangeCurrency -= SetMemory;
            GameManager.Inst.Services.DayService.OnChangeDay -= OnChangeDay;
            GameManager.Inst.Services.DayService.OnChangeHour -= OnChangeHour;
        }
    }

    private void Update()
    {
        GameManager.Inst.Services.DayService.UpdateTimer(Time.deltaTime);
    }

    private void OnChangeDay(int day)
    {
        SetDayText();
    }

    private void OnChangeHour(int hour)
    {
        Text_Time.text = $"{hour:D2} : 00";
    }

    private void OnClickQuest()
    {
        ChangePanel(TycoonPanelType.Quest);
    }

    private void OnClickInventory()
    {
        ChangePanel(TycoonPanelType.Inventory);
    }

    private void OnClickHero()
    {
        ChangePanel(TycoonPanelType.Hero);
    }

    private void OnClickConstruct()
    {
        ChangePanel(TycoonPanelType.Construct);
    }

    private void OnClickFurniture()
    {
        ChangePanel(TycoonPanelType.Furniture);
    }

    private void OnClickSetting()
    {
        UIManager.Inst.OpenSettingPopup();
        ChangePanel(TycoonPanelType.None);
        UpdateButton(TycoonPanelType.Setting);
    }

    private void ChangePanel(TycoonPanelType type)
    {
        if (type == TycoonPanelType.None)
        {
            foreach (PanelStruct panel in PanelList)
            {
                panel.PanelObject.SetActive(false);
            }

            return;
        }

        foreach (PanelStruct panel in PanelList)
        {
            panel.PanelObject.SetActive(panel.Type == type);
        }

        UpdateButton(type);
    }

    private void UpdateButton(TycoonPanelType type)
    {
        foreach (ButtonStruct button in ButtonList)
        {
            if (button.Type == type)
            {
                button.BackgroundImage.sprite = Sprite_Select;
            }
            else
            {
                button.BackgroundImage.sprite = Sprite_Unselect;
            }
        }
    }

    private void OnClickHome()
    {
        ObjectManager.Inst.DestroyHeroAndMap();         // 테스트

        UIManager.Inst.OpenTitleUI();
        UIManager.Inst.CloseTycoonMainUI();
    }

    private void SetDayText()
    {
        int day = SaveManager.Inst.CurrentPlayerModel.Day;
        Text_Day.text = $"Day {day}";
    }

    private void SetGoldText()
    {
        if (Text_Gold == null)
        {
            return;
        }

        int gold = GameManager.Inst.Services.CurrencyService.CurrentGold;
        Text_Gold.text = $"{gold}";
    }

    private void SetMemory()
    {
        if (Text_Memory == null)
        {
            return;
        }

        int memory = GameManager.Inst.Services.CurrencyService.CurrentMemoryFragment;
        Text_Memory.text = $"{memory}";
    }
}
