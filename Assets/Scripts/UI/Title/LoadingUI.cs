using TMPro;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_Loading;

    private void OnEnable()
    {
        Text_Loading.text = SetLoadingText();
    }

    private string SetLoadingText()
    {
        string loadingText = "";

        return loadingText;
    }
}
