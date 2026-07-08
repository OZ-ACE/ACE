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
    SaveUI,
    DialogueUI,
    TycoonUI
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
        uiManager.OpenDialogueUI();
    }

    public static void OpenDialogueUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Content, UIType.DialogueUI);
    }

    public static void CloseDialogueUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.DialogueUI);
    }

    public static void OpenSaveUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.Main, UIType.SaveUI);
    }

    public static void CloseSaveUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.SaveUI);
    }
}