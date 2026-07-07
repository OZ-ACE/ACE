using System.Collections.Generic;

public class AdmissionPopupViewModel : ViewModelBase
{
    private readonly List<AdmissionData> _admissionDatas = new List<AdmissionData>();

    public IReadOnlyList<AdmissionData> AdmissionDatas => _admissionDatas;
    public AdmissionData SelectedAdmissionData { get; private set; }

    public void Initialize()
    {
        _admissionDatas.Clear();

        // TEST
        AddAdmissionData("admission_01");
        AddAdmissionData("admission_02");
        AddAdmissionData("admission_03");
    }

    public void SelectAdmissionData(string id)
    {
        SelectedAdmissionData = null;

        foreach (AdmissionData admissionData in _admissionDatas)
        {
            if (admissionData.ID == id)
            {
                SelectedAdmissionData = admissionData;
                OnPropertyChanged(nameof(SelectedAdmissionData));
                return;
            }
        }
    }

    private void AddAdmissionData(string id)
    {
        AdmissionData admissionData = GameDataManager.Inst.GetData<AdmissionData>(id);

        if (admissionData == null)
        {
            return;
        }

        _admissionDatas.Add(admissionData);
    }
}