using System;

public enum HeroRequestState
{
    None = 0,
    InProgress,
    Completed,
    Failed,
    Rewarded
}

[Serializable]
public class HeroRequestModel
{
    public string RequestId;
    public string HeroId;

    public int CreatedDay;
    public int CreatedHour;

    public int ExpireHour;

    public int CurrentProgress;
    public int State;
}
