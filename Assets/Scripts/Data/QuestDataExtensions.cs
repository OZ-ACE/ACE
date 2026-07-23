public static class QuestDataExtensions
{
    //조건 타입 문자열을 enum으로 변환
    public static QuestConditionType GetConditionType(this QuestData quest)
    {
        switch (quest.ConditionType)
        {
            case "BuildRoom": return QuestConditionType.BuildRoom;
            case "PurchaseItem": return QuestConditionType.PurchaseItem;
            case "AdvanceDay": return QuestConditionType.AdvanceDay;
            case "ReachGold": return QuestConditionType.ReachGold;
            case "AdmitHero": return QuestConditionType.AdmitHero;
            case "Schedule": return QuestConditionType.Schedule;
            case "Battle": return QuestConditionType.Battle;
            case "Digging": return QuestConditionType.Digging;
        }

        return QuestConditionType.None;
    }

    //보상 타입 문자열을 enum으로 변환
    public static QuestRewardType GetRewardType(this QuestData quest)
    {
        switch (quest.RewardType)
        {
            case "Gold": return QuestRewardType.Gold;
            case "MemoryFragment": return QuestRewardType.MemoryFragment;
        }
        return QuestRewardType.None;
    }

    //누적이 아니라 현재 상태를 조회해야 하는 조건 판단
    public static bool IsStateCondition(this QuestData quest)
    {
        return quest.GetConditionType() == QuestConditionType.ReachGold;
    }

    //선행퀘 판단
    public static bool HasRequiredQuest(this QuestData quest)
    {
        return string.IsNullOrEmpty(quest.RequiredQuestID) == false;
    }
}