using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroRequestService
{
    private PlayerModel PlayerModel => SaveManager.Inst.CurrentPlayerModel;

    public HeroRequestModel CurrentRequest => PlayerModel?.CurrentHeroRequest;
    
    public int CurrentHour { get; private set; }

    public event Action OnHeroRequestChanged;

    public void TryCreateDailyRequest(int currentDay)
    {
        if (PlayerModel == null)
        {
            return;
        }

        if (PlayerModel.LastHeroRequestDay == currentDay)
        {
            return;
        }

        HeroRequestData selectedRequest = GameDataManager.Inst.GetData<HeroRequestData>(PlayerModel.HeroRequestScheduledRequestId);

        if (selectedRequest == null)
        {
            return;
        }

        int currentHour = PlayerModel.HeroRequestScheduledHour;

        CreateRequest(PlayerModel, selectedRequest, currentDay, currentHour);

        SaveManager.Inst.RequestSaveData(PlayerModel);

        Debug.Log($"HeroRequestService - 요청 생성 완료 / 영웅 : {selectedRequest.HeroId}, 요청 : {selectedRequest.ID}");
    }

    private void CreateRequest(PlayerModel playerModel, HeroRequestData data, int currentDay, int currentHour)
    {
        int expireHour = currentHour + data.RequiredHour + 2;

        playerModel.CurrentHeroRequest = new HeroRequestModel
        {
            RequestId = data.ID,
            HeroId = data.HeroId,

            CreatedDay = currentDay,
            CreatedHour = currentHour,

            ExpireHour = expireHour,

            CurrentProgress = 0,
            State = (int)HeroRequestState.InProgress
        };

        playerModel.LastHeroRequestDay = currentDay;
        playerModel.LastHeroRequestHeroId = data.HeroId;

        OnHeroRequestChanged?.Invoke();
    }

    public void OnChangeDay(int currentDay)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null)
        {
            return;
        }

        player.CurrentHeroRequest = null;
        player.HeroRequestScheduledRequestId = null;
        player.HeroRequestScheduledDay = currentDay;

        List<HeroRequestData> allRequests = GameDataManager.Inst.GetDataList<HeroRequestData>();

        if (allRequests == null || allRequests.Count == 0)
        {
            return;
        }

        List<string> admittedHeroIds = player.HeroStats.Select(hero => hero.HeroID).ToList();

        List<HeroRequestData> candidates = allRequests.Where(request => admittedHeroIds.Contains(request.HeroId)).ToList();

        if (candidates.Count > 1)
        {
            candidates = candidates.Where(request => request.HeroId != player.LastHeroRequestHeroId).ToList();
        }

        if (candidates.Count == 0)
        {
            return;
        }

        HeroRequestData selectedRequest = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        int latestStartHour = 24 - selectedRequest.RequiredHour - 2;

        player.HeroRequestScheduledRequestId = selectedRequest.ID;
        player.HeroRequestScheduledHour = UnityEngine.Random.Range(8, latestStartHour + 1);

        SaveManager.Inst.RequestSaveData(player);

        OnHeroRequestChanged?.Invoke();

        Debug.Log($"HeroRequestService - Day {currentDay} 요청 예정 : {player.HeroRequestScheduledHour:00}:00 / {selectedRequest.ID}");
    }

    public void OnChangeHour(int currentHour)
    {
        CurrentHour = currentHour;

        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null)
        {
            return;
        }

        if (player.CurrentHeroRequest != null)
        {
            HeroRequestModel request = player.CurrentHeroRequest;

            if ((HeroRequestState)request.State == HeroRequestState.InProgress && currentHour >= request.ExpireHour)
            {
                request.State = (int)HeroRequestState.Failed;

                SaveManager.Inst.RequestSaveData(player);
                OnHeroRequestChanged?.Invoke();

                Debug.Log("HeroRequest 실패");
            }

            return;
        }

        if (player.HeroRequestScheduledDay != player.Day)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.HeroRequestScheduledRequestId))
        {
            return;
        }

        if (currentHour < player.HeroRequestScheduledHour)
        {
            return;
        }

        TryCreateDailyRequest(player.Day);
    }

    public HeroRequestModel GetCurrentRequest()
    {
        return PlayerModel?.CurrentHeroRequest;
    }

    public HeroRequestData GetCurrentRequestData()
    {
        HeroRequestModel current = PlayerModel?.CurrentHeroRequest;

        if (current == null)
        {
            return null;
        }

        return GameDataManager.Inst.GetData<HeroRequestData>(current.RequestId);
    }

    public void EvaluateScheduleRequest(HeroModel heroModel, int sunCount, int sleepCount, int showerCount, int restCount)
    {
        HeroRequestModel request = GetCurrentRequest();

        if (request == null)
        {
            return;
        }

        if (request.HeroId != heroModel.HeroID)
        {
            return;
        }

        if ((HeroRequestState)request.State != HeroRequestState.InProgress)
        {
            return;
        }

        HeroRequestData data = GetCurrentRequestData();

        if (data == null)
        {
            return;
        }

        int previousProgress = request.CurrentProgress;

        switch (data.RequiredSchedule)
        {
            case ScheduleState.Rest:
                request.CurrentProgress = Mathf.Min(restCount, data.RequiredHour);
                break;

            case ScheduleState.Sleep:
                request.CurrentProgress = Mathf.Min(sleepCount, data.RequiredHour);
                break;

            case ScheduleState.Shower:
                request.CurrentProgress = Mathf.Min(showerCount, data.RequiredHour);
                break;

            case ScheduleState.Sun:
                request.CurrentProgress = Mathf.Min(sunCount, data.RequiredHour);
                break;
        }

        if (request.CurrentProgress != previousProgress)
        {
            SaveManager.Inst.RequestSaveData(PlayerModel);
            OnHeroRequestChanged?.Invoke();
        }

        if (request.CurrentProgress < data.RequiredHour)
        {
            return;
        }

        CompleteRequest(request, data);
    }

    private void CompleteRequest(HeroRequestModel request, HeroRequestData data)
    {
        HeroStat hero = PlayerModel.HeroStats.FirstOrDefault(x => x.HeroID == request.HeroId);

        if (hero == null)
        {
            return;
        }

        hero.Affection = Mathf.Clamp(hero.Affection + data.RewardAffection, 0, 100);

        hero.Satisfaction = Mathf.Clamp(hero.Satisfaction + data.RewardSatisfaction, 0, 100);

        request.State = (int)HeroRequestState.Rewarded;

        SaveManager.Inst.RequestSaveData(PlayerModel);

        OnHeroRequestChanged?.Invoke();

        Debug.Log($"HeroRequest 완료 - {request.HeroId} / " + $"호감도 + {data.RewardAffection}, " + $"만족도 + {data.RewardSatisfaction}");
    }
}