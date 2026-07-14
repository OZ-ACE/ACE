public class AffinityConditionEvaluator : IEpisodeConditionEvaluator
{
    public EpisodeConditionType ConditionType => EpisodeConditionType.Affinity;

    public bool IsSatisfied(EpisodeConditionData conditionData, PlayerModel playerModel)
    {
        if (conditionData == null || playerModel == null || playerModel.HeroStats == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(conditionData.TargetId))
        {
            return false;
        }

        for (int i = 0; i < playerModel.HeroStats.Count; i++)
        {
            HeroStat heroStat = playerModel.HeroStats[i];

            if (heroStat == null || heroStat.HeroID != conditionData.TargetId)
            {
                continue;
            }

            return heroStat.Affection >= conditionData.RequiredValue;
        }

        return false;
    }
}