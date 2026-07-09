using System.Collections.Generic;

public static class BattleTargetSelector
{
    //targetSelectType과 targetCount를 기준으로 실제 대상 목록을 선택
    //targetCount가 -1이면 조건에 맞는 전체 대상을 의미
    public static List<BattleUnitModel> SelectTargets(
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        TargetSelectType targetSelectType,
        int targetCount)
    {
        if (unit == null)
        {
            return new List<BattleUnitModel>();
        }

        List<BattleUnitModel> candidateList = GetCandidateList(unit, heroList, enemyList, targetSelectType);

        if (candidateList == null || candidateList.Count <= 0)
        {
            return new List<BattleUnitModel>();
        }

        List<BattleUnitModel> aliveTargetList = new List<BattleUnitModel>();

        foreach (BattleUnitModel target in candidateList)
        {
            if (target == null)
            {
                continue;
            }

            if (target.IsDefeated == true)
            {
                continue;
            }

            aliveTargetList.Add(target);
        }

        candidateList = aliveTargetList;

        if (candidateList.Count <= 0)
        {
            return new List<BattleUnitModel>();
        }

        if (targetSelectType == TargetSelectType.Self)
        {
            return new List<BattleUnitModel>() { unit };
        }

        if (targetCount == -1)
        {
            return candidateList;
        }

        if (targetCount <= 0)
        {
            return new List<BattleUnitModel>();
        }

        if (targetCount > candidateList.Count)
        {
            targetCount = candidateList.Count;
        }

        switch (targetSelectType)
        {
            case TargetSelectType.RandomEnemy:
            case TargetSelectType.RandomFriendly:
                return SelectRandomTargets(candidateList, targetCount);

            case TargetSelectType.LowestHpEnemy:
            case TargetSelectType.LowestHpFriendly:
                return SelectLowestHpTargets(candidateList, targetCount);

            default:
                return new List<BattleUnitModel>();
        }
    }

    //행동 유닛의 진영과 targetSelectType을 기준으로 후보 대상 목록을 가져온다.
    private static List<BattleUnitModel> GetCandidateList(
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        TargetSelectType targetSelectType)
    {
        switch (targetSelectType)
        {
            case TargetSelectType.RandomEnemy:
            case TargetSelectType.LowestHpEnemy:
                if (unit.IsHero == true)
                {
                    return enemyList;
                }

                return heroList;

            case TargetSelectType.RandomFriendly:
            case TargetSelectType.LowestHpFriendly:
                if (unit.IsHero == true)
                {
                    return heroList;
                }

                return enemyList;

            case TargetSelectType.Self:
                return new List<BattleUnitModel>() { unit };

            default:
                return new List<BattleUnitModel>();
        }
    }

    //후보 목록에서 중복 없이 랜덤 대상을 선택한다.
    private static List<BattleUnitModel> SelectRandomTargets(List<BattleUnitModel> candidateList, int targetCount)
    {
        List<BattleUnitModel> resultList = new List<BattleUnitModel>();
        List<BattleUnitModel> copyList = new List<BattleUnitModel>(candidateList);

        for (int i = 0; i < targetCount; i++)
        {
            if (copyList.Count <= 0)
            {
                break;
            }

            int randomIndex = UnityEngine.Random.Range(0, copyList.Count);
            BattleUnitModel target = copyList[randomIndex];

            resultList.Add(target);
            copyList.RemoveAt(randomIndex);
        }

        return resultList;
    }

    //후보 목록에서 현재 체력이 낮은 순서대로 대상을 선택한다.
    private static List<BattleUnitModel> SelectLowestHpTargets(List<BattleUnitModel> candidateList, int targetCount)
    {
        List<BattleUnitModel> resultList = new List<BattleUnitModel>();
        List<BattleUnitModel> copyList = new List<BattleUnitModel>(candidateList);

        for (int i = 0; i < targetCount; i++)
        {
            if (copyList.Count <= 0)
            {
                break;
            }

            BattleUnitModel lowestHpTarget = copyList[0];

            foreach (BattleUnitModel target in copyList)
            {
                if (target.CurrentHp < lowestHpTarget.CurrentHp)
                {
                    lowestHpTarget = target;
                }
            }

            resultList.Add(lowestHpTarget);
            copyList.Remove(lowestHpTarget);
        }

        return resultList;
    }
}
