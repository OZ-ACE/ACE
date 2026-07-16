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

    private ScheduleViewModel _scheduleVM;
    private List<ScheduleSlot> _slots = new List<ScheduleSlot>();

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

        CreateTimeSlot();
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

            scheduleSlot.InitSlot(i, _scheduleVM);
            _slots.Add(scheduleSlot);
        }
    }

    private void SetSelectedState(ScheduleState state)
    {
        _scheduleVM.SelectedState = state;
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
