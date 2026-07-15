using UnityEngine;

// 사무실 3D 사물. 자기 종류만 들고 있고 로직은 없음
public class OfficeObject : MonoBehaviour
{
    [SerializeField] private OfficeObjectType _objectType = OfficeObjectType.None;

    public OfficeObjectType ObjectType { get { return _objectType; } }
}