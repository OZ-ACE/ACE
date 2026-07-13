using System.Collections.Generic;

public class EpisodeArchiveViewModel : ViewModelBase
{
    private readonly EpisodeService _episodeService;

    private readonly List<EpisodeArchiveItemViewModel> _episodeItems = new List<EpisodeArchiveItemViewModel>();

    public IReadOnlyList<EpisodeArchiveItemViewModel> EpisodeItems => _episodeItems;

    public EpisodeArchiveItemViewModel SelectedEpisode { get; private set; }

    public EpisodeArchiveViewModel(EpisodeService episodeService)
    {
        _episodeService = episodeService;
    }

    public void RefreshEpisodes()
    {
        _episodeItems.Clear();

        if (_episodeService == null)
        {
            return;
        }

        List<EpisodeData> episodeDatas = _episodeService.GetUnlockedArchiveEpisodes();

        for (int i = 0; i < episodeDatas.Count; i++)
        {
            EpisodeData episodeData = episodeDatas[i];
            EpisodeProgressModel progressModel = _episodeService.GetEpisodeProgress(episodeData.ID);
            EpisodeArchiveItemViewModel itemViewModel = new EpisodeArchiveItemViewModel(episodeData, progressModel);

            _episodeItems.Add(itemViewModel);
        }

        SelectedEpisode = null;

        OnPropertyChanged(nameof(EpisodeItems));
        OnPropertyChanged(nameof(SelectedEpisode));
    }

    public void SelectEpisode(string episodeDataId)
    {
        SelectedEpisode = null;

        for (int i = 0; i < _episodeItems.Count; i++)
        {
            EpisodeArchiveItemViewModel itemViewModel = _episodeItems[i];

            if (itemViewModel.EpisodeDataId != episodeDataId)
            {
                continue;
            }

            SelectedEpisode = itemViewModel;

            _episodeService.MarkEpisodeAsViewed(episodeDataId);
            itemViewModel.ChangeViewed();

            break;
        }

        OnPropertyChanged(nameof(SelectedEpisode));
    }
}