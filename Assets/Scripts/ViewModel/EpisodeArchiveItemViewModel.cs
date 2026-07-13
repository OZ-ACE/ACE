public class EpisodeArchiveItemViewModel : ViewModelBase
{
    private bool _isNew;

    public string EpisodeDataId { get; private set; }
    public string EpisodeName { get; private set; }
    public string EpisodeDescription { get; private set; }
    public EpisodeCategory Category { get; private set; }

    public bool IsNew
    {
        get
        {
            return _isNew;
        }

        private set
        {
            if (_isNew == value)
            {
                return;
            }

            _isNew = value;
            OnPropertyChanged(nameof(IsNew));
        }
    }

    public EpisodeArchiveItemViewModel(EpisodeData episodeData, EpisodeProgressModel progressModel)
    {
        if (episodeData == null)
        {
            return;
        }

        EpisodeDataId = episodeData.ID;
        EpisodeName = episodeData.EpisodeName;
        EpisodeDescription = episodeData.EpisodeDescription;
        Category = episodeData.GetCategory();

        IsNew = progressModel != null && progressModel.IsNew;
    }

    public void ChangeViewed()
    {
        IsNew = false;
    }
}