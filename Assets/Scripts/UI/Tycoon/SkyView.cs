using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

public class SkyView : MonoBehaviour
{
    [Serializable]
    public struct SkySetting
    {
        public SkyTime State;
        public Sprite SkySprite;
    }

    [SerializeField] private SpriteRenderer SpriteRenderer_Sky;
    [SerializeField] private SpriteRenderer SpriteRenderer_Fade;

    [SerializeField] private SkySetting[] SkySettings;

    private float _transitionDuration = 2f;
    private SkyViewModel _skyVM;
    private CancellationTokenSource _skyCancel;

    private void Awake()
    {
        _skyVM = new SkyViewModel();
        _skyVM.PropertyChanged += OnViewModelPropertyChanged;

        if (SpriteRenderer_Fade != null)
        {
            Color color = SpriteRenderer_Fade.color;
            color.a = 0f;
            SpriteRenderer_Fade.color = color;
        }
    }

    private void OnEnable()
    {
        GameManager.Inst.Services.DayService.OnChangeHour += OnHourChanged;
        _skyVM.OnHourChanged(GameManager.Inst.Services.DayService.CurrentHour);
    }

    private void OnDisable()
    {
        GameManager.Inst.Services.DayService.OnChangeDay -= OnHourChanged;
    }

    private void OnDestroy()
    {
        _skyVM.PropertyChanged -= OnViewModelPropertyChanged;

        CancelSky();
    }

    private void OnHourChanged(int hour)
    {
        _skyVM.OnHourChanged(hour);
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(SkyViewModel.CurrentState))
        {
            StartTransition(_skyVM.CurrentState);
        }
    }

    private void StartTransition(SkyTime targetState)
    {
        SkySetting targetSetting = default;
        bool isFind = false;

        for (int i = 0; i < SkySettings.Length; i++)
        {
            if (SkySettings[i].State == targetState)
            {
                targetSetting = SkySettings[i];
                isFind = true;
                break;
            }
        }

        if (!isFind)
        {
            return;
        }

        CancelSky();
        _skyCancel = new CancellationTokenSource();

        AnimateSkybox(targetSetting.SkySprite, _skyCancel).Forget();
    }

    private async UniTaskVoid AnimateSkybox(Sprite target, CancellationTokenSource cancel)
    {
        SpriteRenderer_Fade.sprite = target;
        SpriteRenderer_Fade.color = new Color(1f, 1f, 1f, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < _transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / _transitionDuration);
            float alpha = Mathf.SmoothStep(0f, 1f, t);

            SpriteRenderer_Fade.color = new Color(1f, 1f, 1f, alpha);

            await UniTask.NextFrame(cancel.Token);
        }

        SpriteRenderer_Sky.sprite = target;
        SpriteRenderer_Fade.color = new Color(1f, 1f, 1f, 0f);
    }


    private void CancelSky()
    {
        if (_skyCancel != null)
        {
            _skyCancel.Cancel();
            _skyCancel.Dispose();
            _skyCancel = null;
        }
    }
}
