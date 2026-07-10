using UnityEngine;
using UnityEngine.UI;

public class TitleUI : UIBase
{
    [SerializeField] private Button Button_NewGame;
    [SerializeField] private Button Button_LoadGame;
    [SerializeField] private Button Button_Setting;
    [SerializeField] private Button Button_Quit;

    private void Awake()
    {
        Button_NewGame.onClick.AddListener(OnClickNewGame);
        Button_LoadGame.onClick.AddListener(OnClickLoadGame);
        Button_Setting.onClick.AddListener(OnClickSetting);
        Button_Quit.onClick.AddListener(OnClickQuit);
    }

    private void OnClickNewGame()
    {
        int nextSlotIndex = SaveManager.Inst.GetEmptySlot();
        SaveManager.Inst.SaveVM.PrepareNewSave(nextSlotIndex);

        UIManager.Inst.OpenNamePopup();
    }

    private void OnClickLoadGame()
    {
        UIManager.Inst.OpenSaveUI();
    }

    private void OnClickSetting()
    {
        UIManager.Inst.OpenSettingPopup();
    }

    private void OnClickQuit()
    {
        Application.Quit();
    }
}
