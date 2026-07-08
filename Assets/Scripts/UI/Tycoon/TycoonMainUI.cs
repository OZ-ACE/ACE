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
    Construct
}

public class TycoonMainUI : UIBase
{
    [Serializable]
    public struct Panel
    {
        public TycoonPanelType Type;
        public GameObject PanelObject;
    }

    [Header("버튼")]
    [SerializeField] Button Button_Quest;
    [SerializeField] Button Button_Inventory;
    [SerializeField] Button Button_Pick;
    [SerializeField] Button Button_Hero;
    [SerializeField] Button Button_Construct;
    [SerializeField] Button Button_Setting;
    [SerializeField] Button Button_Home;

    [Header("패널")]
    [SerializeField] GameObject Panel_Quest;
    [SerializeField] GameObject Panel_Inventory;
    //[SerializeField] GameObject Panel_Pick;
    [SerializeField] GameObject Panel_Hero;
    [SerializeField] GameObject Panel_Construct;
    [SerializeField] List<Panel> PanelList;

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
        Button_Home.onClick.AddListener(OnClickHome);
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

    private void ChangePanel(TycoonPanelType type)
    {
        foreach (Panel panel in PanelList)
        {
            panel.PanelObject.SetActive(panel.Type == type);
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
        
    }
}
