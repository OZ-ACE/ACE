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

    private ScheduleUI _scheduleUI;

    public void InitSlot(int hour, ScheduleViewModel scheduleVM, ScheduleUI scheduleUI)
    {
        _hour = hour;
        _scheduleVM = scheduleVM;
        _scheduleUI = scheduleUI;
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

        Image_Background.color = _scheduleUI.GetSlotColor(state);
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
