using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshManager : SingletonBase<NavMeshManager>
{
    [SerializeField] private NavMeshSurface NavMeshSurface;

    public void UpdateNavMesh()
    {
        NavMeshSurface.UpdateNavMesh(NavMeshSurface.navMeshData);
    }

    public void BuildNavMesh()
    {
        NavMeshSurface.BuildNavMesh();
    }
}
