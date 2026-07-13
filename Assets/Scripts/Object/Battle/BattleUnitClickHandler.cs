using System;
using UnityEngine;

// 전투 씬에 배치된 유닛의 클릭/호버 데이터를 들고 있는 컴포넌트 (테스트용 스폰 전용)
public class BattleUnitClickHandler : MonoBehaviour
{
    private const float DefaultColliderRadius = 0.5f;
    private const float DefaultColliderHeight = 2f;
    private const float DefaultColliderCenterY = 1f;
    private const int CapsuleDirectionY = 1; // 캡슐 콜라이더 방향축 - 0:X, 1:Y, 2:Z, 캐릭터가 세로로 서있으므로 Y

    private static readonly Color HighlightColor = Color.white;
    private const string BaseColorPropertyName = "_BaseColor";

    public string UnitId { get; set; }

    public event Action<string> OnUnitClicked;

    private Renderer[] _rendererList;
    private Color[] _originalColorList;
    private bool _isHighlighted;

    private void Awake()
    {
        AttachColliderIfMissing();
        CacheRenderers();
    }

    private void AttachColliderIfMissing()
    {
        Collider existingCollider = GetComponent<Collider>();

        if (existingCollider != null)
        {
            return;
        }

        CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.center = new Vector3(0f, DefaultColliderCenterY, 0f);
        capsuleCollider.radius = DefaultColliderRadius;
        capsuleCollider.height = DefaultColliderHeight;
        capsuleCollider.direction = CapsuleDirectionY;
    }

    //자식에 있는 렌더러들의 원본 색상을 미리 캐싱해둔다. 호버할 때마다 매번 조회하지 않기 위함
    private void CacheRenderers()
    {
        _rendererList = GetComponentsInChildren<Renderer>();
        _originalColorList = new Color[_rendererList.Length];

        for (int i = 0; i < _rendererList.Length; i++)
        {
            _originalColorList[i] = _rendererList[i].material.GetColor(BaseColorPropertyName);
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (_isHighlighted == isHighlighted)
        {
            return;
        }

        _isHighlighted = isHighlighted;

        for (int i = 0; i < _rendererList.Length; i++)
        {
            Color targetColor = isHighlighted ? HighlightColor : _originalColorList[i];
            _rendererList[i].material.SetColor(BaseColorPropertyName, targetColor);
        }
    }

    public void NotifyClicked()
    {
        if (string.IsNullOrEmpty(UnitId))
        {
            Debug.LogWarning("[BattleUnitClickHandler] UnitId가 설정되지 않음");
            return;
        }

        OnUnitClicked?.Invoke(UnitId);
    }
}