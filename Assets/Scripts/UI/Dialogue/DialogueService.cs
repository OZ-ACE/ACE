using System;

public class DialogueService
{
    public event Action<DialoguePlayContext> OnCompleteDialogue;

    public DialoguePlayContext CurrentPlayContext { get; private set; }

    public void BeginDialogue(DialoguePlayContext playContext)
    {
        CurrentPlayContext = playContext;
    }

    public void CompleteDialogue()
    {
        DialoguePlayContext completedContext = CurrentPlayContext;
        CurrentPlayContext = null;

        OnCompleteDialogue?.Invoke(completedContext);
    }

    public Dialogue GetDialogueData(string dialogueID)
    {
        return GameDataManager.Inst.GetData<Dialogue>(dialogueID);
    }

    public string GetNextDialogueID(string currentID)
    {
        Dialogue currentData = GameDataManager.Inst.GetData<Dialogue>(currentID);

        if (currentData == null)
        {
            return "0";
        }

        return currentData.NextID;
    }

    public void Release()
    {
        CurrentPlayContext = null;
        OnCompleteDialogue = null;
    }
}
