using System;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";
    public ICurrencyService CurrencyService { get; private set; }

    public Action<float> OnChangeBrightness;

    protected override void Awake()
    {
        base.Awake();
        CurrencyService = new CurrencyService();
    }

    public void SetDialogueID(string dialogueID)
    {
        CurrentDialogueID = dialogueID;
    }
}