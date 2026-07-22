using UnityEngine;
using UnityEngine.UI;

public class GameOver : UIBase
{
    [SerializeField] private Button Button_Close;

    private void Awake()
    {
        Button_Close.onClick.AddListener(OnClickClose);
    }

    private void OnClickClose()
    {
        UIManager.Inst.InitStartUI();
        UIManager.Inst.CloseGameOver();
    }
}
