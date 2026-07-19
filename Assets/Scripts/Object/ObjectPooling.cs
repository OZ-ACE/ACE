using UnityEngine;
using UnityEngine.Pool;

public class ObjectPooling<T> where T : MonoBehaviour
{
    private readonly IObjectPool<T> _pool;
    private readonly T _prefab;
    private readonly Transform _parent;

    public ObjectPooling (T prefab, Transform parent, int capacity = 10, int maxSize = 30)
    {
        _prefab = prefab;
        _parent = parent;

        _pool = new ObjectPool<T>(CreateObject, GetObject, ReleaseObject, DestroyObject, true, capacity, maxSize);
    }

    private T CreateObject()
    {
        return Object.Instantiate(_prefab, _parent);
    }

    private void GetObject(T prefab)
    {
        prefab.gameObject.SetActive(true);
    }

    private void ReleaseObject(T prefab)
    {
        prefab.gameObject.SetActive(false);
    }

    private void DestroyObject(T prefab)
    {
        Object.Destroy(prefab.gameObject);
    }

    public T Get()
    {
        return _pool.Get();
    }

    public void Release(T prefab)
    {
        _pool.Release(prefab);
    }
}
