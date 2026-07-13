using System;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeService
{
    public event Action<EpisodeData> OnTriggerEpisode;

    private readonly EpisodeConditionRegistry _conditionRegistry;
    private readonly Dictionary<string, List<EpisodeConditionData>> _conditionDict = new Dictionary<string, List<EpisodeConditionData>>();
    private readonly Dictionary<string, EpisodeProgressModel> _progressDict = new Dictionary<string, EpisodeProgressModel>();

    public EpisodeService()
    {
        _conditionRegistry = new EpisodeConditionRegistry();
    }

    public void Initialize()
    {
        CacheEpisodeConditions();
        CacheEpisodeProgress();
    }

    public void Release()
    {
        OnTriggerEpisode = null;

        _conditionDict.Clear();
        _progressDict.Clear();
    }

    public bool TryTriggerEpisode(string episodeDataId)
    {
        if (string.IsNullOrEmpty(episodeDataId) == true)
        {
            return false;
        }

        EpisodeData episodeData = GameDataManager.Inst.GetData<EpisodeData>(episodeDataId);

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (episodeData == null || playerModel == null)
        {
            Debug.LogWarning($"EpisodeService - 에피소드 or 플레이어 데이터를 찾을 수 없음. EpisodeId : {episodeDataId}");
            return false;
        }

        EpisodeProgressModel progressModel = GetOrCreateProgressModel(episodeDataId, playerModel);

        if (CanTriggerEpisode(episodeData, progressModel) == false)
        {
            return false;
        }

        if (AreAllConditionsSatisfied(episodeDataId, playerModel) == false)
        {
            return false;
        }

        UpdateEpisodeProgress(episodeData, progressModel);

        OnTriggerEpisode?.Invoke(episodeData);

        Debug.Log($"EpisodeService - 에피소드 발생 : {episodeData.EpisodeName}");
        return true;
    }

    private bool AreAllConditionsSatisfied(string episodeDataId, PlayerModel playerModel)
    {
        if (string.IsNullOrEmpty(episodeDataId) == true || playerModel == null)
        {
            return false;
        }

        if (_conditionDict.TryGetValue(episodeDataId, out List<EpisodeConditionData> conditions) == false)
        {
            Debug.LogWarning($"EpisodeService - 조건 데이터를 찾을 수 없음. EpisodeId : {episodeDataId}");
            return false;
        }

        if (conditions.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < conditions.Count; i++)
        {
            EpisodeConditionData conditionData = conditions[i];

            EpisodeConditionType conditionType = conditionData.GetConditionType();

            IEpisodeConditionEvaluator evaluator = _conditionRegistry.GetEvaluator(conditionType);

            if (evaluator == null)
            {
                Debug.LogWarning($"EpisodeService - 조건 판정기를 찾을 수 없음. ConditionType : {conditionType}");
                return false;
            }

            if (evaluator.IsSatisfied(conditionData, playerModel) == false)
            {
                return false;
            }
        }

        return true;
    }

    private EpisodeProgressModel GetOrCreateProgressModel(string episodeDataId, PlayerModel playerModel)
    {
        if (string.IsNullOrEmpty(episodeDataId) == true || playerModel == null)
        {
            return null;
        }

        if (_progressDict.TryGetValue(episodeDataId, out EpisodeProgressModel progressModel) == true)
        {
            return progressModel;
        }

        EpisodeProgressModel newProgressModel = new EpisodeProgressModel{EpisodeDataId = episodeDataId, TriggerCount = 0, IsCompleted = false};

        playerModel.EpisodeProgressList.Add(newProgressModel);
        _progressDict.Add(episodeDataId, newProgressModel);

        return newProgressModel;
    }

    private void CacheEpisodeConditions()
    {
        _conditionDict.Clear();

        List<EpisodeConditionData> conditionDatas = GameDataManager.Inst.GetDataList<EpisodeConditionData>();

        if (conditionDatas == null || conditionDatas.Count == 0)
        {
            Debug.LogWarning("EpisodeService - 에피소드 조건 데이터가 없음.");
            return;
        }

        for (int i = 0; i < conditionDatas.Count; i++)
        {
            EpisodeConditionData conditionData = conditionDatas[i];

            if (conditionData == null || string.IsNullOrEmpty(conditionData.EpisodeId) == true)
            {
                continue;
            }

            if (_conditionDict.TryGetValue(conditionData.EpisodeId, out List<EpisodeConditionData> conditions) == false)
            {
                conditions = new List<EpisodeConditionData>();
                _conditionDict.Add(conditionData.EpisodeId, conditions);
            }

            conditions.Add(conditionData);
        }
    }

    private bool CanTriggerEpisode(EpisodeData episodeData, EpisodeProgressModel progressModel)
    {
        if (episodeData == null || progressModel == null)
        {
            return false;
        }

        if (progressModel.IsCompleted == true)
        {
            return false;
        }

        if (episodeData.IsRepeatable == false &&
            progressModel.TriggerCount > 0)
        {
            return false;
        }

        bool hasTriggerLimit = episodeData.MaxTriggerCount > 0;

        if (hasTriggerLimit == true && progressModel.TriggerCount >= episodeData.MaxTriggerCount)
        {
            return false;
        }

        return true;
    }

    private void UpdateEpisodeProgress(EpisodeData episodeData, EpisodeProgressModel progressModel)
    {
        if (episodeData == null || progressModel == null)
        {
            return;
        }

        progressModel.TriggerCount++;

        if (episodeData.IsRepeatable == false)
        {
            progressModel.IsCompleted = true;
            return;
        }

        bool hasTriggerLimit = episodeData.MaxTriggerCount > 0;

        if (hasTriggerLimit == true && progressModel.TriggerCount >= episodeData.MaxTriggerCount)
        {
            progressModel.IsCompleted = true;
        }
    }

    private void CacheEpisodeProgress()
    {
        _progressDict.Clear();

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (playerModel == null)
        {
            Debug.LogWarning("EpisodeService - PlayerModel 을 찾을 수 없음.");
            return;
        }

        if (playerModel.EpisodeProgressList == null)
        {
            playerModel.EpisodeProgressList = new List<EpisodeProgressModel>();
            return;
        }

        for (int i = 0; i < playerModel.EpisodeProgressList.Count; i++)
        {
            EpisodeProgressModel progressModel = playerModel.EpisodeProgressList[i];

            if (progressModel == null || string.IsNullOrEmpty(progressModel.EpisodeDataId) == true)
            {
                continue;
            }

            if (_progressDict.ContainsKey(progressModel.EpisodeDataId) == true)
            {
                continue;
            }

            _progressDict.Add(progressModel.EpisodeDataId, progressModel);
        }
    }
}