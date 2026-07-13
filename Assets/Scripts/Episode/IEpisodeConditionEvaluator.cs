public interface IEpisodeConditionEvaluator
{
    EpisodeConditionType ConditionType { get; }

    bool IsSatisfied(EpisodeConditionData conditionData, PlayerModel playerModel);
}