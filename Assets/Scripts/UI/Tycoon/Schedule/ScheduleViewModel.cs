using System.Collections.Generic;

public enum ScheduleState
{
    None,
    Rest,
    Sun,
    Sleep,
    Gym,
    Meal,
    Shower,
    Counsel,
    Cure
}

public enum ScheduleVisualType
{
    Past,
    Current,
    Future
}

public class ScheduleViewModel : ViewModelBase
{
    private HeroModel _hero;
    private int _currentInGameHour = 0;

    private ScheduleState _selectedState = ScheduleState.Sleep;
    public ScheduleState SelectedState
    {
        get => _selectedState;
        set
        {
            if (_selectedState != value)
            {
                _selectedState = value;
                OnPropertyChanged(nameof(SelectedState));
            }
        }
    }

    private List<ScheduleState> _editingStates = new List<ScheduleState>(new ScheduleState[24]);
    public List<ScheduleState> EditingStates
    {
        get => _editingStates;
    }

    public void Init(HeroModel hero, int currentInGameHour)
    {
        _hero = hero;
        _currentInGameHour = currentInGameHour;

        for (int i = 0; i < 24; i++)
        {
            _editingStates[i] = _hero.HourlyStates[i];
            OnPropertyChanged($"Hour_{i}");
            OnPropertyChanged($"VisualType_{i}");
        }
    }

    public void UpdateCurrentTime(int newHour)
    {
        if (_currentInGameHour == newHour)
        {
            return;
        }

        _currentInGameHour = newHour;

        for (int i = 0; i < 24; i++)
        {
            OnPropertyChanged($"VisualType_{i}");
        }
    }

    public ScheduleVisualType GetVisualType(int hour)
    {
        if (hour < _currentInGameHour)
        {
            return ScheduleVisualType.Past;
        }

        if (hour == _currentInGameHour)
        {
            return ScheduleVisualType.Current;
        }

        return ScheduleVisualType.Future;
    }

    public bool IsEditable(int hour)
    {
        return hour >= _currentInGameHour;
    }

    public void SetSchedule(int hour, ScheduleState state)
    {
        if (hour < 0 || hour >= _editingStates.Count)
        {
            return;
        }

        if (!IsEditable(hour))
        {
            return;
        }

        _editingStates[hour] = state;
        OnPropertyChanged($"Hour_{hour}");

        GameManager.Inst.Services.QuestService.GetQuestViewModel().ReportProgress(QuestConditionType.Schedule, state.ToString(), 1);
    }

    public void SaveAndApply()
    {
        for (int i = 0; i < 24; i++)
        {
            _hero.HourlyStates[i] = _editingStates[i];
        }

        _hero.OnUpdateSchedule?.Invoke();
    }
}
