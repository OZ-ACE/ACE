public enum SkyTime
{
    None,
    Morning,
    Afternoon,
    Evening,
    Night
}

public class SkyboxViewModel : ViewModelBase
{
    private SkyTime _currentState;
    public SkyTime CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
            }

            OnPropertyChanged(nameof(CurrentState));
        }
    }

    public void OnHourChanged(int hour)
    {
        SkyTime newState = CalculateTime(hour);
        CurrentState = newState;
    }

    private SkyTime CalculateTime (int hour)
    {
        if (hour >= 21)
        {
            return SkyTime.Night;
        }
        else if (hour >= 17)
        {
            return SkyTime.Evening;
        }
        else if (hour >= 10)
        {
            return SkyTime.Afternoon;
        }
        else if (hour >= 5)
        {
            return SkyTime.Morning;
        }

        return SkyTime.Night;
    }
}
