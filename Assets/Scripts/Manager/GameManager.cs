using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";

    public void SetDialogueID(string dialogueID)
    {
        CurrentDialogueID = dialogueID;
    }
}