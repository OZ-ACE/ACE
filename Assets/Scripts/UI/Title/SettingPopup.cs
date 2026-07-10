using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingPopup : UIBase
{
    [Header("버튼")]
    [SerializeField] private Button Button_Close;
    [SerializeField] private Button Button_Confirm;
    [SerializeField] private Button Button_Reset;

    [Header("사운드")]
    [SerializeField] private AudioMixer AudioMixer_Sound;
    [SerializeField] private Slider Slider_BGM;
    [SerializeField] private Slider Slider_SFX;

    [Header("스크린")]
    [SerializeField] private Slider Slider_Bright;
    [SerializeField] private Toggle Toggle_FullScreen;
    [SerializeField] private TMP_Dropdown Dropdown_TextSpeed;

    private SettingViewModel _settingVM;

    private void Awake()
    {
        _settingVM = new SettingViewModel();

        Button_Close.onClick.AddListener(OnClickClose);
        Button_Confirm.onClick.AddListener(OnClickConfirm);
        Button_Reset.onClick.AddListener(OnClickReset);

        Slider_BGM.onValueChanged.AddListener(OnBGMChanged);
        Slider_SFX.onValueChanged.AddListener(OnSFXChanged);
        Toggle_FullScreen.onValueChanged.AddListener(OnFullScreenChanged);
        Dropdown_TextSpeed.onValueChanged.AddListener(OnTextSpeedChanged);
        Slider_Bright.onValueChanged.AddListener(OnBrightSliderChanged);

        InitDropdownOptions();

        SettingModel model = new SettingModel();
        model.LoadSetting();

        _settingVM = new SettingViewModel();
        _settingVM.Init(model);

        BindViewModel();
    }

    private void OnDestroy()
    {
        if (_settingVM != null)
        {
            _settingVM.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    public void BindViewModel()
    {
        _settingVM.PropertyChanged += OnPropertyChanged_View;
        _settingVM.InvokeOnceOnInit();
    }

    private void InitDropdownOptions()
    {
        Dropdown_TextSpeed.ClearOptions();
        Dropdown_TextSpeed.AddOptions(new List<string> { "느림", "보통", "빠름", "즉시" });
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_settingVM.BGMVolume):
                Slider_BGM.value = _settingVM.BGMVolume;
                ApplyBGMVolume(_settingVM.BGMVolume);
                break;

            case nameof(_settingVM.SFXVolume):
                Slider_SFX.value = _settingVM.SFXVolume;
                ApplySFXVolume(_settingVM.SFXVolume);
                break;

            case nameof(_settingVM.IsFullScreen):
                Toggle_FullScreen.isOn = _settingVM.IsFullScreen;
                ApplyFullScreen(_settingVM.IsFullScreen);
                break;

            case nameof(_settingVM.TextSpeedIndex):
                Dropdown_TextSpeed.value = _settingVM.TextSpeedIndex;
                Dropdown_TextSpeed.RefreshShownValue();
                break;

            case nameof(_settingVM.Brightness):
                Slider_Bright.value = _settingVM.Brightness;
                ApplyBrightness(_settingVM.Brightness);
                break;
        }
    }

    private void OnBGMChanged(float value)
    {
        _settingVM.BGMVolume = value;
    }

    private void OnSFXChanged(float value)
    {
        _settingVM.SFXVolume = value;
    }

    private void OnFullScreenChanged(bool isFull)
    {
        _settingVM.IsFullScreen = isFull;
    }

    private void OnTextSpeedChanged(int idx)
    {
        _settingVM.TextSpeedIndex = idx;
    }

    private void OnBrightSliderChanged(float value)
    {
        _settingVM.Brightness = value;
    }

    private void OnClickClose()
    {
        float originalBGM = PlayerPrefs.GetFloat("BGM", 0.5f);
        float originalSFX = PlayerPrefs.GetFloat("SFX", 0.5f);
        bool originalFull = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        int originalSpeed = PlayerPrefs.GetInt("TextSpeedIndex", 0);
        float originalBright = PlayerPrefs.GetFloat("Brightness", 0f);

        ApplyBGMVolume(originalBGM);
        ApplySFXVolume(originalSFX);
        ApplyFullScreen(originalFull);
        ApplyBrightness(originalBright);

        OnBGMChanged(originalBGM);
        OnSFXChanged(originalSFX);
        OnFullScreenChanged(originalFull);
        OnTextSpeedChanged(originalSpeed);
        OnBrightSliderChanged(originalBright);

        if (UIManager.Inst.IsOpened(UIType.TycoonMainUI) is TycoonMainUI tycoon)
        {
            tycoon.OnCloseSetting?.Invoke();
        }

        UIManager.Inst.CloseSettingPopup();
    }

    private void OnClickConfirm()
    {
        _settingVM.SaveSetting();
        UIManager.Inst.CloseSettingPopup();
    }

    private void OnClickReset()
    {
        _settingVM.ResetSetting();
    }

    private void ApplyBGMVolume(float value)
    {
        AudioMixer_Sound.SetFloat("BGM", Mathf.Log10(Mathf.Max(value, 0.001f)) * 20);
    }

    private void ApplySFXVolume(float value)
    {
        AudioMixer_Sound.SetFloat("SFX", Mathf.Log10(Mathf.Max(value, 0.001f)) * 20);
    }

    private void ApplyBrightness(float value)
    {
        GameManager.Inst.OnChangeBrightness?.Invoke(value);
    }

    private void ApplyFullScreen(bool isFull)
    {
        Screen.fullScreen = isFull;
    }
}