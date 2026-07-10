using UnityEngine;
using UnityEngine.Audio;

public class SettingViewModel : ViewModelBase
{
    private SettingModel _model;

    private float _bgmVolume;
    public float BGMVolume
    {
        get => _bgmVolume;
        set
        {
            if (_bgmVolume != value)
            {
                _bgmVolume = value;
                OnPropertyChanged(nameof(BGMVolume));
            }
        }
    }
    
    private float _sfxVolume;
    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            if (_sfxVolume != value)
            {
                _sfxVolume = value;
                OnPropertyChanged(nameof(SFXVolume));
            }
        }
    }

    private bool _isFullScreen;
    public bool IsFullScreen
    {
        get => _isFullScreen;
        set
        {
            if (_isFullScreen != value)
            {
                _isFullScreen = value;
                OnPropertyChanged(nameof(IsFullScreen));
            }
        }
    }

    private int _textSpeedIndex;
    public int TextSpeedIndex
    {
        get => _textSpeedIndex;
        set
        {
            if (_textSpeedIndex != value)
            {
                _textSpeedIndex = value;
                OnPropertyChanged(nameof(TextSpeedIndex));
            }
        }
    }

    private float _brightness;
    public float Brightness
    {
        get => _brightness;
        set
        {
            if (_brightness != value)
            {
                _brightness = value;
                OnPropertyChanged(nameof(Brightness));
            }
        }
    }

    public void Init(SettingModel model)
    {
        _model = model;

        _bgmVolume = _model.BGMVolume;
        _sfxVolume = _model.SFXVolume;
        _isFullScreen = _model.IsFullScreen;
        _textSpeedIndex = _model.TextSpeedIndex;
        _brightness = _model.Brightness;
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(BGMVolume));
        OnPropertyChanged(nameof(SFXVolume));
        OnPropertyChanged(nameof(IsFullScreen));
        OnPropertyChanged(nameof(TextSpeedIndex));
        OnPropertyChanged(nameof(Brightness));
    }

    public void ResetSetting()
    {
        BGMVolume = SettingModel.DefaultVolume;
        SFXVolume = SettingModel.DefaultVolume;
        IsFullScreen = (SettingModel.DefaultFullScreen == 1);
        TextSpeedIndex = SettingModel.DefaultTextSpeed;
        Brightness = SettingModel.DefaultBrightness;
    }

    public void SaveSetting()
    {
        _model.BGMVolume = BGMVolume;
        _model.SFXVolume = SFXVolume;
        _model.IsFullScreen = IsFullScreen;
        _model.TextSpeedIndex = TextSpeedIndex;
        _model.Brightness = Brightness;

        _model.SaveAndApplySetting();
    }
}
