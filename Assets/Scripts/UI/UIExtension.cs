using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public enum UIRootType
{
    Background,
    Main,
    Content,
    Popup,
    Front
}

public enum UIType
{
    None,
    OverlayScreen,
    TitleUI,
    NamePopup,
    SettingPopup,
    SaveUI,
    LoadingUI,
    DialogueUI,
    TycoonMainUI,
    UIAdmissionPopup,
    BattleMainUI,
    OfficeUI,
    ShopUI,
    SettlementUI,
    UIEpisodeArchive
}

public static class UIExtension
{
    public static string GetUIPath (this UIManager uiManager, UIRootType root, UIType type)
    {
        string path = string.Empty;

        path = $"Prefabs/UI/{root}/{type}";
        return path;
    }

    // 이 밑에다가 UI별로 Open 및 Close 함수 작성해주세요.
    public static void InitStartUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Front, UIType.OverlayScreen);
        uiManager.OpenTitleUI();

        AdmissionManager.Inst.Initialize();
    }

    public static void OpenTitleUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Main, UIType.TitleUI);
    }

    public static void CloseTitleUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.TitleUI);
    }

    public static void OpenLoadingUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Popup, UIType.LoadingUI);
    }

    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.LoadingUI);
    }

    public static void OpenNamePopup(this UIManager uIManager)
    {
       uIManager.OpenUI(UIRootType.Popup, UIType.NamePopup);
    }

    public static void CloseNamePopup(this UIManager uIManager)
    {
        uIManager.CloseUI(UIType.NamePopup);
    }

    public static void OpenSettingPopup(this UIManager uIManager)
    {
        uIManager.OpenUI(UIRootType.Popup, UIType.SettingPopup);
    }

    public static void CloseSettingPopup(this UIManager uIManager)
    {
        uIManager.CloseUI(UIType.SettingPopup);
    }

    public static void OpenDialogueUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Main, UIType.DialogueUI);
    }

    public static void CloseDialogueUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.DialogueUI);
    }

    public static void OpenSaveUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Popup, UIType.SaveUI);
    }

    public static void CloseSaveUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.SaveUI);
    }

    public static void OpenTycoonMainUI(this UIManager uIManager)
    {
        uIManager.OpenUI(UIRootType.Main, UIType.TycoonMainUI);
        ObjectManager.Inst.CreateBuildGridView();
        ObjectManager.Inst.SpawnHero("hero_03").Forget();
    }

    public static void CloseTycoonMainUI(this UIManager uIManager)
    {
        uIManager.CloseUI(UIType.TycoonMainUI);
    }

    public static void OpenAdmissionPopup(this UIManager uiManager)
    {
        Debug.Log("팝업 열기");

        UIBase uiBase = uiManager.OpenUI(UIRootType.Popup, UIType.UIAdmissionPopup);

        if (uiBase == null)
        {
            Debug.LogWarning("UIAdmissionPopup 생성 실패");
            return;
        }

        if (uiBase is UIAdmissionPopup admissionPopup)
        {
            admissionPopup.Initialize();
        }
    }

    public static void CloseAdmissionPopup(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.UIAdmissionPopup);
    }

    public static void OpenBattleMainUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Main, UIType.BattleMainUI);
    }

    public static void CloseBattleMainUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.BattleMainUI);
    }

    public static void OpenOfficeUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Popup, UIType.OfficeUI);
    }

    public static void CloseOfficeUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.OfficeUI);
    }

    public static void OpenShopUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Popup, UIType.ShopUI);
    }
    public static void CloseShopUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.ShopUI);
    }

    public static void OpenSettlementUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Popup, UIType.SettlementUI);
    }
    public static void CloseSettlementUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.SettlementUI);
    }









    public static void OpenEpisodeArchive(this UIManager uiManager)
    {
        UIBase uiBase = uiManager.OpenUI(UIRootType.Content, UIType.UIEpisodeArchive);

        if (uiBase == null)
        {
            Debug.LogWarning("UIEpisodeArchive 생성 실패");
            return;
        }

        if (uiBase is UIEpisodeArchive episodeArchive)
        {
            EpisodeArchiveViewModel viewModel = new EpisodeArchiveViewModel(GameManager.Inst.Services.EpisodeService);
            episodeArchive.Bind(viewModel);
        }
    }

    public static void CloseEpisodeArchive(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.UIEpisodeArchive);
    }
}