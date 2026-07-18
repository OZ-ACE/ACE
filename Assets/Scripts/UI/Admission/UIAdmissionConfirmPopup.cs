using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdmissionConfirmPopup : UIBase, IClosablePopup
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    public void Open(string heroName, string roomName, Action onConfirm, Action onCancel)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        _messageText.text = $"{heroName} 영웅을\n{roomName}에 입소시키겠습니까?";

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void RequestClose()
    {
        Action onCancel = _onCancel;

        UIManager.Inst.ClosePopup(this);

        onCancel?.Invoke();
    }

    public void CloseImmediately()
    {
        _onConfirm = null;
        _onCancel = null;

        gameObject.SetActive(false);
    }

    private void BindButtonEvents()
    {
        if (_confirmButton != null)
        {
            _confirmButton.onClick.RemoveListener(OnClickConfirmButton);
            _confirmButton.onClick.AddListener(OnClickConfirmButton);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(OnClickCancelButton);
            _cancelButton.onClick.AddListener(OnClickCancelButton);
        }
    }

    private void UnbindButtonEvents()
    {
        if (_confirmButton != null)
        {
            _confirmButton.onClick.RemoveListener(OnClickConfirmButton);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(OnClickCancelButton);
        }
    }

    private void OnClickConfirmButton()
    {
        Action onConfirm = _onConfirm;

        UIManager.Inst.ClosePopup(this);

        onConfirm?.Invoke();
    }

    private void OnClickCancelButton()
    {
        RequestClose();
    }

    private void OnDestroy()
    {
        UnbindButtonEvents();

        _onConfirm = null;
        _onCancel = null;
    }
}