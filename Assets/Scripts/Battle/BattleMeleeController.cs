using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//근접 공격 영웅의 타겟 접근 및 원위치 복귀 연출을 담당한다
public class BattleMeleeController : MonoBehaviour
{
    private struct OriginalPose
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [Header("전투 유닛")]
    [SerializeField] private EnemySpawner _enemySpawner;

    [Header("이동 연출")]
    [SerializeField] private float _approachDuration = 0.35f;
    [SerializeField] private float _returnDuration = 0.35f;
    [SerializeField] private float _targetStopDistance = 1.2f;

    private readonly Dictionary<BattleUnitModel, OriginalPose> _originalPoseMap
        = new Dictionary<BattleUnitModel, OriginalPose>();

    //근접 공격 영웅을 타겟 앞까지 이동시킨다
    public async UniTask PlayApproachAsync(
        BattleActionModel action,
        CancellationToken token)
    {
        if (TryGetUnitTransforms(
                action,
                out Transform heroTransform,
                out Transform targetTransform) == false)
        {
            return;
        }

        SaveOriginalPose(action.Unit, heroTransform);

        Vector3 approachPosition = CalculateApproachPosition(
            heroTransform,
            targetTransform);

        Quaternion approachRotation = CalculateTargetRotation(
            approachPosition,
            targetTransform.position,
            heroTransform.rotation);

        await MoveTransformAsync(
            heroTransform,
            approachPosition,
            approachRotation,
            _approachDuration,
            token);
    }

    //근접 공격이 끝난 영웅을 기존 위치와 방향으로 복귀시킨다
    public async UniTask PlayReturnAsync(
        BattleActionModel action,
        CancellationToken token)
    {
        if (action == null || action.Unit == null)
        {
            return;
        }

        if (_originalPoseMap.TryGetValue(
                action.Unit,
                out OriginalPose originalPose) == false)
        {
            return;
        }

        if (BattleHeroSpawner.Inst == null)
        {
            return;
        }

        bool hasHeroTransform = BattleHeroSpawner.Inst.TryGetHeroTransform(
            action.Unit,
            out Transform heroTransform);

        if (hasHeroTransform == false || heroTransform == null)
        {
            return;
        }

        await MoveTransformAsync(
            heroTransform,
            originalPose.Position,
            originalPose.Rotation,
            _returnDuration,
            token);

        _originalPoseMap.Remove(action.Unit);
    }

    //전투 종료나 재진입 시 이동 중이던 영웅을 즉시 원래 자리로 돌린다
    public void RestoreAllImmediately()
    {
        if (BattleHeroSpawner.Inst == null)
        {
            _originalPoseMap.Clear();
            return;
        }

        foreach (KeyValuePair<BattleUnitModel, OriginalPose> pair in _originalPoseMap)
        {
            bool hasHeroTransform = BattleHeroSpawner.Inst.TryGetHeroTransform(
                pair.Key,
                out Transform heroTransform);

            if (hasHeroTransform == false || heroTransform == null)
            {
                continue;
            }

            heroTransform.SetPositionAndRotation(
                pair.Value.Position,
                pair.Value.Rotation);
        }

        _originalPoseMap.Clear();
    }

    private bool TryGetUnitTransforms(
        BattleActionModel action,
        out Transform heroTransform,
        out Transform targetTransform)
    {
        heroTransform = null;
        targetTransform = null;

        if (action == null ||
            action.Unit == null ||
            action.Target == null ||
            action.Unit.IsHero == false ||
            action.Target.IsHero)
        {
            return false;
        }

        if (BattleHeroSpawner.Inst == null || _enemySpawner == null)
        {
            return false;
        }

        bool hasHeroTransform = BattleHeroSpawner.Inst.TryGetHeroTransform(
            action.Unit,
            out heroTransform);

        bool hasTargetTransform = _enemySpawner.TryGetEnemyTransform(
            action.Target,
            out targetTransform);

        return hasHeroTransform &&
               hasTargetTransform &&
               heroTransform != null &&
               targetTransform != null;
    }

    private void SaveOriginalPose(
        BattleUnitModel heroUnit,
        Transform heroTransform)
    {
        if (_originalPoseMap.ContainsKey(heroUnit))
        {
            return;
        }

        OriginalPose originalPose = new OriginalPose();
        originalPose.Position = heroTransform.position;
        originalPose.Rotation = heroTransform.rotation;

        _originalPoseMap.Add(heroUnit, originalPose);
    }

    private Vector3 CalculateApproachPosition(
        Transform heroTransform,
        Transform targetTransform)
    {
        Vector3 targetToHeroDirection =
            heroTransform.position - targetTransform.position;

        targetToHeroDirection.y = 0f;

        if (targetToHeroDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            targetToHeroDirection = -targetTransform.forward;
            targetToHeroDirection.y = 0f;
        }

        Vector3 approachPosition =
            targetTransform.position +
            targetToHeroDirection.normalized * _targetStopDistance;

        approachPosition.y = heroTransform.position.y;

        return approachPosition;
    }

    private Quaternion CalculateTargetRotation(
        Vector3 currentPosition,
        Vector3 targetPosition,
        Quaternion fallbackRotation)
    {
        Vector3 targetDirection = targetPosition - currentPosition;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return fallbackRotation;
        }

        return Quaternion.LookRotation(targetDirection.normalized);
    }

    private async UniTask MoveTransformAsync(
        Transform targetTransform,
        Vector3 destinationPosition,
        Quaternion destinationRotation,
        float duration,
        CancellationToken token)
    {
        if (targetTransform == null)
        {
            return;
        }

        if (duration <= 0f)
        {
            targetTransform.SetPositionAndRotation(
                destinationPosition,
                destinationRotation);

            return;
        }

        Vector3 startPosition = targetTransform.position;
        Quaternion startRotation = targetTransform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(
                elapsedTime / duration);

            targetTransform.position = Vector3.Lerp(
                startPosition,
                destinationPosition,
                normalizedTime);

            targetTransform.rotation = Quaternion.Slerp(
                startRotation,
                destinationRotation,
                normalizedTime);

            await UniTask.Yield(
                PlayerLoopTiming.Update,
                token);
        }

        targetTransform.SetPositionAndRotation(
            destinationPosition,
            destinationRotation);
    }
}