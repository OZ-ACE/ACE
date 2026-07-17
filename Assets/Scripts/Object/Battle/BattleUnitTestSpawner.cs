using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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


    [Header("스폰 기준 위치")]
    [SerializeField] private Transform Transform_SpawnRoot;


    [Header("레이캐스트")]
    [SerializeField] private Camera Camera_Raycast;

    private const float RaycastMaxDistance = 100f;

    private List<BattleUnitClickHandler> _spawnedHandlerList = new List<BattleUnitClickHandler>();
    private BattleUnitClickHandler _hoveredHandler;

    public event Action<string> OnUnitClicked;

    private void Start()
    {
        Test_SpawnHeroes();
    }

    [ContextMenu("영웅 테스트 스폰")]
    private void Test_SpawnHeroes()
    {
        _spawnedHandlerList.Clear();

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

        Vector3 basePosition = Vector3.zero;
        if (Transform_SpawnRoot != null)
        {
            basePosition = Transform_SpawnRoot.position;
        }

        Vector3 spawnPosition = basePosition + new Vector3(index * SpawnPositionSpacingX, 0f, 0f);
        GameObject spawnedObj = Instantiate(entry.Prefab, spawnPosition, Quaternion.identity);

        BattleUnitClickHandler clickHandler = spawnedObj.GetComponent<BattleUnitClickHandler>();

        if (clickHandler == null)
        {
            clickHandler = spawnedObj.AddComponent<BattleUnitClickHandler>();
        }

        clickHandler.UnitId = entry.HeroId;
        clickHandler.OnUnitClicked += HandleUnitClicked;
        _spawnedHandlerList.Add(clickHandler);
    }

    private void HandleUnitClicked(string unitId)
    {
        OnUnitClicked?.Invoke(unitId);
    }

    private void Update()
    {
        UpdateHover();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryClickHoveredUnit();
        }
    }

    //마우스 아래에 있는 유닛이 바뀌었을 때만 하이라이트를 갱신한다
    private void UpdateHover()
    {
        BattleUnitClickHandler hitHandler = GetHandlerUnderMouse();

        if (hitHandler == _hoveredHandler)
        {
            return;
        }

        if (_hoveredHandler != null)
        {
            _hoveredHandler.SetHighlight(false);
        }

        _hoveredHandler = hitHandler;

        if (_hoveredHandler != null)
        {
            _hoveredHandler.SetHighlight(true);
        }
    }

    private void TryClickHoveredUnit()
    {
        if (_hoveredHandler == null)
        {
            return;
        }

        _hoveredHandler.NotifyClicked();
    }

    //지정된 카메라 기준으로만 레이캐스트를 쏜다. 씬에 카메라가 여러 개 있어도 이 카메라만 사용하므로 태그 충돌과 무관하다
    private BattleUnitClickHandler GetHandlerUnderMouse()
    {
        if (Camera_Raycast == null)
        {
            return null;
        }

        Ray ray = Camera_Raycast.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, RaycastMaxDistance) == false)
        {
            return null;
        }

        return hit.collider.GetComponent<BattleUnitClickHandler>();
    }
}