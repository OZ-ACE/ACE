using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] Canvas Canvas_Background;
    [SerializeField] Canvas Canvas_Main;
    [SerializeField] Canvas Canvas_Content;
    [SerializeField] Canvas Canvas_Popup;
    [SerializeField] Canvas Canvas_Front;

    private Dictionary<UIType, UIBase> _createdUI = new Dictionary<UIType, UIBase>();
    private HashSet<UIType> _openedUI = new HashSet<UIType>();

    private void Start()
    {
        // 시작 시 처음 열리는 UI메서드
        this.StartInit();
    }

    public UIBase OpenUI(UIRootType root, UIType type, bool isActive = true)
    {
        var openedUI = GetCreatedUI(root, type);

        bool isSetActive = (isActive == true);

        if (_openedUI.Contains(type) == false)
        {
            openedUI.gameObject.SetActive(isSetActive);
            _openedUI.Add(type);
        }

        return openedUI;
    }

    public void CloseUI(UIType type)
    {
        if (_openedUI.Contains(type) == true)
        {
            var openedUI = _createdUI[type];
            openedUI.gameObject.SetActive(false);
            _openedUI.Remove(type);
        }
    }

    private Transform GetRootTransform(UIRootType rootType)
    {
        Transform root = null;

        switch (rootType)
        {
            case UIRootType.Background:
                root = Canvas_Background.transform;
                break;
            
            case UIRootType.Main:
                root = Canvas_Main.transform;
                break;

            case UIRootType.Content:
                root = Canvas_Content.transform;
                break;

            case UIRootType.Popup:
                root = Canvas_Popup.transform;
                break;

            case UIRootType.Front:
                root = Canvas_Front.transform;
                break;
        }

        return root;
    }

    private void CreateUI(UIRootType root, UIType type)
    {
        if (_createdUI.ContainsKey(type) == false)
        {
            var path = this.GetUIPath(root, type);
            GameObject loadedObj = (GameObject)Resources.Load(path);
            Transform transform = GetRootTransform(root);
            GameObject gobj = Instantiate(loadedObj, transform);

            if (gobj != null)
            {
                var uiBase = gobj.GetComponent<UIBase>();
                _createdUI.Add(type, uiBase);
            }
        }
    }

    public UIBase GetOpenedUI(UIRootType root, UIType type)
    {
        return GetCreatedUI(root, type);
    }

    private UIBase GetCreatedUI(UIRootType root, UIType type)
    {
        if (_createdUI.ContainsKey(type) == false)
        {
            CreateUI(root, type);
        }

        return _createdUI[type];
    }
}
