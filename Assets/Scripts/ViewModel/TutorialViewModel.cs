using System.Collections.Generic;
public class TutorialViewModel : ViewModelBase
{
    private List<Tutorial> _pages = new List<Tutorial>();
    private int _pageIndex;
    private bool _isIgnoreChecked;
    public string CurrentImagePath
    {
        get
        {
            if (_pages == null || _pageIndex >= _pages.Count) { return string.Empty; }
            return _pages[_pageIndex].ImagePath;
        }
    }
    public string CurrentText
    {
        get
        {
            if (_pages == null || _pageIndex >= _pages.Count) { return string.Empty; }
            return _pages[_pageIndex].Text;
        }
    }
    public bool IsLastPage
    {
        get
        {
            if (_pages == null) { return true; }
            return _pageIndex >= _pages.Count - 1;
        }
    }
    public bool IsIgnoreChecked
    {
        get => _isIgnoreChecked;
        set
        {
            if (_isIgnoreChecked != value)
            {
                _isIgnoreChecked = value;
                OnPropertyChanged(nameof(IsIgnoreChecked));
            }
        }
    }
    public void SetTutorial(List<Tutorial> pages)
    {
        _pages = pages;
        _pageIndex = 0;
        _isIgnoreChecked = false;
        OnPropertyChanged(nameof(CurrentImagePath));
        OnPropertyChanged(nameof(CurrentText));
        OnPropertyChanged(nameof(IsIgnoreChecked));
    }
    public bool GoNextPage()
    {
        if (IsLastPage == true)
        {
            return false;
        }
        _pageIndex++;
        OnPropertyChanged(nameof(CurrentImagePath));
        OnPropertyChanged(nameof(CurrentText));
        return true;
    }
    public void ApplyIgnore()
    {
        if (_isIgnoreChecked == false)
        {
            return;
        }
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            return;
        }
        player.IsTutorialDisabled = true;
        SaveManager.Inst.RequestSaveData(player);
    }

    public bool IsFirstPage
    {
        get { return _pageIndex <= 0; }
    }
    public bool GoPrevPage()
    {
        if (_pageIndex <= 0)
        {
            return false;   // 첫 장이면 아무 일 없음
        }
        _pageIndex--;
        OnPropertyChanged(nameof(CurrentImagePath));
        OnPropertyChanged(nameof(CurrentText));
        return true;
    }
}