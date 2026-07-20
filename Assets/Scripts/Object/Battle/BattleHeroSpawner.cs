using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//전투 참여 영웅을 스폰하고 클릭/호버 입력을 처리하는 컴포넌트
public class BattleHeroSpawner : SingletonBase<BattleHeroSpawner>
{
    [Serializable]
    public struct HeroSpawnEntry
    {
        public string HeroId;
        public GameObject Prefab;
        public float Scale;
    }

    private const float SpawnPositionSpacingX = 2f;

    [Header("영웅 프리팹 매핑")]
    [SerializeField] private List<HeroSpawnEntry> _heroSpawnEntryList;

    [Header("스폰 기준 위치")]
    [SerializeField] private Transform Transform_SpawnRoot;


    [Header("레이캐스트")]
    [SerializeField] private Camera Camera_Raycast;

    private const float RaycastMaxDistance = 100f;

    private List<string> _selectedHeroIdList = new List<string>();
    private List<BattleUnitClickHandler> _spawnedHandlerList = new List<BattleUnitClickHandler>();
    private BattleUnitClickHandler _hoveredHandler;

    public event Action<string> OnUnitClicked;

    private void OnEnable()
    {
        SpawnHeroes();
    }

    //캐릭터 선택 시스템에서 전투에 데려갈 유닛 ID 목록을 받는다.
    public void SetSelectedHeroIdList(List<string> heroIdList)
    {
        _selectedHeroIdList = heroIdList;
    }

    [ContextMenu("영웅 테스트 스폰")]
    public void SpawnHeroes()
    {
        ClearSpawnedHeroes();

        List<string> heroIdListToSpawn = (_selectedHeroIdList != null && _selectedHeroIdList.Count > 0)
            ? _selectedHeroIdList
            : GetFallbackHeroIdList();

        for (int i = 0; i < heroIdListToSpawn.Count; i++)
        {
            HeroSpawnEntry entry = FindSpawnEntry(heroIdListToSpawn[i]);

            if (string.IsNullOrEmpty(entry.HeroId))
            {
                Debug.LogWarning($"[BattleHeroSpawner] {heroIdListToSpawn[i]}에 대한 프리팹 매핑을 찾을 수 없음");
                continue;
            }

            SpawnHero(entry, i);
        }
    }

    //인스펙터에 등록된 전체 목록을 예비로 반환한다 (선택 결과 미주입 시 기존 테스트 동작과 동일하게 유지)
    private List<string> GetFallbackHeroIdList()
    {
        List<string> fallbackList = new List<string>();

        foreach (HeroSpawnEntry entry in _heroSpawnEntryList)
        {
            fallbackList.Add(entry.HeroId);
        }

        return fallbackList;
    }

    private HeroSpawnEntry FindSpawnEntry(string heroId)
    {
        foreach (HeroSpawnEntry entry in _heroSpawnEntryList)
        {
            if (entry.HeroId == heroId)
            {
                return entry;
            }
        }

        return default;
    }    

    private void ClearSpawnedHeroes()
    {
        foreach (BattleUnitClickHandler handler in _spawnedHandlerList)
        {
            if (handler != null)
            {
                handler.OnUnitClicked -= HandleUnitClicked;
                Destroy(handler.gameObject);
            }
        }

        _spawnedHandlerList.Clear();
        _hoveredHandler = null;
    }

    //실제 스폰된 유닛 기준 ID 목록을 반환해 전투 턴 순서 계산에 사용
    public List<string> GetHeroIdList()
    {
        List<string> heroIdList = new List<string>();

        foreach (BattleUnitClickHandler handler in _spawnedHandlerList)
        {
            heroIdList.Add(handler.UnitId);
        }

        return heroIdList;
    }

    private void SpawnHero(HeroSpawnEntry entry, int index)
    {
        if (entry.Prefab == null)
        {
            Debug.LogWarning($"[BattleHeroSpawner] {entry.HeroId} 프리팹이 할당되지 않음");
            return;
        }

        Vector3 basePosition = Vector3.zero;
        Vector3 spawnDirection = Vector3.right;
        if (Transform_SpawnRoot != null)
        {
            basePosition = Transform_SpawnRoot.position;
            spawnDirection = Transform_SpawnRoot.right;
        }

        Vector3 spawnPosition = basePosition + spawnDirection * (index * SpawnPositionSpacingX);
        GameObject spawnedObj = Instantiate(entry.Prefab, spawnPosition, Quaternion.Euler(0f, 100f, 0f));
        float scale = entry.Scale > 0f ? entry.Scale : 1f; //인스펙터 미입력(0)이면 원본 크기 유지
        spawnedObj.transform.localScale = new Vector3(scale, scale, scale);

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
