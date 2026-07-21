using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class HeroHotBar : MonoBehaviour
{
    [SerializeField] private Button Button_Schedule;

    private HeroViewModel _targetVM;

    private void Awake()
    {
        Button_Schedule.onClick.AddListener(OnClickSchedule);
    
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_targetVM != null)
        {
            _targetVM.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    public void OpenHotBar(HeroViewModel heroVM)
    {
        if (_targetVM != null)
        {
            _targetVM.PropertyChanged -= OnPropertyChanged_View;
        }

        _targetVM = heroVM;
        _targetVM.PropertyChanged += OnPropertyChanged_View;

        gameObject.SetActive(true);

        _targetVM.InvokeOnceOnInit();
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_targetVM.Affection):
                // 호감도 반영
                break;

            case nameof(_targetVM.Satisfaction):
                // 불만도 반영
                break;
        }
    }

    private void OnClickSchedule()
    {
        UIBase uiBase = UIManager.Inst.OpenScheduleUI();

        if (uiBase is ScheduleUI scheduleUI)
        {
            scheduleUI.OpenSchedule(_targetVM, GameManager.Inst.Services.DayService.CurrentHour);
        }
    }
}