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
    Battle,
    Counsel,
    Cure
}

public class ScheduleViewModel : ViewModelBase
{
    private HeroModel _hero;

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

    public void Init(HeroModel hero)
    {
        _hero = hero;

        for (int i = 0; i < 24; i++)
        {
            _editingStates[i] = _hero.HourlyStates[i];
            OnPropertyChanged($"Hour_{i}");
        }
    }

    public void SetSchedule(int hour)
    {
        if (hour < 0 || hour >= _editingStates.Count)
        {
            return;
        }

        if (_editingStates[hour] != _selectedState)
        {
            _editingStates[hour] = _selectedState;
            OnPropertyChanged($"Hour_{hour}");
        }
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
