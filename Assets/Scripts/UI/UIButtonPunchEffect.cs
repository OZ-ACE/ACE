using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// 버튼 클릭 시 순간적으로 커졌다가 줄어드는 펀치 스케일 이펙트 (공용 컴포넌트, 아무 버튼에나 부착)
[RequireComponent(typeof(Button))]
public class UIButtonPunchEffect : MonoBehaviour
{
    private const float PunchScaleMultiplier = 1.15f;
    private const float ScaleUpDurationSeconds = 0.08f;
    private const float ScaleDownDurationSeconds = 0.12f;

    private Button _button;
    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private CancellationTokenSource _cancellationTokenSource;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClickButton);
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClickButton);
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        if (_rectTransform != null)
        {
            _rectTransform.localScale = _originalScale;
        }
    }

    private void OnClickButton()
    {
        PlayPunchEffect().Forget();
    }

    private async UniTaskVoid PlayPunchEffect()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = _cancellationTokenSource.Token;

        _rectTransform.localScale = _originalScale;

        bool isCanceled = await TweenScale(_originalScale, _originalScale * PunchScaleMultiplier, ScaleUpDurationSeconds, token);

        if (isCanceled)
        {
            return;
        }

        isCanceled = await TweenScale(_originalScale * PunchScaleMultiplier, _originalScale, ScaleDownDurationSeconds, token);

        if (isCanceled)
        {
            return;
        }

        _rectTransform.localScale = _originalScale;
    }

    ///<summary> fromScale에서 toScale까지 durationSeconds 동안 선형 보간한다. 도중에 취소되면 true 반환 </summary>
    private async UniTask<bool> TweenScale(Vector3 fromScale, Vector3 toScale, float durationSeconds, CancellationToken token)
    {
        float elapsedSeconds = 0f;

        while (elapsedSeconds < durationSeconds)
        {
            if (token.IsCancellationRequested)
            {
                return true;
            }

            elapsedSeconds += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedSeconds / durationSeconds);

            _rectTransform.localScale = Vector3.Lerp(fromScale, toScale, progress);

            await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
        }

        return token.IsCancellationRequested;
    }
}