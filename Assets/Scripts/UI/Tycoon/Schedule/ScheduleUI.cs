using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleUI : UIBase
{
    [SerializeField] private GameObject Prefab_TimeSlot;
    [SerializeField] private Transform Transform_Parent;

    [Header("행동 버튼")]
    [SerializeField] private Button Button_Shower;
    [SerializeField] private Button Button_Rest;
    [SerializeField] private Button Button_Sun;
    [SerializeField] private Button Button_Meal;
    [SerializeField] private Button Button_Counsel;
    [SerializeField] private Button Button_Cure;
    [SerializeField] private Button Button_Sleep;
    [SerializeField] private Button Button_Gym;

    [SerializeField] private Button Button_One;
    [SerializeField] private Button Button_All;
    [SerializeField] private Button Button_Close;

    [Header("버튼 색")]
    [SerializeField] private Color Color_Shower = new Color(0.98f, 0.50f, 0.45f);
    [SerializeField] private Color Color_Sun = new Color(0.60f, 0.85f, 0.60f);
    [SerializeField] private Color Color_Counsel = new Color(0.96f, 0.87f, 0.70f);
    [SerializeField] private Color Color_Cure = new Color(0.25f, 0.88f, 0.82f);
    [SerializeField] private Color Color_Gym = new Color(1.00f, 0.84f, 0.00f);
    [SerializeField] private Color Color_Rest = new Color(0.3f, 0.91f, 0.1f);
    [SerializeField] private Color Color_Sleep = new Color(0.50f, 1.00f, 0.83f);
    [SerializeField] private Color Color_Meal = new Color(0.87f, 0.63f, 0.87f);

    private HeroViewModel _targetHeroVM;
    private ScheduleViewModel _scheduleVM;
    private List<ScheduleSlot> _slots = new List<ScheduleSlot>();
    private Dictionary<ScheduleState, Button> _buttons = new Dictionary<ScheduleState, Button>();

    public ScheduleState CurrentSelectedToolState
    {
        get => _scheduleVM.SelectedState;
    }

    private void Awake()
    {
        _scheduleVM = new ScheduleViewModel();

        Button_One.onClick.AddListener(OnClickApplyOne);
        Button_All.onClick.AddListener(OnClickApplyAll);
        Button_Close.onClick.AddListener(OnClickClose);

        Button_Shower.onClick.AddListener(OnClickShower);
        Button_Rest.onClick.AddListener(OnClickRest);
        Button_Sun.onClick.AddListener(OnClickSun);
        Button_Meal.onClick.AddListener(OnClickMeal);
        Button_Counsel.onClick.AddListener(OnClickCounsel);
        Button_Cure.onClick.AddListener(OnClickCure);
        Button_Sleep.onClick.AddListener(OnClickSleep);
        Button_Gym.onClick.AddListener(OnClickGym);

        InitButtons();
        CreateTimeSlot();
    }

    private void InitButtons()
    {
        _buttons.Add(ScheduleState.Rest, Button_Rest);
        _buttons.Add(ScheduleState.Gym, Button_Gym);
        _buttons.Add(ScheduleState.Meal, Button_Meal);
        _buttons.Add(ScheduleState.Counsel, Button_Counsel);
        _buttons.Add(ScheduleState.Sun, Button_Sun);
        _buttons.Add(ScheduleState.Cure, Button_Cure);
        _buttons.Add(ScheduleState.Sleep, Button_Sleep);
        _buttons.Add(ScheduleState.Shower, Button_Shower);
    }

    public void OpenSchedule(HeroViewModel targetVM, int currentInGameHour)
    {
        _targetHeroVM = targetVM;

        _scheduleVM.Init(_targetHeroVM.Model, currentInGameHour);
        SetSelectedState(ScheduleState.Sleep);
    }

    private void CreateTimeSlot()
    {
        for (int i = 0; i < 24; i++)
        {
            GameObject slot = Instantiate(Prefab_TimeSlot, Transform_Parent);
            ScheduleSlot scheduleSlot = slot.GetComponent<ScheduleSlot>();

            scheduleSlot.InitSlot(i, _scheduleVM, this);
            _slots.Add(scheduleSlot);
        }
    }

    public Color GetSlotColor(ScheduleState state)
    {
        switch (state)
        {
            case ScheduleState.Shower:
                return Color_Shower;

            case ScheduleState.Sun:
                return Color_Sun;

            case ScheduleState.Counsel:
                return Color_Counsel;

            case ScheduleState.Cure:
                return Color_Cure;

            case ScheduleState.Gym:
                return Color_Gym;

            case ScheduleState.Rest:
                return Color_Rest;

            case ScheduleState.Sleep:
                return Color_Sleep;

            case ScheduleState.Meal:
                return Color_Meal;

            case ScheduleState.None:
                return Color.white;

            default:
                return Color.white;
        }
    }

    private void UpdateButtonColor(ScheduleState activeSlot)
    {
        foreach (var button in _buttons)
        {
            button.Value.image.color = button.Key == activeSlot ? GetSlotColor(activeSlot) : Color.white;
        }
    }

    private void SetSelectedState(ScheduleState state)
    {
        _scheduleVM.SelectedState = state;

        UpdateButtonColor(state);
    }

    private void OnClickApplyOne()
    {
        _scheduleVM.SaveAndApplyOne();
        UIManager.Inst.CloseScheduleUI();
    }

    private void OnClickApplyAll()
    {
        List<HeroModel> activeHeroes = GetActiveHeroModels();

        if (activeHeroes.Count == 0)
        {
            return;
        }

        _scheduleVM.SaveAndApplyAll(activeHeroes);
        UIManager.Inst.CloseScheduleUI();
    }

    private void OnClickClose()
    {
        UIManager.Inst.CloseScheduleUI();
    }

    private List<HeroModel> GetActiveHeroModels()
    {
        List<HeroModel> heroModels = new List<HeroModel>();
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;
        RoomAssignmentService roomService = GameManager.Inst.Services.RoomAssignmentService;

        if (playerModel == null || playerModel.HeroStats == null)
        {
            return heroModels;
        }

        HashSet<string> pendingHeroIds = new HashSet<string>();
        if (playerModel.PendingHeroes != null)
        {
            for (int i = 0; i < playerModel.PendingHeroes.Count; i++)
            {
                var pending = playerModel.PendingHeroes[i];

                if (pending != null && !string.IsNullOrEmpty(pending.HeroID))
                {
                    pendingHeroIds.Add(pending.HeroID);
                }
            }
        }

        foreach (var heroStat in playerModel.HeroStats)
        {
            string heroId = heroStat.HeroID;

            if (roomService != null && roomService.IsHeroAssigned(heroId) == false)
            {
                continue;
            }

            if (pendingHeroIds.Contains(heroId))
            {
                continue;
            }

            var spawnedAgent = ObjectManager.Inst.GetSpawnAgent(heroId);

            if (spawnedAgent != null && spawnedAgent.HeroModel != null)
            {
                heroModels.Add(spawnedAgent.HeroModel);
            }
        }

        return heroModels;
    }

    private void OnClickShower() => SetSelectedState(ScheduleState.Shower);
    private void OnClickRest() => SetSelectedState(ScheduleState.Rest);
    private void OnClickSun() => SetSelectedState(ScheduleState.Sun);
    private void OnClickMeal() => SetSelectedState(ScheduleState.Meal);
    private void OnClickCounsel() => SetSelectedState(ScheduleState.Counsel);
    private void OnClickCure() => SetSelectedState(ScheduleState.Cure);
    private void OnClickSleep() => SetSelectedState(ScheduleState.Sleep);
    private void OnClickGym() => SetSelectedState(ScheduleState.Gym);
}
