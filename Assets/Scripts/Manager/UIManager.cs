using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] private Canvas Canvas_Background;
    [SerializeField] private Canvas Canvas_Main;
    [SerializeField] private Canvas Canvas_Content;
    [SerializeField] private Canvas Canvas_Popup;
    [SerializeField] private Canvas Canvas_Front;

    private readonly Stack<IClosablePopup> _popupStack = new Stack<IClosablePopup>();

    private Dictionary<UIType, UIBase> _createdUI = new Dictionary<UIType, UIBase>();
    private HashSet<UIType> _openedUI = new HashSet<UIType>();

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame == false )
        {
            return;
        }

        RequestCloseTopPopup();
    }

    public UIBase OpenUI(UIRootType root, UIType type, bool isActive = true)
    {
        UIBase openedUI = GetCreatedUI(root, type);

        if (openedUI == null)
        {
            Debug.LogError($"UI를 열 수 없음. UIType : {type}");
            return null;
        }

        if (_openedUI.Contains(type) == false)
        {
            openedUI.gameObject.SetActive(isActive);
            _openedUI.Add(type);
        }
        else if (isActive == true && openedUI.gameObject.activeSelf == false)
        {
            openedUI.gameObject.SetActive(true);
        }

        return openedUI;
    }

    public void CloseUI(UIType type)
    {
        if (_openedUI.Contains(type) == false)
        {
            return;
        }

        if (_createdUI.TryGetValue(type, out UIBase openedUI) == false)
        {
            _openedUI.Remove(type);
            return;
        }

        if (openedUI != null)
        {
            openedUI.gameObject.SetActive(false);
        }

        _openedUI.Remove(type);
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
        GameObject loadedObj = Resources.Load<GameObject>(path);

        if (loadedObj == null)
        {
            Debug.LogError($"UI 프리팹을 찾을 수 없음. Path : {path}");
            return;
        }

        Transform rootTransform = GetRootTransform(root);

        if (rootTransform == null)
        {
            Debug.LogError($"UI Root를 찾을 수 없음. Root : {root}");
            return;
        }

        GameObject createdObject = Instantiate(loadedObj, rootTransform);

        UIBase uiBase = createdObject.GetComponent<UIBase>();

        if (uiBase == null)
        {
            Debug.LogError($"UIBase 컴포넌트가 없음. UIType : {type}");
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

        if (_createdUI.TryGetValue(type, out UIBase createdUI) == false)
        {
            return null;
        }

        return createdUI;
    }

    public UIBase IsOpened(UIType type)
    {
        if (_openedUI.Contains(type))
        {
            return _createdUI[type];
        }

        return null;
    }

    public UIBase OpenPopup(UIType type)
    {
        UIBase popupUI = OpenUI(UIRootType.Popup, type);

        if (popupUI == null)
        {
            Debug.LogError($"팝업 생성에 실패함. UIType : {type}");
            return null;
        }

        IClosablePopup closablePopup = popupUI as IClosablePopup;

        if (closablePopup == null)
        {
            Debug.LogError($"팝업이 IClosablePopup 구현하지 않음. UIType : {type}");
            return popupUI;
        }

        if (_popupStack.Contains(closablePopup) == false)
        {
            _popupStack.Push(closablePopup);
        }

        popupUI.transform.SetAsLastSibling();

        return popupUI;
    }

    public void RequestCloseTopPopup()
    {
        RemoveInvalidPopupEntries();

        if (_popupStack.Count <= 0)
        {
            return;
        }

        IClosablePopup topPopup = _popupStack.Peek();
        topPopup.RequestClose();
    }

    public void ClosePopup(IClosablePopup popup)
    {
        if (popup == null)
        {
            return;
        }

        RemoveInvalidPopupEntries();

        if (_popupStack.Count <= 0)
        {
            popup.CloseImmediately();
            return;
        }

        if (object.ReferenceEquals(_popupStack.Peek(), popup) == false)
        {
            Debug.LogWarning("최상단 팝업이 아니므로 닫을 수 없음.");
            return;
        }

        _popupStack.Pop();
        popup.CloseImmediately();
    }

    private void RemoveInvalidPopupEntries()
    {
        while (_popupStack.Count > 0)
        {
            IClosablePopup topPopup = _popupStack.Peek();
            UnityEngine.Object unityObject = topPopup as UnityEngine.Object;

            if (unityObject == null)
            {
                _popupStack.Pop();
                continue;
            }

            break;
        }
    }
}
