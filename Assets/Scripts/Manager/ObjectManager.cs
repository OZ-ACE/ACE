using Cysharp.Threading.Tasks;
using UnityEngine;


public class ObjectManager : SingletonBase<ObjectManager>
{
    [Header("건설 격자 뷰 프리팹")]
    [SerializeField] private GameObject Prefab_BuildGridView;

    [Header("3D 사무실 루트 프리팹")]
    [SerializeField] private GameObject Prefab_OfficeRoot;


    private BuildGridView _buildGridView;
    private GameObject _hero;

    private GameObject _officeRoot;
    private OfficeInputView _officeInputView;
    private Camera _mainCamera;

    public BuildGridView BuildGridView { get { return _buildGridView; } }

    private void Start()
    {
        _mainCamera = Camera.main;
        //CreateBuildGridView();
        CreateOfficeRoot();
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

        _buildGridView.Bind(viewModel);
        Debug.Log("[ObjectManager] 건설 격자 생성 완료");
    }

    private void OnClickOffice()
    {
        EnterOffice();
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

            case OfficeObjectType.Battle:
                GameManager.Inst.Services.DayService.MarkBattleDone();
                break;

            case OfficeObjectType.Admission:
                UIManager.Inst.OpenAdmissionPopup();
                break;

            case OfficeObjectType.Close:
                ExitOffice();
                break;
        }
    }



    ////////////////////////////////////////

    // 테스트용 임시 메서드
    public async UniTask SpawnHero(string heroID)
    {
        GameObject prefab = await ResourceManager.Inst.InstantiateAsync($"Prefabs/Character/Hero/{heroID}");
        _hero = prefab;
    }

    public void DestroyHeroAndMap()
    {
        Destroy(_hero);
        Destroy(_buildGridView.gameObject);
    }
}