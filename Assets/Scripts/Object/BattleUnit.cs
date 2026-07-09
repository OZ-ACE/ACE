using System.Collections.Generic;


//BT와 BattleAction에서 영웅/적을 공통으로 참조할 수 있는 최소 구조로 사용 예정
public class BattleUnit
{
    public string UnitId { get; private set; }
    public string UnitName { get; private set; }

    public bool IsHero { get; private set; }

    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }

    public bool IsActionBlocked { get; private set; }

    public List<string> SkillIdList { get; private set; }

    public bool IsDead
    {
        get
        {
            return CurrentHp <= 0;
        }
    }

    public BattleUnit(
        string unitId,
        string unitName,
        bool isHero,
        int maxHp,
        List<string> skillIdList)
    {
        UnitId = unitId;
        UnitName = unitName;
        IsHero = isHero;

        if (maxHp <= 0)
        {
            maxHp = 1;
        }

        MaxHp = maxHp;
        CurrentHp = maxHp;

        IsActionBlocked = false;

        if (skillIdList == null)
        {
            SkillIdList = new List<string>();
        }
        else
        {
            SkillIdList = skillIdList;
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead == true)
        {
            return;
        }

        CurrentHp -= damage;

        if (CurrentHp < 0)
        {
            CurrentHp = 0;
        }
    }

    public void Heal(int amount)
    {
        if (IsDead == true)
        {
            return;
        }

        CurrentHp += amount;

        if (CurrentHp > MaxHp)
        {
            CurrentHp = MaxHp;
        }
    }

    public void SetActionBlocked(bool isBlocked)
    {
        IsActionBlocked = isBlocked;
    }
}
