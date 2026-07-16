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
    [SerializeField] private Button Button_Battle;
    [SerializeField] private Button Button_Meal;
    [SerializeField] private Button Button_Counsel;
    [SerializeField] private Button Button_Cure;
    [SerializeField] private Button Button_Sleep;
    [SerializeField] private Button Button_Gym;

    [SerializeField] private Button Button_Confirm;
    [SerializeField] private Button Button_Close;

    [Header("버튼 색")]
    [SerializeField] private Color Color_Shower = new Color(0.93f, 0.93f, 0.88f);
    [SerializeField] private Color Color_Battle = new Color(0.98f, 0.50f, 0.45f);
    [SerializeField] private Color Color_Sun = new Color(0.60f, 0.85f, 0.60f);
    [SerializeField] private Color Color_Counsel = new Color(0.96f, 0.87f, 0.70f);
    [SerializeField] private Color Color_Cure = new Color(0.25f, 0.88f, 0.82f);
    [SerializeField] private Color Color_Gym = new Color(1.00f, 0.84f, 0.00f);
    [SerializeField] private Color Color_Rest = new Color(0.3f, 0.91f, 0.1f);
    [SerializeField] private Color Color_Sleep = new Color(0.50f, 1.00f, 0.83f);
    [SerializeField] private Color Color_Meal = new Color(0.87f, 0.63f, 0.87f);

    private ScheduleViewModel _scheduleVM;
    private List<ScheduleSlot> _slots = new List<ScheduleSlot>();
    private Dictionary<ScheduleState, Button> _buttons = new Dictionary<ScheduleState, Button>();

    private void Awake()
    {
        _scheduleVM = new ScheduleViewModel();

        Button_Confirm.onClick.AddListener(OnClickConfirm);
        Button_Close.onClick.AddListener(OnClickClose);

        Button_Shower.onClick.AddListener(OnClickShower);
        Button_Rest.onClick.AddListener(OnClickRest);
        Button_Sun.onClick.AddListener(OnClickSun);
        Button_Battle.onClick.AddListener(OnClickBattle);
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
        _buttons.Add(ScheduleState.Battle, Button_Battle);
    }

    public void OpenSchedule(HeroModel target)
    {
        _scheduleVM.Init(target);
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

            case ScheduleState.Battle:
                return Color_Battle;

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

    private void OnClickConfirm()
    {
        _scheduleVM.SaveAndApply();
        UIManager.Inst.CloseScheduleUI();
    }

    private void OnClickClose()
    {
        UIManager.Inst.CloseScheduleUI();
    }

    private void OnClickShower() => SetSelectedState(ScheduleState.Shower);
    private void OnClickRest() => SetSelectedState(ScheduleState.Rest);
    private void OnClickSun() => SetSelectedState(ScheduleState.Sun);
    private void OnClickBattle() => SetSelectedState(ScheduleState.Battle);
    private void OnClickMeal() => SetSelectedState(ScheduleState.Meal);
    private void OnClickCounsel() => SetSelectedState(ScheduleState.Counsel);
    private void OnClickCure() => SetSelectedState(ScheduleState.Cure);
    private void OnClickSleep() => SetSelectedState(ScheduleState.Sleep);
    private void OnClickGym() => SetSelectedState(ScheduleState.Gym);
}
