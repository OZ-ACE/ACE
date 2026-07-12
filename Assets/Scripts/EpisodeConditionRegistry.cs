using System.Collections.Generic;

public class EpisodeConditionRegistry
{
    private readonly Dictionary<EpisodeConditionType, IEpisodeConditionEvaluator> _evaluatorDict = new Dictionary<EpisodeConditionType, IEpisodeConditionEvaluator>();

    public EpisodeConditionRegistry()
    {
        RegisterEvaluator(new DayConditionEvaluator());
    }

    // 조건 판정기를 타입별로 등록한다.
    public void RegisterEvaluator(IEpisodeConditionEvaluator evaluator)
    {
        if (evaluator == null)
        {
            return;
        }

        if (_evaluatorDict.ContainsKey(evaluator.ConditionType) == true)
        {
            return;
        }

        _evaluatorDict.Add(evaluator.ConditionType, evaluator);
    }

    public IEpisodeConditionEvaluator GetEvaluator(EpisodeConditionType conditionType)
    {
        if (_evaluatorDict.TryGetValue(conditionType, out IEpisodeConditionEvaluator evaluator) == true)
        {
            return evaluator;
        }

        return null;
    }
}