using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NamePopup : UIBase
{
    [SerializeField] private Button Button_Close;
    [SerializeField] private Button Button_Confirm;
    [SerializeField] private TMP_InputField InputField_Name;

    private void Awake()
    {
        Button_Close.onClick.AddListener(OnClickClose);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
    }

    private void OnEnable()
    {
        InputField_Name.text = string.Empty;
    }

    private void OnClickClose()
    {
        UIManager.Inst.ClsoeNamePopup();
    }

    private void OnClickConfirm()
    {
        SaveManager.Inst.SaveVM.CreateAndSavePlayer(InputField_Name.text);

        UIManager.Inst.ClsoeNamePopup();
        UIManager.Inst.OpenTycoonMainUI();
        UIManager.Inst.CloseTitleUI();
    }
}
