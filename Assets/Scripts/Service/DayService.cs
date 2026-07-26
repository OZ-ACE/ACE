using UnityEngine;
using System;

// 날짜 진행 담당
public class DayService
{
    public event Action<int> OnChangeDay;
    public event Action<int> OnChangeHour;
    public event Action OnEndDay;

    private const string DINING_ROOM_ID = "Room_Meal";
    private const string COUNSEL_ROOM_ID = "Room_Counsel";

    private const float _realTime = 5f;
    private float _time = 0f;
    private bool _isTimerPlaying = false;

    private int _currentHour = 0;
    public int CurrentHour
    {
        get => _currentHour;
        set
        {
            if (_currentHour != value)
            {
                _currentHour = value;
            }
        }
    }

    // 프로퍼티
    public int CurrentDay
    {
        get
        {
            PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
            return (player != null) ? player.Day : 1;
        }
    }

    public bool IsBattleDoneToday
    {
        get
        {
            PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
            return (player != null) && player.IsBattleDoneToday;
        }
    }

    // 메서드

    // 다음날로 넘길 수 있는가
    public bool IsAdvanceable()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return false;
        }
        return player.IsBattleDoneToday;
    }

    // 오늘 전투 완료 표시
    public void MarkBattleDone()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }

        player.IsBattleDoneToday = true;

        Debug.Log("[DayService] 오늘 전투 완료 표시");
    }

    // 다음날로 넘기기 시도
    public bool TryAdvanceDay()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return false;
        }

        EvaluateHeroSchedule();

        if (CurrentDay != 1 && player.IsBattleDoneToday == false)
        {
            Debug.Log("[DayService] 오늘 전투 마쳐야 함");
            return false;
        }

        player.Day++;
        player.IsBattleDoneToday = false;

        SaveManager.Inst.RequestSaveData(player);

        _currentHour = 0;
        _time = 0f;

        StartTimer();

        if (OnChangeDay != null)
        {
            OnChangeDay.Invoke(player.Day);
        }

        Debug.Log($"[DayService] 다음날로 이동 Day : {player.Day}");
        GameManager.Inst.Services.QuestService.ReportProgress(QuestConditionType.AdvanceDay, string.Empty, 1);
        return true;
    }

    public void StartTimer()
    {
        _isTimerPlaying = true;
    }

    public void PauseTimer()
    {
        _isTimerPlaying = false;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (_isTimerPlaying == false)
        {
            return;
        }

        _time += deltaTime;

        if (_time >= _realTime)
        {
            _time -= _realTime;
            AddHour();
        }
    }

    private void AddHour()
    {
        _currentHour++;

        if (_currentHour >= 24)
        {
            _currentHour = 0;
            PauseTimer();

            OnEndDay.Invoke();
            return;
        }

        OnChangeHour?.Invoke(_currentHour);
    }

    private void EvaluateHeroSchedule()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        RoomAssignmentService roomService = GameManager.Inst.Services.RoomAssignmentService;
        BuildGridModel gridModel = GameManager.Inst.Services.BuildService.GetBuildGridViewModel().BuildGridModel;

        bool hasDiningRoom = gridModel.HasRoom(DINING_ROOM_ID);
        bool hasCounselRoom = gridModel.HasRoom(COUNSEL_ROOM_ID);

        foreach (HeroStat heroStat in player.HeroStats)
        {
            HeroModel heroModel = new HeroModel();
            heroModel.LoadHeroData(heroStat.HeroID);

            bool isBedroomAdjacent = false;
            long bedroomInstanceId = roomService.GetAssignedRoomInstanceId(heroStat.HeroID);

            if (bedroomInstanceId > 0)
            {
                PlacedRoomData bedroom = gridModel.GetRoomByInstanceId(bedroomInstanceId);

                if (bedroom != null)
                {
                    isBedroomAdjacent = gridModel.HasAdjacentSameTypeRoom(bedroom);
                }
            }

            ScheduleEvaluator.EvaluateDailySchedule(heroModel, isBedroomAdjacent, hasDiningRoom, hasCounselRoom);
        }
    }
}