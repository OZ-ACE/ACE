using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HeroMovingAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent HeroAgent;
    [SerializeField] private float MinDelay = 3f;
    [SerializeField] private float MaxDelay = 7f;

    private GridSystem _gridSystem;
    private CancellationTokenSource _movingToken;

    private void OnEnable()
    {
        _gridSystem = GameManager.Inst.Services.BuildService.GetBuildGridViewModel().GridSystem;

        StartMoving().Forget();
    }

    private void OnDisable()
    {
        CancelMoving();
    }

    private async UniTask StartMoving()
    {
        if (_movingToken != null)
        {
            CancelMoving();
        }

        _movingToken = new CancellationTokenSource();

        BuildGridData gridData = SaveManager.Inst.CurrentPlayerModel.BuildGridData;
        List<PlacedRoomData> placedRooms = gridData.PlacedRooms;

        while (!_movingToken.Token.IsCancellationRequested)
        {
            if (placedRooms.Count == 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(MinDelay), cancellationToken: _movingToken.Token);
                continue;
            }

            int randomIndex = Random.Range(0, placedRooms.Count);
            PlacedRoomData targetRoom = placedRooms[randomIndex];

            RoomData room = GameDataManager.Inst.GetData<RoomData>(targetRoom.RoomId);

            Vector3 targetPos = GetRoomCenterPosition(targetRoom.Origin, room.GetSize());

            SetDestination(targetPos);

            while (!IsArrived())
            {
                if (HeroAgent.hasPath && HeroAgent.velocity.sqrMagnitude > 0.01f)
                {
                    SetDirection(HeroAgent.velocity.x);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: _movingToken.Token);
            }

            await UniTask.WaitUntil(IsArrived, cancellationToken: _movingToken.Token);
            
            float randomDelay = Random.Range(MinDelay, MaxDelay);
            await UniTask.Delay(TimeSpan.FromSeconds(randomDelay), cancellationToken: _movingToken.Token);
        }
    }

    public void SetDestination(Vector3 targetPos)
    {
        if (HeroAgent != null && HeroAgent.isOnNavMesh)
        {
            HeroAgent.SetDestination(targetPos);
        }
    }

    private void SetDirection(float dir)
    {
        if (Mathf.Abs(dir) < 0.01f)
        {
            return;
        }

        Vector3 scale = transform.localScale;

        if (dir > 0f)
        {
            scale.x = Math.Abs(scale.x);
        }
        else if (dir < 0f)
        {
            scale.x = -Math.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    private Vector3 GetRoomCenterPosition(GridCoord origin, Vector2 size)
    {
        Vector3 originPos = _gridSystem.GetWorldPosition(origin);

        float offsetX = (size.x - 1) * 0.5f * _gridSystem.CellWidth;
        float offsetY = (size.y - 1) * 0.5f * _gridSystem.CellHeight;

        return new Vector3(originPos.x + offsetX, originPos.y + offsetY, 0f);
    }

    private bool IsArrived()
    {
        if (!HeroAgent.pathPending && HeroAgent.remainingDistance <= HeroAgent.stoppingDistance)
        {
            if (!HeroAgent.hasPath || HeroAgent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }

        return false;
    }

    private void CancelMoving()
    {
        if (_movingToken != null)
        {
            _movingToken.Cancel();
            _movingToken.Dispose();
            _movingToken = null;
        }
    }
}
