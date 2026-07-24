using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BattleVfxController : MonoBehaviour
{
    private const string CommonHitVfxAddress = "BattleVFX_CommonHit";
    private const string EnemyMuzzleVfxAddress = "BattleVFX_Enemy01Muzzle";

    [Header("전투 유닛 참조")]
    [SerializeField] private EnemySpawner _enemySpawner;

    [Header("투사체 VFX")]
    [SerializeField] private string _magicArrowVfxAddress;
    [SerializeField] private string _fireballVfxAddress;
    [SerializeField] private float _projectileMoveDuration = 0.5f;
    [SerializeField] private float _fireballLaunchDelay = 0.4f;

    private const string MagicArrowSkillId = "heroSkill_03_01";
    private const string FireBallSkillId = "heroSkill_04_01";
    private const string HealingBuffVfxAddress = "BattleVFX_HealingBuff";

    public async UniTask PlayCommonHitVfxAsync(BattleUnitModel unit)
    {
        if (TryGetUnitVfxPoint(unit, out Transform vfxPoint) == false)
        {
            return;
        }

        await PlayWorldParticleVfxAsync(
            CommonHitVfxAddress,
            vfxPoint.position,
            vfxPoint.rotation);
    }

    public async UniTask PlayEnemyMuzzleVfxAsync(BattleUnitModel unit)
    {
        if (unit == null || unit.IsHero)
        {
            return;
        }

        if (_enemySpawner == null)
        {
            return;
        }

        bool hasMuzzlePoint = _enemySpawner.TryGetMuzzlePoint(
            unit,
            out Transform muzzlePoint);

        if (hasMuzzlePoint == false || muzzlePoint == null)
        {
            return;
        }

        await PlayChildParticleVfxAsync(
            EnemyMuzzleVfxAddress,
            muzzlePoint);
    }
   
    public async UniTask PlayHealVfxAsync(BattleUnitModel unit)
    {
        if (TryGetUnitVfxPoint(unit, out Transform vfxPoint) == false)
        {
            return;
        }

        await PlayChildParticleVfxAsync(
            HealingBuffVfxAddress,
            vfxPoint);
    }

    public async UniTask PlayProjectileVfxAsync(BattleActionModel action)
    {
        if (action == null || action.Unit == null || action.Target == null)
        {
            return;
        }

        string vfxAddress = GetProjectileVfxAddress(action.SkillId);

        if (string.IsNullOrEmpty(vfxAddress))
        {
            return;
        }

        float launchDelay = GetProjectileLaunchDelay(action.SkillId);

        if (launchDelay > 0f)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(launchDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();
        }

        bool hasStartPoint = TryGetUnitVfxPoint(
            action.Unit,
            out Transform startPoint);

        bool hasTargetPoint = TryGetUnitVfxPoint(
            action.Target,
            out Transform targetPoint);

        if (hasStartPoint == false ||
            hasTargetPoint == false ||
            startPoint == null ||
            targetPoint == null)
        {
            return;
        }

        await PlayMovingProjectileVfxAsync(
            vfxAddress,
            startPoint.position,
            targetPoint);
    }

    private string GetProjectileVfxAddress(string skillId)
    {
        switch (skillId)
        {
            case MagicArrowSkillId:
                return _magicArrowVfxAddress;

            case FireBallSkillId:
                return _fireballVfxAddress;

            default:
                return null;
        }
    }

    private float GetProjectileLaunchDelay(string skillId)
    {
        switch (skillId)
        {
            case FireBallSkillId:
                return _fireballLaunchDelay;

            default:
                return 0f;
        }
    }

    private bool TryGetUnitVfxPoint(
        BattleUnitModel unit,
        out Transform vfxPoint)
    {
        vfxPoint = null;

        if (unit == null)
        {
            return false;
        }

        if (unit.IsHero)
        {
            if (BattleHeroSpawner.Inst == null)
            {
                return false;
            }

            return BattleHeroSpawner.Inst.TryGetVfxPoint(
                unit,
                out vfxPoint);
        }

        if (_enemySpawner == null)
        {
            return false;
        }

        return _enemySpawner.TryGetVfxPoint(
            unit,
            out vfxPoint);
    }
    
    private async UniTask PlayWorldParticleVfxAsync(
        string vfxAddress,
        Vector3 position,
        Quaternion rotation)
    {
        if (string.IsNullOrEmpty(vfxAddress))
        {
            return;
        }

        GameObject vfxObject = await ResourceManager.Inst.InstantiateAsync(
            vfxAddress);

        if (vfxObject == null)
        {
            return;
        }

        vfxObject.transform.SetPositionAndRotation(
            position,
            rotation);

        await PlayAndReleaseParticleVfxAsync(vfxObject);
    }
    
    private async UniTask PlayChildParticleVfxAsync(
        string vfxAddress,
        Transform parent)
    {
        if (string.IsNullOrEmpty(vfxAddress) || parent == null)
        {
            return;
        }

        GameObject vfxObject = await ResourceManager.Inst.InstantiateAsync(
            vfxAddress,
            parent);

        if (vfxObject == null)
        {
            return;
        }

        vfxObject.transform.localPosition = Vector3.zero;
        vfxObject.transform.localRotation = Quaternion.identity;

        await PlayAndReleaseParticleVfxAsync(vfxObject);
    }
    
    private async UniTask PlayMovingProjectileVfxAsync(
        string vfxAddress,
        Vector3 startPosition,
        Transform targetPoint)
    {
        if (string.IsNullOrEmpty(vfxAddress) || targetPoint == null)
        {
            return;
        }

        GameObject vfxObject = await ResourceManager.Inst.InstantiateAsync(vfxAddress);

        if (vfxObject == null)
        {
            return;
        }

        vfxObject.transform.position = startPosition;
        PrepareOneShotParticleVfx(vfxObject);

        float moveDuration = Mathf.Max(
            _projectileMoveDuration,
            0.01f);

        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < moveDuration)
            {
                if (targetPoint == null)
                {
                    return;
                }

                Vector3 targetPosition = targetPoint.position;
                Vector3 direction = targetPosition - vfxObject.transform.position;

                if (direction.sqrMagnitude > 0f)
                {
                    vfxObject.transform.rotation = Quaternion.LookRotation(direction);
                }

                elapsedTime += Time.deltaTime;

                float moveRatio = Mathf.Clamp01(
                    elapsedTime / moveDuration);

                vfxObject.transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    moveRatio);

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    this.GetCancellationTokenOnDestroy());
            }
        }
        finally
        {
            if (vfxObject != null)
            {
                Addressables.ReleaseInstance(vfxObject);
            }
        }
    }

    private async UniTask PlayAndReleaseParticleVfxAsync(GameObject vfxObject)
    {
        PrepareOneShotParticleVfx(vfxObject);

        float playDuration = GetParticlePlayDuration(vfxObject);

        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(playDuration),
                cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();
        }
        finally
        {
            if (vfxObject != null)
            {
                Addressables.ReleaseInstance(vfxObject);
            }
        }
    }
    
    private void PrepareOneShotParticleVfx(GameObject vfxObject)
    {
        ParticleSystem[] particleSystems =
            vfxObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = false;

            particleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            particleSystem.Play(true);
        }
    }

    private float GetParticlePlayDuration(GameObject vfxObject)
    {
        const float MinimumDuration = 0.1f;

        float maxDuration = MinimumDuration;

        ParticleSystem[] particleSystems =
            vfxObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            float duration =
                main.duration +
                main.startLifetime.constantMax;

            if (duration > maxDuration)
            {
                maxDuration = duration;
            }
        }

        return maxDuration;
    }
}
