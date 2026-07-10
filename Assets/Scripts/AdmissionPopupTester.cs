using UnityEngine;

public class AdmissionPopupTester : MonoBehaviour
{
    private void Start()
    {
        AdmissionManager.Inst.Initialize();
        UIManager.Inst.OpenAdmissionPopup();
    }
}