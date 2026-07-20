using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : UIBase, IClosablePopup
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

    public void OpenConfirm(string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        _messageText.text = message;

        _confirmButton.gameObject.SetActive(true);
        _cancelButton.gameObject.SetActive(true);

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void OpenNotice(string message, Action onConfirm = null)
    {
        _onConfirm = onConfirm;
        _onCancel = null;

        _messageText.text = message;

        _confirmButton.gameObject.SetActive(true);
        _cancelButton.gameObject.SetActive(false);

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