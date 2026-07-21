using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScheduleSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private Image Image_Lock;
    [SerializeField] private Image Image_Highlight;
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
        else if (e.PropertyName == $"VisualType_{_hour}")
        {
            RefreshVisualType();
        }
    }

    private void RefreshColor()
    {
        ScheduleState state = _scheduleVM.EditingStates[_hour];

        Image_Background.color = _scheduleUI.GetSlotColor(state);
    }

    private void RefreshVisualType()
    {
        ScheduleVisualType visualType = _scheduleVM.GetVisualType(_hour);

        switch (visualType)
        {
            case ScheduleVisualType.Past:
                Image_Background.color = Color.darkGray;
                Image_Lock.gameObject.SetActive(true);
                Text_Hour.gameObject.SetActive(false);
                Image_Highlight.gameObject.SetActive(false);
                break;

            case ScheduleVisualType.Current:
                Image_Highlight.gameObject.SetActive(true);
                break;

            case ScheduleVisualType.Future:
                Image_Highlight.gameObject.SetActive(false);
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_scheduleVM.IsEditable(_hour))
        {
            return;
        }

        ScheduleState selectedTool = _scheduleUI.CurrentSelectedToolState;
        ScheduleState currentSlotState = _scheduleVM.EditingStates[_hour];

        if (currentSlotState == selectedTool)
        {
            _scheduleVM.SetSchedule(_hour, ScheduleState.None);
        }
        else
        {
            _scheduleVM.SetSchedule(_hour, selectedTool);
        }
    }
}
