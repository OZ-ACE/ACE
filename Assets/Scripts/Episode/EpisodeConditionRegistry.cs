using System.Collections.Generic;

public class EpisodeConditionRegistry
{
    private readonly Dictionary<EpisodeConditionType, IEpisodeConditionEvaluator> _evaluatorDict
        = new Dictionary<EpisodeConditionType, IEpisodeConditionEvaluator>();

    public EpisodeConditionRegistry()
    {
        RegisterEvaluator(new DayConditionEvaluator());
    }

    /// <summary>
    /// 조건 판정기를 타입별로 등록한다.
    /// </summary>
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

    /// <summary>
    /// 조건 타입에 해당하는 조건 판정기를 반환한다.
    /// </summary>
    public IEpisodeConditionEvaluator GetEvaluator(EpisodeConditionType conditionType)
    {
        if (_evaluatorDict.TryGetValue(
            conditionType,
            out IEpisodeConditionEvaluator evaluator) == true)
        {
            return evaluator;
        }

        return null;
    }
}