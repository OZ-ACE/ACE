using Cysharp.Threading.Tasks;
using UnityEngine;


public class ObjectManager : SingletonBase<ObjectManager>
{
    [Header("건설 격자 뷰 프리팹")]
    [SerializeField] private GameObject Prefab_BuildGridView;

    private BuildGridView _buildGridView;

    public BuildGridView BuildGridView { get { return _buildGridView; } }

    private void Start()
    {
        //CreateBuildGridView();
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

        _buildGridView.Bind(GameManager.Inst.Services.BuildService.GetBuildGridViewModel());
        Debug.Log("[ObjectManager] 건설 격자 생성 완료");
    }

    // 테스트용 임시 메서드
    public async UniTask SpawnHero(string heroID)
    {
        GameObject prefab = await ResourceManager.Inst.InstantiateAsync($"Prefabs/Character/Hero/{heroID}");
    }
}