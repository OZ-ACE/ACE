using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] private Canvas Canvas_Background;
    [SerializeField] private Canvas Canvas_Main;
    [SerializeField] private Canvas Canvas_Content;
    [SerializeField] private Canvas Canvas_Popup;
    [SerializeField] private Canvas Canvas_Front;

    private Dictionary<UIType, UIBase> _createdUI = new Dictionary<UIType, UIBase>();
    private HashSet<UIType> _openedUI = new HashSet<UIType>();

    private void Start()
    {
        // 시작 시 처음 열리는 UI메서드
        this.InitStartUI();
    }

    public UIBase OpenUI(UIRootType root, UIType type, bool isActive = true)
    {
        UIBase openedUI = GetCreatedUI(root, type);

        if (openedUI == null)
        {
            Debug.LogWarning($"UIManager - UI 열 수 없음. Root : {root}, Type : {type}");

            return null;
        }

        if (_openedUI.Contains(type) == false)
        {
            openedUI.gameObject.SetActive(isActive);
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
        if (_createdUI.ContainsKey(type) == true)
        {
            return;
        }

        string path = this.GetUIPath(root, type);
        GameObject loadedObject = Resources.Load<GameObject>(path);

        if (loadedObject == null)
        {
            Debug.LogError($"UIManager - UI 프리팹을 찾을 수 없음. Path : Resources/{path}");
            return;
        }

        Transform rootTransform = GetRootTransform(root);

        if (rootTransform == null)
        {
            Debug.LogError($"UIManager - UI 루트를 찾을 수 없음. Root : {root}");
            return;
        }

        GameObject createdObject = Instantiate(loadedObject, rootTransform);

        UIBase uiBase = createdObject.GetComponent<UIBase>();

        if (uiBase == null)
        {
            Debug.LogError($"UIManager - 프리팹 루트에 UIBase 상속 컴포넌트가 없음. Type : {type}");
            Destroy(createdObject);
            return;
        }

        _createdUI.Add(type, uiBase);
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

        if (_createdUI.TryGetValue(type, out UIBase uiBase) == true)
        {
            return uiBase;
        }

        return null;
    }

    public UIBase IsOpened(UIType type)
    {
        if (_openedUI.Contains(type))
        {
            return _createdUI[type];
        }

        return null;
    }
}
