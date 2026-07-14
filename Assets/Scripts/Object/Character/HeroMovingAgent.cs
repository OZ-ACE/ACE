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

        while (!_movingToken.Token.IsCancellationRequested)
        {
            var currentRooms = GameManager.Inst.Services.BuildService.GetBuildGridViewModel().GetPlacedRooms();

            if (currentRooms.Count == 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: _movingToken.Token);
                continue;
            }

            int randomIndex = Random.Range(0, currentRooms.Count);
            PlacedRoomData targetRoom = currentRooms[randomIndex];
            RoomData room = GameDataManager.Inst.GetData<RoomData>(targetRoom.RoomId);

            if (room == null)
            {
                await UniTask.DelayFrame(1, cancellationToken: _movingToken.Token);
                continue;
            }

            Vector3 targetPos = GetRoomCenterPosition(targetRoom.Origin, room.GetSize());

            SetDestination(targetPos);

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
