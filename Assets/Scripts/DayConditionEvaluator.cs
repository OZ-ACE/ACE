public class DayConditionEvaluator : IEpisodeConditionEvaluator
{
    public EpisodeConditionType ConditionType => EpisodeConditionType.Day;

    public bool IsSatisfied(EpisodeConditionData conditionData, PlayerModel playerModel)
    {
        if (conditionData == null || playerModel == null)
        {
            return false;
        }

        return playerModel.Day >= conditionData.RequiredValue;
    }
}