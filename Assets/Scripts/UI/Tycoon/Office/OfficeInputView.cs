using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
// 사무실 3D 사물 클릭 감지 + 호버 하이라이트(스케일). 판단은 하지 않고 이벤트만 발사
public class OfficeInputView : ViewBase
{
    [Header("사무실 카메라")]
    [SerializeField] private Camera Camera_Office;
    [Header("클릭 판정")]
    [SerializeField] private float _maxDistance = 100f;
    [Header("하이라이트")]
    [SerializeField] private float _highlightScale = 1.08f;   // 호버 시 확대 배율
    public event Action<OfficeObjectType> OnClickOfficeObject;
    private OfficeObject _hoveredObject;
    private Vector3 _originalScale;
    private bool _hasHighlight;
    private void Update()
    {
        UpdateHover();
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
    private void OnDisable()
    {
        ClearHighlight();
    }
    //커서 아래 상호작용 오브젝트가 바뀌었을 때만 하이라이트를 갱신한다
    private void UpdateHover()
    {
        OfficeObject target = GetHoveredObject();
        if (target == _hoveredObject)
        {
            return;
        }
        ClearHighlight();
        _hoveredObject = target;
        if (_hoveredObject != null)
        {
            ApplyHighlight(_hoveredObject);
        }
    }
    //현재 커서 아래의 상호작용 가능한 OfficeObject를 반환한다 (UI 위 / 비상호작용은 제외)
    private OfficeObject GetHoveredObject()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() == true)
        {
            return null;
        }
        OfficeObject result;
        if (TryGetClickedObject(out result) == false)
        {
            return null;
        }
        if (result.ObjectType == OfficeObjectType.None)
        {
            return null;
        }
        return result;
    }
    //대상을 살짝 키워서 하이라이트한다 (셰이더 무관)
    private void ApplyHighlight(OfficeObject target)
    {
        _originalScale = target.transform.localScale;
        target.transform.localScale = _originalScale * _highlightScale;
        _hasHighlight = true;
    }
    //하이라이트했던 대상의 스케일을 원래대로 되돌린다
    private void ClearHighlight()
    {
        if (_hasHighlight && _hoveredObject != null)
        {
            _hoveredObject.transform.localScale = _originalScale;
        }
        _hasHighlight = false;
        _hoveredObject = null;
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