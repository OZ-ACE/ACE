using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";
    public ICurrencyService CurrencyService { get; private set; }

    public void SetDialogueID(string dialogueID)
    {
        CurrentDialogueID = dialogueID;
    }

    protected override void Awake()
    {
        base.Awake();
        CurrencyService = new CurrencyService();
    }

}