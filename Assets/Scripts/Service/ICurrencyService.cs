using System;


//재화(Gold) 관리 + 기억의파편 추가
public interface ICurrencyService
{
    event Action OnChangeCurrency;

    int CurrentGold { get; }
    bool IsAffordable(int amount);
    bool TrySpend(int amount);
    void AddGold(int amount);

    int CurrentMemoryFragment { get; }
    void AddMemoryFragment(int amount);
}
