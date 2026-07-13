using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshManager : SingletonBase<NavMeshManager>
{
    [SerializeField] private NavMeshSurface _navMeshSurface;

    public void BuildNavMesh()
    {
        _navMeshSurface.BuildNavMesh();
    }
}
