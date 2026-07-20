using System;
using UnityEngine;

public static class GameUtil
{
    public static long GenerateUniqueId()
    {
        return DateTime.UtcNow.Ticks;
    }

    //유닛ID로 표시용 이름을 반환한다.
    public static string GetUnitDisplayName(string unitId)
    {
        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(unitId);

        if (heroData != null)
        {
            return heroData.HeroName;
        }

        EnemyData enemyData = GameDataManager.Inst.GetData<EnemyData>(unitId);

        if (enemyData != null)
        {
            return enemyData.EnemyName;
        }

        return unitId;
    }
}
