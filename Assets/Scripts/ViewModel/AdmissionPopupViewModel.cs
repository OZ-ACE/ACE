using System.Collections.Generic;

public class AdmissionPopupViewModel : ViewModelBase
{
    private readonly List<AdmissionCandidateModel> _candidateModels = new List<AdmissionCandidateModel>();

    public IReadOnlyList<AdmissionCandidateModel> CandidateModels => _candidateModels;
    public AdmissionCandidateModel SelectedCandidateModel { get; private set; }

    public void Initialize()
    {
        _candidateModels.Clear();

        List<AdmissionCandidateModel> candidateModels = AdmissionManager.Inst.GetCandidateModels();

        for (int i = 0; i < candidateModels.Count; i++)
        {
            _candidateModels.Add(candidateModels[i]);
        }
    }

    public HeroData GetHeroData(int index)
    {
        if (index < 0 || index >= _candidateModels.Count)
        {
            return null;
        }

        return GameDataManager.Inst.GetData<HeroData>(_candidateModels[index].HeroId);
    }

    public void SelectCandidateModel(int candidateId)
    {
        SelectedCandidateModel = null;

        for (int i = 0; i < _candidateModels.Count; i++)
        {
            if (_candidateModels[i].CandidateId == candidateId)
            {
                SelectedCandidateModel = _candidateModels[i];
                OnPropertyChanged(nameof(SelectedCandidateModel));
                return;
            }
        }
    }
}