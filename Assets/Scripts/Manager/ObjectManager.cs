using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectManager : SingletonBase<ObjectManager>
{
    [Header("건설 격자 뷰 프리팹")]
    [SerializeField] private GameObject Prefab_BuildGridView;

    [Header("3D 사무실 루트 프리팹")]
    [SerializeField] private GameObject Prefab_OfficeRoot;

    [Header("3D 전투 공간 프리팹")]
    [SerializeField] private GameObject Prefab_BattleRoot;

    private BuildGridView _buildGridView;
    private GameObject _hero;

    private GameObject _officeRoot;
    private OfficeInputView _officeInputView;
    private Camera _mainCamera;

    private GameObject _battleRoot;

    public BuildGridView BuildGridView { get { return _buildGridView; } }

    private Dictionary<string, HeroMovingAgent> _spawnHero = new Dictionary<string, HeroMovingAgent>();

    private void Start()
    {
        _mainCamera = Camera.main;
        CreateOfficeRoot();
        CreateBattleRoot();
    }

    public void CreateBuildGridView()
    {
        if (Prefab_BuildGridView == null)
        {
            Debug.LogWarning("[ObjectManager] 격자 프리팹 인스펙터 할당 안 됨");
            return;
        }

        if (_buildGridView != null)
        {
            return;
        }

        GameObject viewObj = Instantiate(Prefab_BuildGridView, Vector3.zero, Quaternion.identity);
        viewObj.name = "BuildGridView";

        _buildGridView = viewObj.GetComponent<BuildGridView>();
        if (_buildGridView == null)
        {
            Debug.LogWarning("[ObjectManager] BuildGridView 컴포넌트 없음");
            return;
        }

        BuildGridViewModel viewModel = GameManager.Inst.Services.BuildService.GetBuildGridViewModel();

        viewModel.OnClickOffice -= OnClickOffice;
        viewModel.OnClickOffice += OnClickOffice;

        viewModel.OnClickBattle -= OnClickBattle;
        viewModel.OnClickBattle += OnClickBattle;

        _buildGridView.Bind(viewModel);
        Debug.Log("[ObjectManager] 건설 격자 생성 완료");
    }

    private void OnClickOffice()
    {
        EnterOffice();
    }

    private void OnClickBattle()
    {
        SoundManager.Inst.PlayBGM("Battle");
        EnterBattle();
    }

    private void CreateOfficeRoot()
    {
        if (Prefab_OfficeRoot == null)
        {
            Debug.LogWarning("[ObjectManager] 사무실 프리팹 인스펙터 할당 안 됨");
            return;
        }

        if (_officeRoot != null)
        {
            return;
        }

        _officeRoot = Instantiate(Prefab_OfficeRoot);
        _officeRoot.name = "OfficeRoot";

        _officeInputView = _officeRoot.GetComponentInChildren<OfficeInputView>(true);
        if (_officeInputView == null)
        {
            Debug.LogWarning("[ObjectManager] OfficeInputView 컴포넌트 없음");
            return;
        }

        _officeInputView.OnClickOfficeObject -= OnClickOfficeObject;
        _officeInputView.OnClickOfficeObject += OnClickOfficeObject;

        _officeRoot.SetActive(false);
        Debug.Log("[ObjectManager] 3D 사무실 생성 완료");
    }

    //3D 사무실 진입. 메인 카메라·그리드 입력 차단 
    public void EnterOffice()
    {
        if (_officeRoot == null)
        {
            return;
        }

        _officeRoot.SetActive(true);
        UIManager.Inst.CloseTycoonMainUI();

        if (_mainCamera != null)
        {
            _mainCamera.gameObject.SetActive(false);
        }

        if (_buildGridView != null)
        {
            _buildGridView.enabled = false;
        }
    }

    //3D 사무실 퇴장. 메인 카메라·그리드 입력 복구 
    public void ExitOffice()
    {
        if (_officeRoot == null)
        {
            return;
        }

        _officeRoot.SetActive(false);
        UIManager.Inst.OpenTycoonMainUI();
        if (_mainCamera != null)
        {
            _mainCamera.gameObject.SetActive(true);
        }

        if (_buildGridView != null)
        {
            _buildGridView.enabled = true;
        }
    }

    private void OnClickOfficeObject(OfficeObjectType type)
    {
        switch (type)
        {
            case OfficeObjectType.NextDay:
                DayService dayService = GameManager.Inst.Services.DayService;
                if (dayService.CurrentDay != 1 && dayService.IsAdvanceable() == false)
                {
                    Debug.Log("[ObjectManager] 오늘 전투를 마쳐야 다음날로 넘어갈 수 있음");
                    return;
                }
                UIManager.Inst.OpenSettlementUI();
                break;

            case OfficeObjectType.Shop:
                UIManager.Inst.OpenShopUI();
                break;

            case OfficeObjectType.Admission:
                UIManager.Inst.OpenAdmissionPopup();
                break;

            case OfficeObjectType.Close:
                ExitOffice();
                break;
        }
    }

    // 3D 사무실에서 전투 UI로 진입
    public void EnterBattle()
    {
        if (_officeRoot != null)
        {
            _officeRoot.SetActive(false);
        }

        if (_mainCamera != null)
        {
            _mainCamera.gameObject.SetActive(false);
        }

        if (_buildGridView != null)
        {
            _buildGridView.enabled = false;
        }

        if (_battleRoot != null)
        {
            _battleRoot.SetActive(true);
        }

        UIManager.Inst.CloseTycoonMainUI();
        UIManager.Inst.OpenBattleMainUI();
    }

    // 전투 종료 후 타이쿤으로 복귀
    public void ExitBattle()
    {
        UIManager.Inst.CloseBattleMainUI();

        if (_battleRoot != null)
        {
            _battleRoot.SetActive(false);
        }

        if (_mainCamera != null)
        {
            _mainCamera.gameObject.SetActive(true);
        }

        if (_buildGridView != null)
        {
            _buildGridView.enabled = true;
        }

        UIManager.Inst.OpenTycoonMainUI();
    }

    //3D 전투맵 생성 
    private void CreateBattleRoot()
    {
        if (Prefab_BattleRoot == null)
        {
            Debug.LogWarning("[ObjectManager] 전투 공간 프리팹 인스펙터 할당 안 됨");
            return;
        }

        if (_battleRoot != null)
        {
            return;
        }

        _battleRoot = Instantiate(Prefab_BattleRoot);
        _battleRoot.name = "BattleRoot";

        BindBattleExecutors();

        _battleRoot.SetActive(false);
        Debug.Log("[ObjectManager] 3D 전투 공간 생성 완료");
    }

    //3D 전투맵 생성 후 필요한 컴포넌트 연결
    private void BindBattleExecutors()
    {
        BattleRootRefs refs = _battleRoot.GetComponentInChildren<BattleRootRefs>(true);
        if (refs == null)
        {
            Debug.LogWarning("[ObjectManager] BattleRootRefs 컴포넌트 없음");
            return;
        }

        if (BattleManager.Inst == null)
        {
            Debug.LogWarning("[ObjectManager] BattleManager 인스턴스 없음");
            return;
        }

        BattleManager.Inst.BindExecutors(refs.HeroExecutor, refs.EnemyExecutor);
        Debug.Log("[ObjectManager] 전투 실행기 연결 완료");
    }

    public async UniTask SpawnHero(string heroId, long roomInstanceId)
    {
        var buildService = GameManager.Inst.Services.BuildService;
        BuildGridViewModel buildVM = buildService.GetBuildGridViewModel();

        List<PlacedRoomData> placedRooms = buildVM.GetPlacedRooms();
        PlacedRoomData targetRoomData = null;

        for (int i = 0; i < placedRooms.Count; i++)
        {
            if (placedRooms[i] != null && placedRooms[i].RoomInstanceId == roomInstanceId)
            {
                targetRoomData = placedRooms[i];
                break;
            }
        }

        GridSystem gridSystem = buildVM.GridSystem;
        Vector3 spawnPosition = gridSystem.GetWorldPosition(targetRoomData.Origin);

        RoomData roomData = GameDataManager.Inst.GetData<RoomData>(targetRoomData.RoomId);
        if (roomData != null)
        {
            Vector2Int size = roomData.GetSize();
            float offsetX = (size.x - 1) * 0.5f * gridSystem.CellWidth;
            float offsetY = (size.y - 1) * 0.5f * gridSystem.CellHeight;

            spawnPosition = new Vector3(spawnPosition.x + offsetX, spawnPosition.y + offsetY, spawnPosition.z);
        }

        GameObject prefab = await ResourceManager.Inst.InstantiateAsync($"Prefabs/Character/Hero/{heroId}");

        if (prefab.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = false;
        }

        prefab.transform.position = spawnPosition;
        prefab.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(spawnPosition);
            agent.SetDestination(spawnPosition);
        }

        HeroMovingAgent movingAgent = prefab.GetComponent<HeroMovingAgent>();

        HeroModel heroModel = new HeroModel();
        heroModel.LoadHeroData(heroId);
        heroModel.RoomInstanceID = roomInstanceId;
        movingAgent.InitHero(heroModel);

        _spawnHero[heroId] = movingAgent;
    }

    public HeroMovingAgent GetSpawnAgent(string heroID)
    {
        if (_spawnHero != null && _spawnHero.TryGetValue(heroID, out HeroMovingAgent agent))
        {
            return agent;
        }

        return null;
    }

    public void DestroyHeroAndMap()
    {
        foreach (var pair in _spawnHero)
        {
            if (pair.Value != null && pair.Value.gameObject != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }

        _spawnHero.Clear();
        _hero = null;

        if (_buildGridView != null)
        {
            Destroy(_buildGridView.gameObject);
            _buildGridView = null;
        }
    }
}