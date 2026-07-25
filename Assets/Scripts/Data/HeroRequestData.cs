public class HeroRequestData : GameDataBase
{
    public string HeroId;
    public string RequestName;
    public string Description;

    public ScheduleState RequiredSchedule;
    public int RequiredHour;

    public int RewardAffection;
    public int RewardSatisfaction;
    public int TimeLimitMinute;
}