using System.Collections.Generic;
using UnityEngine;

public class SettingModel
{
    public const float DefaultVolume = 0.5f;
    public const int DefaultFullScreen = 1;
    public const int DefaultTextSpeed = 1;
    public const float DefaultBrightness = 0f;

    private readonly List<float> _textSpeed = new List<float> { 0.09f, 0.06f, 0.03f, 0f };

    public float BGMVolume = 0.5f;
    public float SFXVolume = 0.5f;
    public bool IsFullScreen = true;
    public int TextSpeedIndex = 1;
    public float Brightness = 0f;

    public void LoadSetting()
    {
        BGMVolume = PlayerPrefs.GetFloat("BGM", DefaultVolume);
        SFXVolume = PlayerPrefs.GetFloat("SFX", DefaultVolume);
        IsFullScreen = PlayerPrefs.GetInt("FullScreen", DefaultFullScreen) == 1;
        Brightness = PlayerPrefs.GetFloat("Brightness", DefaultBrightness);
        TextSpeedIndex = PlayerPrefs.GetInt("TextSpeedIndex", DefaultTextSpeed);
    }

    public void SaveAndApplySetting()
    {
        PlayerPrefs.SetFloat("BGM", BGMVolume);
        PlayerPrefs.SetFloat("SFX", SFXVolume);
        PlayerPrefs.SetInt("FullScreen", IsFullScreen ? 1 : 0);
        PlayerPrefs.SetInt("TextSpeedIndex", TextSpeedIndex);
        PlayerPrefs.SetFloat("TextSpeed", _textSpeed[TextSpeedIndex]);
        PlayerPrefs.SetFloat("Brightness", Brightness);
        PlayerPrefs.Save();
    }
}
