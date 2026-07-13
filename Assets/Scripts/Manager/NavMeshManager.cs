using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshManager : SingletonBase<NavMeshManager>
{
    [SerializeField] private NavMeshSurface NavMeshSurface;

    public void BuildNavMesh()
    {
        NavMeshSurface.BuildNavMesh();
    }
}
