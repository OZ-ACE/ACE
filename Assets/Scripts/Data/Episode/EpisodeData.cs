using System;

[Serializable]
public class EpisodeData : GameDataBase
{
    public string EpisodeName;
    public string EpisodeDescription;
    public string DialogueId;

    public string Category;
    public string PlaybackType;
    public bool IsArchiveVisible;

    public bool IsRepeatable;
    public int MaxTriggerCount;

    public EpisodeCategory GetCategory()
    {
        if (Enum.TryParse(Category, true, out EpisodeCategory category) == false)
        {
            return EpisodeCategory.None;
        }

        return category;
    }

    public EpisodePlaybackType GetPlaybackType()
    {
        if (Enum.TryParse(PlaybackType, true, out EpisodePlaybackType playbackType) == false)
        {
            return EpisodePlaybackType.None;
        }

        return playbackType;
    }
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