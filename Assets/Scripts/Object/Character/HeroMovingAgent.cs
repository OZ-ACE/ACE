using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class HeroMovingAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent HeroAgent;
    [SerializeField] private float MinDelay = 0.5f;
    [SerializeField] private float MaxDelay = 0.3f;

    private GridSystem _gridSystem;
    private HeroModel _heroModel;
    private BuildGridViewModel _buildVM;

    private CancellationTokenSource _movingToken;
    private int _cancelMovingHour = -1;

    private void OnEnable()
    {
        _buildVM = GameManager.Inst.Services.BuildService.GetBuildGridViewModel();
        _gridSystem = _buildVM.GridSystem;
    }

    private void OnDisable()
    {
        GameManager.Inst.Services.DayService.OnChangeHour -= ChangeTargetRoom;
        CancelMoving();
    }

    public void InitHero(HeroModel heroModel)
    {
        _heroModel = heroModel;

        GameManager.Inst.Services.DayService.OnChangeHour += ChangeTargetRoom;
    }

    private void ChangeTargetRoom(int hour)
    {
        ScheduleState state = _heroModel.HourlyStates[hour];

        Vector3 targetPos = GetRoomPosition(state);

        StartMoving(targetPos).Forget();
    }

    private async UniTask StartMoving(Vector3 targetPos)
    {
        if (_movingToken != null)
        {
            CancelMoving();
        }

        _movingToken = new CancellationTokenSource();

        SetDestination(targetPos);

        await UniTask.WaitUntil(IsArrived, cancellationToken: _movingToken.Token);
    }

    public void SetDestination(Vector3 targetPos)
    {
        if (HeroAgent != null && HeroAgent.isOnNavMesh)
        {
            HeroAgent.SetDestination(targetPos);
        }
    }

    public Vector3 GetRoomPosition(ScheduleState state)
    {
        if (_buildVM == null)
        {
            return Vector3.zero;
        }

        List<PlacedRoomData> placedRooms = _buildVM.GetPlacedRooms();

        foreach (PlacedRoomData data in placedRooms)
        {
            if (data.RoomId.Contains($"{state}"))
            {
                Vector3 originWorld = _gridSystem.GetWorldPosition(data.Origin);

                RoomData roomData = GameDataManager.Inst.GetData<RoomData>(data.RoomId);
                if (roomData != null)
                {
                    Vector2Int size = roomData.GetSize();
                    float offsetX = (size.x - 1) * 0.5f * _gridSystem.CellWidth;
                    float offsetY = (size.y - 1) * 0.5f * _gridSystem.CellHeight;

                    Vector3 centerPos = new Vector3(originWorld.x + offsetX, originWorld.y + offsetY, 0f);
                    return centerPos;
                }

                return originWorld;
            }
        }

        return Vector3.zero;
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
