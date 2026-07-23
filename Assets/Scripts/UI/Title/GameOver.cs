using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EndingType
{
    None,
    GameOver,
    Happy,
    Bad
}

public class GameOver : UIBase
{
    [SerializeField] private Button Button_Close;
    [SerializeField] private TextMeshProUGUI Text_Ending;

    public EndingType EndingType { get; set; }

    private void Awake()
    {
        Button_Close.onClick.AddListener(OnClickClose);
    }

    private void OnEnable()
    {
        if (EndingType == EndingType.GameOver)
        {
            Text_Ending.text = "Game Over";
            Text_Ending.color = Color.red;
        }
        else if (EndingType == EndingType.Happy)
        {
            Text_Ending.text = "Happy End";
            Text_Ending.color = Color.cyan;
        }
        else
        {
            Text_Ending.text = "Bad End";
            Text_Ending.color = Color.red;
        }
    }

    private void OnClickClose()
    {
        UIManager.Inst.InitStartUI();

        UIManager.Inst.CloseGameOver();
    }
}
