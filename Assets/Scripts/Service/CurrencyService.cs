using UnityEngine;
using System;



public class CurrencyService : ICurrencyService
{
    private const int FRAGMENT_TO_GOLD_RATE = 1;

    public event Action OnChangeCurrency;

    public int CurrentGold
    {
        get
        {
            PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
            if (player == null )
            { 
                return 0; 
            }
            return player.Gold;
        }
    }

    public bool IsAffordable(int amount)
    {
        return CurrentGold >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[CurrencyService] 음수 차감 시도 {amount}");
            return false;
        }

        if (IsAffordable(amount) == false)
        {
            return false;
        }

        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        player.Gold -= amount;

        NotifyChange();
        Debug.Log($"[CurrencyService] {amount} 차감 → 잔액 {player.Gold}");
        return true;

    }

    public void AddGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[CurrencyService] 음수 추가 시도: {amount}");
            return;
        }

        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }

        player.Gold += amount;

        NotifyChange();
        Debug.Log($"[CurrencyService] {amount} 획득 → 잔액 {player.Gold}");
    }


    // 잔액 변동 알림
    private void NotifyChange()
    {
        if (OnChangeCurrency != null)
        {
            OnChangeCurrency.Invoke();
        }
    }

    //이하 기억의파편
    public int CurrentMemoryFragment
    {
        get
        {
            PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

            if (player == null)
            {
                return 0;
            }

            return player.MemoryFragment;
        }
    }

    public void AddMemoryFragment(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[CurrencyService] 음수 추가 시도: {amount}");
            return;
        }

        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null)
        {
            return;
        }

        player.MemoryFragment += amount;
        player.TodayMemoryFragment += amount;

        NotifyChange();
        Debug.Log($"[CurrencyService] 기억의파편 {amount} 획득 → 잔액 {player.MemoryFragment}, 오늘치 {player.TodayMemoryFragment}");
    }

    public int CurrentTodayMemoryFragment
    {
        get
        {
            PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

            if (player == null)
            {
                return 0;
            }

            return player.TodayMemoryFragment;
        }
    }


    //파편을 Gold로 교환
    public bool TryExchangeFragmentToGold(int fragmentAmount)
    {
        if (fragmentAmount <=0)
        {
            return false;
        }

        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return false;
        }

        if (player.MemoryFragment < fragmentAmount)
        {
            Debug.Log($"[CurrencyService] 파편 부족 (보유 파편 {player.MemoryFragment}, 요구 파편 {fragmentAmount}");
            return false;
        }

        int gold = fragmentAmount * FRAGMENT_TO_GOLD_RATE;
        player.MemoryFragment -= fragmentAmount;
        player.Gold += gold;

        NotifyChange();
        Debug.Log($"[CurrencyService] 파편 {fragmentAmount} → Gold {gold} (잔여 파편 {player.MemoryFragment})");
        return true;
    }


    //마감 정산 처리 시 성광님 쪽에서 호출, 오늘치 누적량을 0으로 초기화
    public void ResetTodayMemoryFragment()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null)
        {
            return;
        }

        player.TodayMemoryFragment = 0;

        NotifyChange();
    }


}
