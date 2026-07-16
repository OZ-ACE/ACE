using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScheduleSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private TextMeshProUGUI Text_Hour;

    private int _hour;
    private ScheduleViewModel _scheduleVM;

    public void InitSlot(int hour, ScheduleViewModel scheduleVM)
    {
        _hour = hour;
        _scheduleVM = scheduleVM;
        Text_Hour.text = $"{hour:D2}:00";

        _scheduleVM.PropertyChanged += OnPropertyChanged_View;

        RefreshColor();
    }

    private void OnDestroy()
    {
        _scheduleVM.PropertyChanged -= OnPropertyChanged_View;
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == $"Hour_{_hour}")
        {
            RefreshColor();
        }
    }

    private void RefreshColor()
    {
        ScheduleState state = _scheduleVM.EditingStates[_hour];

        switch (state)
        {
            case ScheduleState.Shower:
                Image_Background.color = Color.navyBlue;
                break;

            case ScheduleState.Battle:
                Image_Background.color = Color.salmon;
                break;

            case ScheduleState.Sun:
                Image_Background.color = Color.softGreen;
                break;

            case ScheduleState.Counsel:
                Image_Background.color = Color.wheat;
                break;

            case ScheduleState.Cure:
                Image_Background.color = Color.turquoise;
                break;

            case ScheduleState.Gym:
                Image_Background.color = Color.gold;
                break;

            case ScheduleState.Rest:
                Image_Background.color = Color.steelBlue;
                break;

            case ScheduleState.Sleep:
                Image_Background.color = Color.purple;
                break;

            case ScheduleState.Meal:
                Image_Background.color = Color.plum;
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _scheduleVM.SetSchedule(_hour);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging || Input.GetMouseButton(0))
        {
            _scheduleVM.SetSchedule(_hour);
        }
    }
}
