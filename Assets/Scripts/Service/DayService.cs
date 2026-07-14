using UnityEngine;
using System;


// 날짜 진행 담당
public class DayService
{
    public event Action<int> OnChangeDay;


    // 프로퍼티
    public int CurrentDay
    {
        get
        {
            PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
            return (player !=null) ? player.Day : 1;
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
    public void MarkBattleDone()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }

        player.IsBattleDoneToday = true;
        SaveManager.Inst.RequestSaveData(player);

        Debug.Log("[DayService] 오늘 전투 완료 표시");
    }

    public bool TryAdvanceDay()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return false;
        }

        if (player.IsBattleDoneToday == false)
        {
            Debug.Log("[DayService] 오늘 전투 마쳐야 함");
            return false;
        }

        player.Day++;
        player.IsBattleDoneToday = false;
        SaveManager.Inst.RequestSaveData(player);

        if (OnChangeDay != null)
        {
            OnChangeDay.Invoke(player.Day);
        }

        Debug.Log($"[DayService] 다음날로 이동 Day : {player.Day}");
        return true;
    }
}
