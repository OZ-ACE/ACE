using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

// Model 역할 + ViewModel 역할
public class QuestViewModel : ViewModelBase
{
    private readonly ICurrencyService _currencyService;

    public event Action OnChangeQuestProgress;

    public QuestViewModel(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    //전체 퀘스트 데이터 (JSON 원본)
    public List<QuestData> QuestList
    {
        get { return GameDataManager.Inst.GetDataList<QuestData>(); }
    }

    public QuestState GetState(string questID)
    {
        QuestProgressModel progress = FindProgress(questID);
        if (progress == null)
        {
            return QuestState.Locked;
        }
        return (QuestState)progress.State;
    }

    public int GetProgressCount(string questID)
    {
        QuestProgressModel progress = FindProgress(questID);
        if (progress == null)
        {
            return 0;
        }
        return progress.CurrentCount;
    }

    //보상을 받을 수 있는 상태인가
    public bool IsClaimable(string questID)
    {
        return GetState(questID) == QuestState.Completed;
    }

    //누락된 퀘스트 진행 엔트리를 생성
    private void EnsureProgressEntries()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }
        if (player.QuestProgressList == null)
        {
            player.QuestProgressList = new List<QuestProgressModel>();
        }
        foreach (QuestData quest in QuestList)
        {
            if (FindProgress(quest.ID) != null)
            {
                continue;
            }
            QuestProgressModel progress = new QuestProgressModel();
            progress.QuestID = quest.ID;
            progress.CurrentCount = 0;
            progress.State = (int)QuestState.InProgress;
            player.QuestProgressList.Add(progress);
        }
    }



    // 세이브에 퀘스트 기록이 없으면 생성 (슬롯 전환 시에도 호출)
    public void InitQuest()
    {
        EnsureProgressEntries();
        RefreshLocks();
        RefreshStateConditions();
    }

    //뷰가 켜질 때 1회 호출
    public void InvokeOnceOnInit()
    {
        RefreshLocks();            //표시 여부(잠금) 최신화
        RefreshStateConditions();
        OnPropertyChanged(nameof(QuestList));
    }

    //슬롯 전환 시 현재 슬롯 기준으로 재구성
    public void ReloadQuest()
    {
        InitQuest();
        OnPropertyChanged(nameof(QuestList));
    }

    //선행 퀘스트 수령 여부에 따라 잠금 상태 갱신
    private void RefreshLocks()
    {
        foreach (QuestData quest in QuestList)
        {
            QuestProgressModel progress = FindProgress(quest.ID);
            if (progress == null)
            {
                continue;
            }
            if (progress.State == (int)QuestState.Completed || progress.State == (int)QuestState.Rewarded)
            {
                continue;
            }

            if (IsUnlocked(quest) == true)
            {
                progress.State = (int)QuestState.InProgress;
            }
            else
            {
                progress.State = (int)QuestState.Locked;
            }
        }
    }

    private bool IsUnlocked(QuestData quest)
    {
        if (quest.HasRequiredQuest() == false)
        {
            return true;
        }

        QuestProgressModel required = FindProgress(quest.RequiredQuestID);
        if (required == null)
        {
            return false;
        }
        return required.State == (int)QuestState.Rewarded;
    }

    //각 시스템이 진행도를 보고 (누적형 조건)
    public void ReportProgress(QuestConditionType type, string targetID, int amount)
    {
        Debug.Log($"[QuestViewModel] ReportProgress 진입: {type} / {targetID} / 퀘스트 {QuestList.Count}개");
        if (amount <= 0)
        {
            return;
        }

        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }

        EnsureProgressEntries();   //로드로 엔트리가 누락됐어도 복구
        RefreshLocks();            //선행 미완료 퀘스트는 여기서 Locked로 확정

        bool isChanged = false;
        foreach (QuestData quest in QuestList)
        {
            if (quest.GetConditionType() != type)
            {
                continue;
            }
            if (IsTargetMatched(quest, targetID) == false)
            {
                continue;
            }

            QuestProgressModel progress = FindProgress(quest.ID);
            if (progress == null)
            {
                continue;
            }
            if (progress.State != (int)QuestState.InProgress)
            {
                continue;
            }

            progress.CurrentCount += amount;
            if (progress.CurrentCount >= quest.ConditionCount)
            {
                progress.CurrentCount = quest.ConditionCount;
                progress.State = (int)QuestState.Completed;
                Debug.Log($"[QuestViewModel] 퀘스트 달성: {quest.QuestName}");
            }
            isChanged = true;
        }

        if (isChanged == false)
        {
            return;
        }

        NotifyProgress();
    }

    //조건 대상이 일치하는가 (빈 값이면 대상 무관)
    private bool IsTargetMatched(QuestData quest, string targetID)
    {
        if (string.IsNullOrEmpty(quest.ConditionTargetID) == true)
        {
            return true;
        }
        return quest.ConditionTargetID == targetID;
    }

    //상태형 조건은 현재 값을 조회해서 갱신
    private void RefreshStateConditions()
    {
        foreach (QuestData quest in QuestList)
        {
            if (quest.IsStateCondition() == false)
            {
                continue;
            }

            QuestProgressModel progress = FindProgress(quest.ID);
            if (progress == null || progress.State != (int)QuestState.InProgress)
            {
                continue;
            }

            int current = GetStateConditionValue(quest);
            progress.CurrentCount = current;
            if (current >= quest.ConditionCount)
            {
                progress.State = (int)QuestState.Completed;
            }
        }
    }

    private int GetStateConditionValue(QuestData quest)
    {
        if (quest.GetConditionType() == QuestConditionType.ReachGold)
        {
            return _currencyService.CurrentGold;
        }

        return 0;
    }

    // 보상 수령. 성공 시 지급 + 상태 변경 + 저장
    public bool TryClaimReward(string questID)
    {
        QuestData quest = GameDataManager.Inst.GetData<QuestData>(questID);
        if (quest == null)
        {
            return false;
        }

        QuestProgressModel progress = FindProgress(questID);
        if (progress == null)
        {
            return false;
        }
        if (progress.State != (int)QuestState.Completed)
        {
            return false;   // 미달성이거나 이미 수령함
        }

        GiveReward(quest);
        progress.State = (int)QuestState.Rewarded;

        RefreshLocks();
        NotifyProgress();

        Debug.Log($"[QuestViewModel] 보상 수령: {quest.QuestName} (+{quest.RewardAmount})");
        return true;
    }

    private void GiveReward(QuestData quest)
    {
        QuestRewardType type = quest.GetRewardType();
        if (type == QuestRewardType.Gold)
        {
            _currencyService.AddGold(quest.RewardAmount);
        }
        else if (type == QuestRewardType.MemoryFragment)
        {
            _currencyService.AddMemoryFragment(quest.RewardAmount);
        }
    }

    private QuestProgressModel FindProgress(string questID)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null || player.QuestProgressList == null)
        {
            return null;
        }

        foreach (QuestProgressModel progress in player.QuestProgressList)
        {
            if (progress.QuestID == questID)
            {
                return progress;
            }
        }
        return null;
    }


    //UI에 표시할 퀘스트만 반환 (선행 미완료로 잠긴 퀘스트 제외)
    public List<QuestData> GetVisibleQuestList()
    {
        List<QuestData> result = new List<QuestData>();
        foreach (QuestData quest in QuestList)
        {
            if (GetState(quest.ID) == QuestState.Locked)
            {
                continue;
            }
            result.Add(quest);
        }
        return result;
    }


    private void NotifyProgress()
    {
        if (OnChangeQuestProgress != null)
        {
            OnChangeQuestProgress.Invoke();
        }
    }
}