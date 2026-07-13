using UnityEngine;
using System;


//PlayerModel.Gold를 관리
public class CurrencyService : ICurrencyService
{
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


    /// <summary> 잔액 변동 알림 </summary>
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

        NotifyChange();
        Debug.Log($"[CurrencyService] 기억의파편 {amount} 획득 → 잔액 {player.MemoryFragment}");
    }
}
