public static class TutorialDataExtensions
{
    public static TutorialTriggerType GetTriggerType(this Tutorial data)
    {
        switch (data.TriggerType)
        {
            case "TycoonEnter": return TutorialTriggerType.TycoonEnter;
            case "QuestReward": return TutorialTriggerType.QuestReward;
        }
        return TutorialTriggerType.None;
    }
}