public enum QuestConditionType
{
    None,
    BuildRoom,      // 방 건설 (누적)
    PurchaseItem,   // 아이템 구매 (누적)
    AdvanceDay,     // 날짜 진행 (누적)
    ReachGold       // 보유 골드 달성 (상태)
}

public enum QuestRewardType
{
    None,
    Gold,
    MemoryFragment
}

// 값 추가는 반드시 맨 끝에 (세이브에 int로 저장됨)
public enum QuestState
{
    Locked,
    InProgress,
    Completed,   // 조건 달성, 보상 미수령
    Rewarded
}