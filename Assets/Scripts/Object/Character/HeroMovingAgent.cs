using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class HeroMovingAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent Agent_Hero;
    [SerializeField] private Animator Animator_Hero;

    private IHeroTycoonState _currentState;
    private Dictionary<TycoonState, IHeroTycoonState> _states;

    private GridSystem _gridSystem;
    private HeroModel _heroModel;
    private BuildGridViewModel _buildVM;

    private CancellationTokenSource _movingToken;

    private void Awake()
    {
        _states = new Dictionary<TycoonState, IHeroTycoonState>
        {
            { TycoonState.Idle, new HeroTycoonState_Idle() },
            { TycoonState.Walking, new HeroTycoonState_Walking() },
            { TycoonState.Rest, new HeroTycoonState_Rest() },
            { TycoonState.Gym, new HeroTycoonState_Gym() }
        };
    }

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

    private void Update()
    {
        if (_currentState != null)
        {
            _currentState.Update(this);
        }
    }

    public void ChangeState(TycoonState newState)
    {
        if (_states.ContainsKey(newState) == false)
        {
            return;
        }

        IHeroTycoonState nextState = _states[newState];

        if (_currentState == nextState)
        {
            return;
        }

        _currentState?.Exit(this);
        _currentState = nextState;
        _currentState.Enter(this);
    }

    public void InitHero(HeroModel heroModel)
    {
        _heroModel = heroModel;

        GameManager.Inst.Services.DayService.OnChangeHour += ChangeTargetRoom;

        ChangeState(TycoonState.Idle);
    }

    private void ChangeTargetRoom(int hour)
    {
        ScheduleState state = _heroModel.HourlyStates[hour];
        Vector3 targetPos = GetRoomPosition(state);

        TycoonState nextState = TycoonState.Idle;
        if (state == ScheduleState.Rest)
        {
            nextState = TycoonState.Rest;
        }
        else if (state == ScheduleState.Gym)
        {
            nextState = TycoonState.Gym;
        }

        ChangeState(nextState);
        StartMoving(targetPos, nextState).Forget();
    }

    private async UniTask StartMoving(Vector3 targetPos, TycoonState targetState)
    {
        if (_movingToken != null)
        {
            CancelMoving();
        }

        _movingToken = new CancellationTokenSource();

        ChangeState(TycoonState.Walking);
        SetDestination(targetPos);

        await UniTask.WaitUntil(IsArrived, cancellationToken: _movingToken.Token);

        ChangeState(targetState);
    }

    public void SetDestination(Vector3 targetPos)
    {
        if (Agent_Hero != null && Agent_Hero.isOnNavMesh)
        {
            Agent_Hero.SetDestination(targetPos);
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
        if (!Agent_Hero.pathPending && Agent_Hero.remainingDistance <= Agent_Hero.stoppingDistance)
        {
            if (!Agent_Hero.hasPath || Agent_Hero.velocity.sqrMagnitude == 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
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

    public Animator GetAnimator()
    {
        return Animator_Hero;
    }
}
