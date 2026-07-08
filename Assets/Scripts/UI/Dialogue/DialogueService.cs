using UnityEngine;

public class DialogueService
{
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
}
