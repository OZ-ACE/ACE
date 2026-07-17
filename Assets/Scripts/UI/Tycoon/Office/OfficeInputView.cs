using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// 사무실 3D 사물 클릭 감지. 판단은 하지 않고 이벤트만 발사
public class OfficeInputView : ViewBase
{
    [Header("사무실 카메라")]
    [SerializeField] private Camera Camera_Office;

    [Header("클릭 판정")]
    [SerializeField] private float _maxDistance = 100f;

    public event Action<OfficeObjectType> OnClickOfficeObject;

    private void Update()
    {
        if (IsClickedThisFrame() == false)
        {
            return;
        }

        OfficeObject officeObject;
        if (TryGetClickedObject(out officeObject) == false)
        {
            return;
        }

        if (OnClickOfficeObject != null)
        {
            OnClickOfficeObject.Invoke(officeObject.ObjectType);
        }
    }

    //왼쪽 클릭 여부 및 UI 위 클릭 판정
    private bool IsClickedThisFrame()
    {
        if (Mouse.current == null)
        {
            return false;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame == false)
        {
            return false;
        }
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return false;
        }
        return true;
    }

    //마우스 위치의 사무실 사물을 Raycast로 검출
    private bool TryGetClickedObject(out OfficeObject officeObject)
    {
        officeObject = null;

        if (Camera_Office == null || Mouse.current == null)
        {
            return false;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera_Office.ScreenPointToRay(mousePos);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, _maxDistance) == false)
        {
            return false;
        }

        officeObject = hit.collider.GetComponentInParent<OfficeObject>();
        if (officeObject == null)
        {
            return false;
        }

        return true;
    }
}