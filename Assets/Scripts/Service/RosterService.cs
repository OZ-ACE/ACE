
public class RosterService
{
    private RosterViewModel _rosterViewModel;
    public RosterViewModel CreateRosterViewModel()
    {
        if (_rosterViewModel != null)
        {
            return _rosterViewModel;
        }
        _rosterViewModel = new RosterViewModel();
        return _rosterViewModel;
    }
    public RosterViewModel GetRosterViewModel()
    {
        return _rosterViewModel;
    }
}