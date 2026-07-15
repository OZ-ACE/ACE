public class HeroAdmittedConditionEvaluator : IEpisodeConditionEvaluator
{
    public EpisodeConditionType ConditionType => EpisodeConditionType.HeroAdmitted;

    public bool IsSatisfied(EpisodeConditionData conditionData, PlayerModel playerModel)
    {
        if (conditionData == null || playerModel == null || playerModel.AdmittedHeroList == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(conditionData.TargetId))
        {
            return false;
        }

        for (int i = 0; i < playerModel.AdmittedHeroList.Count; i++)
        {
            AdmittedHeroModel admittedHero = playerModel.AdmittedHeroList[i];

            if (admittedHero == null)
            {
                continue;
            }

            if (admittedHero.HeroId == conditionData.TargetId)
            {
                return true;
            }
        }

        return false;
    }
}