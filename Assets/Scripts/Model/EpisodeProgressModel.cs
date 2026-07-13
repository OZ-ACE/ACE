using System;

[Serializable]
public class EpisodeProgressModel
{
    public string EpisodeDataId;
    public EpisodeProgressState State;
    public int TriggerCount;
    public bool IsNew;
}