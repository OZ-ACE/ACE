using System;

[Serializable]
public class EpisodeData : GameDataBase
{
    public string EpisodeName;
    public string EpisodeDescription;
    public string DialogueId;

    public bool IsRepeatable;
    public int MaxTriggerCount;
}

[Serializable]
public class EpisodeConditionData : GameDataBase
{
    public string EpisodeId;
    public string ConditionType;
    public string TargetId;
    public int RequiredValue;

    public EpisodeConditionType GetConditionType()
    {
        if (Enum.TryParse(ConditionType, true, out EpisodeConditionType conditionType) == false)
        {
            return EpisodeConditionType.None;
        }

        return conditionType;
    }
}