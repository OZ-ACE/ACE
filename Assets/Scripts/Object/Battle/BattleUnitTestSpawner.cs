using System;
using System.Collections.Generic;
using UnityEngine;

// 히어로 프리팹을 임시 위치에 배치하고 클릭 핸들러를 붙이는 테스트 전용 스포너 (정식 스폰 시스템 완성되면 삭제 대상)
public class BattleUnitTestSpawner : SingletonBase<BattleUnitTestSpawner>
{
    [Serializable]
    public struct HeroSpawnEntry
    {
        public string HeroId;
        public GameObject Prefab;
    }

    private const float SpawnPositionSpacingX = 2f;

    [SerializeField] private List<HeroSpawnEntry> _heroSpawnEntryList;

    public event Action<string> OnUnitClicked;

    [ContextMenu("영웅 테스트 스폰")]
    private void Test_SpawnHeroes()
    {
        for (int i = 0; i < _heroSpawnEntryList.Count; i++)
        {
            SpawnHero(_heroSpawnEntryList[i], i);
        }
    }

    public List<string> GetHeroIdList()
    {
        List<string> heroIdList = new List<string>();

        foreach (HeroSpawnEntry entry in _heroSpawnEntryList)
        {
            heroIdList.Add(entry.HeroId);
        }

        return heroIdList;
    }

    private void SpawnHero(HeroSpawnEntry entry, int index)
    {
        if (entry.Prefab == null)
        {
            Debug.LogWarning($"[BattleUnitTestSpawner] {entry.HeroId} 프리팹이 할당되지 않음");
            return;
        }

        Vector3 spawnPosition = new Vector3(index * SpawnPositionSpacingX, 0f, 0f);
        GameObject spawnedObj = Instantiate(entry.Prefab, spawnPosition, Quaternion.identity);

        BattleUnitClickHandler clickHandler = spawnedObj.GetComponent<BattleUnitClickHandler>();

        if (clickHandler == null)
        {
            clickHandler = spawnedObj.AddComponent<BattleUnitClickHandler>();
        }

        clickHandler.UnitId = entry.HeroId;
        clickHandler.OnUnitClicked += HandleUnitClicked;
    }

    private void HandleUnitClicked(string unitId)
    {
        OnUnitClicked?.Invoke(unitId);
    }
}