using UnityEngine;

public class AdmissionModel
{
    public int AdmissionId { get; private set; }
    public string HeroId { get; private set; }
    public int RequiredRoomCondition { get; private set; }
    public bool IsExpired { get; private set; }

    public AdmissionModel(int admissionId, string heroId, int requiredRoomCondition, bool isExpired)
    {
        AdmissionId = admissionId;
        HeroId = heroId;
        RequiredRoomCondition = requiredRoomCondition;
        IsExpired = false;
    }

    public void Expire()
    {
        IsExpired = true;
    }
}
