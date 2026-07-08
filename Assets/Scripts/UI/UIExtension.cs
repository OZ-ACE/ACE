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
    TycoonMainUI
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

    public static void StartInit(this UIManager uiManager)
    {
        uiManager.OpenTycoonMainUI();
    }

    public static void OpenTycoonMainUI(this UIManager uIManager)
    {
        uIManager.OpenUI(UIRootType.Main, UIType.TycoonMainUI);
    }

    public static void CloseTycoonMainUI( this UIManager uIManager)
    {
        uIManager.CloseUI(UIType.TycoonMainUI);
    }
}