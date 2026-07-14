using System;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeService
{
    public event Action<EpisodeData> OnUnlockEpisode;
    public event Action<EpisodeData> OnRequestPlayEpisode;

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
        OnUnlockEpisode = null;
        OnRequestPlayEpisode = null;

        _conditionDict.Clear();
        _progressDict.Clear();
    }

    public bool TryUnlockEpisode(string episodeDataId)
    {
        if (string.IsNullOrEmpty(episodeDataId) == true)
        {
            return false;
        }

        EpisodeData episodeData = GameDataManager.Inst.GetData<EpisodeData>(episodeDataId);

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        if (episodeData == null || playerModel == null)
        {
            Debug.LogWarning($"EpisodeService - 에피소드 또는 플레이어 데이터를 찾을 수 없음. EpisodeId : {episodeDataId}");
            return false;
        }

        EpisodeProgressModel progressModel = GetOrCreateProgressModel(episodeDataId, playerModel);

        if (CanUnlockEpisode(progressModel) == false)
        {
            return false;
        }

        if (AreAllConditionsSatisfied(episodeDataId, playerModel) == false)
        {
            return false;
        }

        UnlockEpisode(episodeData, progressModel);
        return true;
    }

    public void CheckAllEpisodes()
    {
        List<EpisodeData> episodeDatas = GameDataManager.Inst.GetDataList<EpisodeData>();

        if (episodeDatas == null || episodeDatas.Count == 0)
        {
            Debug.LogWarning("EpisodeService - 검사할 에피소드 데이터가 없음.");
            return;
        }

        for (int i = 0; i < episodeDatas.Count; i++)
        {
            EpisodeData episodeData = episodeDatas[i];

            if (episodeData == null)
            {
                continue;
            }

            TryUnlockEpisode(episodeData.ID);
        }
    }

    public List<EpisodeData> GetUnlockedArchiveEpisodes()
    {
        List<EpisodeData> unlockedEpisodes = new List<EpisodeData>();

        foreach (KeyValuePair<string, EpisodeProgressModel> pair in _progressDict)
        {
            EpisodeProgressModel progressModel = pair.Value;

            if (progressModel == null)
            {
                continue;
            }

            bool isUnlocked = progressModel.State == EpisodeProgressState.Unlocked ||
                progressModel.State == EpisodeProgressState.Viewed ||
                progressModel.State == EpisodeProgressState.Completed;

            if (isUnlocked == false)
            {
                continue;
            }

            EpisodeData episodeData = GameDataManager.Inst.GetData<EpisodeData>(progressModel.EpisodeDataId);

            if (episodeData == null || episodeData.IsArchiveVisible == false)
            {
                continue;
            }

            unlockedEpisodes.Add(episodeData);
        }

        return unlockedEpisodes;
    }

    public EpisodeProgressModel GetEpisodeProgress(string episodeDataId)
    {
        if (string.IsNullOrEmpty(episodeDataId) == true)
        {
            return null;
        }

        if (_progressDict.TryGetValue(episodeDataId, out EpisodeProgressModel progressModel) == true)
        {
            return progressModel;
        }

        return null;
    }

    public bool CompleteEpisode(string episodeDataId)
    {
        if (string.IsNullOrEmpty(episodeDataId))
        {
            return false;
        }

        EpisodeProgressModel progressModel = GetEpisodeProgress(episodeDataId);

        if (progressModel == null)
        {
            Debug.LogWarning($"EpisodeService - 진행 데이터를 찾을 수 없음. EpisodeId : {episodeDataId}");
            return false;
        }

        if (progressModel.State == EpisodeProgressState.Locked)
        {
            Debug.LogWarning($"EpisodeService - 잠긴 에피소드는 완료할 수 없음. EpisodeId : {episodeDataId}");
            return false;
        }

        progressModel.TriggerCount++;
        progressModel.State = EpisodeProgressState.Completed;
        progressModel.IsNew = false;

        Debug.Log( $"EpisodeService - 에피소드 완료 : {episodeDataId}");

        return true;
    }

    public void MarkEpisodeAsViewed(string episodeDataId)
    {
        EpisodeProgressModel progressModel = GetEpisodeProgress(episodeDataId);

        if (progressModel == null)
        {
            return;
        }

        if (progressModel.State == EpisodeProgressState.Unlocked)
        {
            progressModel.State = EpisodeProgressState.Viewed;
        }

        progressModel.IsNew = false;
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

        EpisodeProgressModel newProgressModel = new EpisodeProgressModel
        {
            EpisodeDataId = episodeDataId,
            State = EpisodeProgressState.Locked,
            TriggerCount = 0,
            IsNew = false
        };

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

            if (conditionData == null || string.IsNullOrEmpty(conditionData.EpisodeId))
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


    private bool CanUnlockEpisode(EpisodeProgressModel progressModel)
    {
        if (progressModel == null)
        {
            return false;
        }

        return progressModel.State == EpisodeProgressState.Locked;
    }

    private void UnlockEpisode(EpisodeData episodeData, EpisodeProgressModel progressModel)
    {
        if (episodeData == null || progressModel == null)
        {
            return;
        }

        progressModel.State = EpisodeProgressState.Unlocked;
        progressModel.IsNew = episodeData.IsArchiveVisible;

        OnUnlockEpisode?.Invoke(episodeData);

        Debug.Log($"EpisodeService - 에피소드 해금 : {episodeData.EpisodeName}");

        if (episodeData.GetPlaybackType() == EpisodePlaybackType.Auto)
        {
            OnRequestPlayEpisode?.Invoke(episodeData);
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

    public string RequestPlayEpisode(string episodeDataId, EpisodePlayMode playMode)
    {
        if (string.IsNullOrEmpty(episodeDataId))
        {
            return string.Empty;
        }

        EpisodeProgressModel progressModel = GetEpisodeProgress(episodeDataId);

        if (progressModel == null)
        {
            Debug.LogWarning($"EpisodeService - 에피소드 진행 데이터 찾을 수 없음. EpisodeId : {episodeDataId}");
            return string.Empty;
        }

        if (progressModel.State == EpisodeProgressState.Locked)
        {
            Debug.LogWarning($"EpisodeService - 잠긴 에피소드. 재생할 수 없음. EpisodeId : {episodeDataId}");
            return string.Empty;
        }

        EpisodeData episodeData = GameDataManager.Inst.GetData<EpisodeData>(episodeDataId);

        if (episodeData == null)
        {
            Debug.LogWarning($"EpisodeService - 에피소드 데이터 찾을 수 없음. EpisodeId : {episodeDataId}");

            return string.Empty;
        }

        if (string.IsNullOrEmpty(episodeData.DialogueId))
        {
            Debug.LogWarning($"EpisodeService - 연결된 다이얼로그 없음. EpisodeId : {episodeDataId}");
            return string.Empty;
        }

        if (playMode == EpisodePlayMode.Normal)
        {
            if (episodeData.IsRepeatable == false && progressModel.TriggerCount > 0)
            {
                Debug.LogWarning($"EpisodeService - 반복 불가능한 에피소드. EpisodeId : {episodeDataId}");
                return string.Empty;
            }

            if (episodeData.MaxTriggerCount > 0 && progressModel.TriggerCount >= episodeData.MaxTriggerCount)
            {
                Debug.LogWarning($"EpisodeService - 최대 발생 횟수에 도달. EpisodeId : {episodeDataId}");
                return string.Empty;
            }
        }

        Dialogue dialogueData = GameDataManager.Inst.GetData<Dialogue>(episodeData.DialogueId);

        if (dialogueData == null)
        {
            Debug.LogWarning($"EpisodeService - 다이얼로그 데이터 찾을 수 없음. DialogueId : {episodeData.DialogueId}");
            return string.Empty;
        }

        return episodeData.DialogueId;
    }
}