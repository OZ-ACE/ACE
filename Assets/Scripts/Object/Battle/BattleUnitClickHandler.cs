using System;
using UnityEngine;

// 전투 씬에 배치된 유닛을 클릭하면 유닛 ID를 알려주는 컴포넌트 (테스트용 스폰 전용
public class BattleUnitClickHandler : MonoBehaviour
{
    private const float DefaultColliderRadius = 0.5f;
    private const float DefaultColliderHeight = 2f;
    private const float DefaultColliderCenterY = 1f;
    private const int CapsuleDirectionY = 1; // 캡슐 콜라이더 방향축 - 0:X, 1:Y, 2:Z, 캐릭터가 세로로 서있으므로 Y

    public string UnitId { get; set; }

    public event Action<string> OnUnitClicked;

    private void Awake()
    {
        AttachColliderIfMissing();
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

    private void OnMouseDown()
    {
        NotifyUnitClicked();
    }

    private void NotifyUnitClicked()
    {
        if (string.IsNullOrEmpty(UnitId))
        {
            Debug.LogWarning("[BattleUnitClickHandler] UnitId가 설정되지 않음");
            return;
        }

        OnUnitClicked?.Invoke(UnitId);
    }
}