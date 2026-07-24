using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class HeroMovingAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent Agent_Hero;
    [SerializeField] private Animator Animator_Hero;

    private HeroModel _heroModel;
    public HeroModel HeroModel
    {
        get => _heroModel;
    }

    private IHeroTycoonState _currentState;
    private Dictionary<TycoonState, IHeroTycoonState> _states;

    private GridSystem _gridSystem;
    private BuildGridViewModel _buildVM;
    private PlacedRoomData _currentRoom;

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
        RefreshBuildViewModel();
    }

    private void OnDisable()
    {
        CancelMoving();
    }

    private void OnDestroy()
    {
        if (_heroModel != null)
        {
            _heroModel.OnUpdateSchedule -= UpdateSchedule;
        }

        if (GameManager.Inst.Services.DayService != null)
        {
            GameManager.Inst.Services.DayService.OnChangeHour -= ChangeTargetRoom;
        }
    }

    private void Update()
    {
        _currentState?.Update(this);
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

        _heroModel.OnUpdateSchedule += UpdateSchedule;
        GameManager.Inst.Services.DayService.OnChangeHour += ChangeTargetRoom;

        ChangeState(TycoonState.Idle);
        UpdateSchedule();
    }

    private void UpdateSchedule()
    {
        int currentHour = GameManager.Inst.Services.DayService.CurrentHour;
        ChangeTargetRoom(currentHour);
    }

    private void RefreshBuildViewModel()
    {
        var buildService = GameManager.Inst.Services.BuildService;
        if (buildService != null)
        {
            _buildVM = buildService.GetBuildGridViewModel();

            if (_buildVM != null)
            {
                _gridSystem = _buildVM.GridSystem;
            }
        }
    }

    private void ChangeTargetRoom(int hour)
    {
        ScheduleState state = _heroModel.HourlyStates[hour];

        LeaveCurrentRoom();

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
        else if (state == ScheduleState.Sleep)
        {
            nextState = TycoonState.Idle;
        }

        if (targetPos == Vector3.zero)
        {
            Debug.Log($"방 없음 {_heroModel.Name}");
            ApplyPenalty(state, 3, 0);

            ChangeState(TycoonState.Idle);
            return;
        }

        if (IsPathInvalid(targetPos))
        {
            Debug.Log($"끊어진 길 {_heroModel.Name}");
            ApplyPenalty(state,0, 3);

            if (Agent_Hero.isOnNavMesh)
            {
                Agent_Hero.ResetPath();
            }

            LeaveCurrentRoom();

            ChangeState(TycoonState.Idle);
            return;
        }

        StartMoving(targetPos, nextState).Forget();
    }

    private void ApplyPenalty(ScheduleState state, int penaltyAffection, int penaltySatisfaction)
    {
        _heroModel.Affection = Mathf.Max(0, _heroModel.Affection - penaltyAffection);
        _heroModel.Satisfaction = Mathf.Max(0, _heroModel.Satisfaction - penaltySatisfaction);

        _heroModel.SaveHeroProgress();

        string heroName = _heroModel.Name;
        string actionName = GetActionText(state);
        string property = penaltyAffection == 0 ? "만족도" : "호감도";
        int penalty = penaltyAffection == 0 ? penaltySatisfaction : penaltyAffection;
        string text = $"{heroName}이(가) {actionName}을(를) 수행할 수 있는 방이 없습니다.\n{property}가 -{penalty} 하락합니다.";

        UIManager.Inst.OpenInfoText(text);
    }

    private string GetActionText(ScheduleState state)
    {
        switch (state)
        {
            case ScheduleState.Sun: return "일광욕";
            case ScheduleState.Sleep: return "수면";
            case ScheduleState.Shower: return "샤워";
            case ScheduleState.Rest: return "휴식";
            case ScheduleState.Gym: return "운동";
            case ScheduleState.Counsel: return "상담";
            case ScheduleState.Cure: return "휴식";
            case ScheduleState.Meal: return "식사";
            default: return "활동";
        }
    }

    private void LeaveCurrentRoom()
    {
        if (_currentRoom != null)
        {
            _currentRoom.UnregisterUser(_heroModel.HeroID);
            _currentRoom = null;
        }
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
        RefreshBuildViewModel();

        if (_buildVM == null)
        {
            return Vector3.zero;
        }

        List<PlacedRoomData> placedRooms = _buildVM.GetPlacedRooms();

        foreach (PlacedRoomData data in placedRooms)
        {
            bool isTargetRoom = false;

            if (state == ScheduleState.Sleep)
            {
                if (data.RoomInstanceId.ToString() == _heroModel.RoomInstanceID.ToString() && _heroModel.RoomInstanceID != 0)
                {
                    isTargetRoom = true;
                }
            }
            else
            {
                if (data.RoomId.Contains(state.ToString()) && data.CanUse())
                {
                    isTargetRoom = true;
                }
            }

            if (isTargetRoom)
            {
                Vector3 originWorld = _gridSystem.GetWorldPosition(data.Origin);
                RoomData roomData = GameDataManager.Inst.GetData<RoomData>(data.RoomId);

                Vector3 targetPos = originWorld;

                if (roomData != null)
                {
                    Vector2 size = roomData.GetSize();
                    float totalWidth = size.x * _gridSystem.CellWidth;

                    float offsetX = _gridSystem.CellWidth * 0.4f;
                    float randomX = Random.Range(offsetX, totalWidth - offsetX);

                    Vector3 calculatedPos = new Vector3(originWorld.x + randomX, originWorld.y, originWorld.z);

                    if (NavMesh.SamplePosition(calculatedPos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                    {
                        targetPos = hit.position;
                    }
                    else
                    {
                        targetPos = calculatedPos;
                    }
                }

                data.RegisterUser(_heroModel.HeroID);
                _currentRoom = data;

                return targetPos;
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

    private bool IsPathInvalid(Vector3 targetPos)
    {
        NavMeshPath path = new NavMeshPath();

        Agent_Hero.CalculatePath(targetPos, path);

        if (path.status == NavMeshPathStatus.PathPartial || path.status == NavMeshPathStatus.PathInvalid)
        {
            return true;
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
