using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class InfoText : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Message;

    private float _showDuration = 2f;
    private float _fadeDuration = 0.5f;

    private CancellationTokenSource _cancel;

    public async UniTask ShowMessage(string message)
    {
        CancelTask();
        _cancel = new CancellationTokenSource();

        Text_Message.text = message;
        SetTextAlpha(1f);

        await UniTask.Delay(TimeSpan.FromSeconds(_showDuration), cancellationToken: _cancel.Token);

        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            SetTextAlpha(alpha);
            
            await UniTask.Yield();
        }

        SetTextAlpha(0f);
        UIManager.Inst.CloseInfoText();
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = Text_Message.color;
        color.a = alpha;
        Text_Message.color = color;
    }

    private void CancelTask()
    {
        if (_cancel != null)
        {
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = null;
        }
    }
}
