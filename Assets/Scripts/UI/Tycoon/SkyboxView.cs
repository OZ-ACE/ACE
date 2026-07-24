using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

public class SkyboxView : MonoBehaviour
{
    [Serializable]
    public struct SkyboxSetting
    {
        public SkyTime State;
        public Material Material;
        public Color LightColor;
        public float LightIntensity;
    }

    [SerializeField] private SkyboxSetting[] Settings;
    [SerializeField] private Light Light_Main;

    private float _transitionDuration = 2f;
    private SkyboxViewModel _skyboxVM;
    private CancellationTokenSource _skyboxCancel;

    private void Awake()
    {
        _skyboxVM = new SkyboxViewModel();
        _skyboxVM.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnEnable()
    {
        GameManager.Inst.Services.DayService.OnChangeHour += OnHourChanged;
        _skyboxVM.OnHourChanged(GameManager.Inst.Services.DayService.CurrentHour);
    }

    private void OnDestroy()
    {
        _skyboxVM.PropertyChanged -= OnViewModelPropertyChanged;

        CancelSkybox();
    }

    private void OnHourChanged(int hour)
    {
        _skyboxVM.OnHourChanged(hour);
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(SkyboxViewModel.CurrentState))
        {
            StartTransition(_skyboxVM.CurrentState);
        }
    }

    private void StartTransition(SkyTime targetState)
    {
        SkyboxSetting targetSetting = default;
        bool isFind = false;

        for (int i = 0; i < Settings.Length; i++)
        {
            if (Settings[i].State == targetState)
            {
                targetSetting = Settings[i];
                isFind = true;
                break;
            }
        }

        if (!isFind)
        {
            return;
        }

        CancelSkybox();
        _skyboxCancel = new CancellationTokenSource();

        AnimateSkybox(targetSetting, _skyboxCancel).Forget();
    }

    private async UniTaskVoid AnimateSkybox(SkyboxSetting target, CancellationTokenSource cancel)
    {
        Color startLight = Light_Main.color;
        float startIntensity = Light_Main.intensity;

        if (target.Material != null)
        {
            RenderSettings.skybox = target.Material;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _transitionDuration)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / _transitionDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (Light_Main != null)
            {
                Light_Main.color = Color.Lerp(startLight, target.LightColor, smooth);
                Light_Main.intensity = Mathf.Lerp(startIntensity, target.LightIntensity, smooth);
            }

            await UniTask.NextFrame(cancel.Token);
        }

        if (Light_Main != null)
        {
            Light_Main.color = target.LightColor;
            Light_Main.intensity = target.LightIntensity;
        }

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancel.Token);
        DynamicGI.UpdateEnvironment();
    }


    private void CancelSkybox()
    {
        if (_skyboxCancel != null)
        {
            _skyboxCancel.Cancel();
            _skyboxCancel.Dispose();
            _skyboxCancel = null;
        }
    }
}
