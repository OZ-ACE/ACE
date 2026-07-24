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
