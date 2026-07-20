public class AdmissionCandidateModel
{
    public int CandidateId { get; private set; }
    public string HeroId { get; private set; }
    public bool IsAdmitted { get; private set; }
    public bool IsExpired { get; private set; }

    public AdmissionCandidateModel(int candidateId, string heroId) : this(candidateId, heroId, false)
    {
    }

    public AdmissionCandidateModel(int candidateId, string heroId, bool isAdmitted)
    {
        CandidateId = candidateId;
        HeroId = heroId;
        IsAdmitted = isAdmitted;
        IsExpired = false;
    }

    public void Admit()
    {
        IsAdmitted = true;
    }

    public void Expire()
    {
        IsExpired = true;
    }
}
