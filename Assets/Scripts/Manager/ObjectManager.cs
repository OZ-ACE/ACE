using Cysharp.Threading.Tasks;
using UnityEngine;


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
                if (GameManager.Inst.Services.DayService.IsAdvanceable() == false)
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





    ////////////////////////////////////////

    // 테스트용 임시 메서드
    public async UniTask SpawnHero(string heroID)
    {
        GameObject prefab = await ResourceManager.Inst.InstantiateAsync($"Prefabs/Character/Hero/{heroID}");

        HeroMovingAgent movingAget = prefab.GetComponent<HeroMovingAgent>();
        HeroModel heroData = new HeroModel();

        for (int i = 0; i < 23; i++)
        {
            heroData.HourlyStates[i] = ScheduleState.Shower;
        }

        movingAget.InitHero(heroData);
    }

    public void DestroyHeroAndMap()
    {
        Destroy(_hero);
        Destroy(_buildGridView.gameObject);
    }
}