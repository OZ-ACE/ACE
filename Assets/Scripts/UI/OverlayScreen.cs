using UnityEngine;
using UnityEngine.UI;

public class OverlayScreen : UIBase
{
    [SerializeField] private Image Image_Overlay;

    private void Awake()
    {
        GameManager.Inst.OnChangeBrightness += ChangeBrightness;

        ChangeBrightness(PlayerPrefs.GetFloat("Brightness"));
    }

    private void OnDestroy()
    {
        GameManager.Inst.OnChangeBrightness -= ChangeBrightness;
    }

    private void ChangeBrightness(float value)
    {
        float alpha = Mathf.Clamp(value, 0f, 0.7f);
        Image_Overlay.color = new Color(0f, 0f, 0f, alpha);
    }
}
