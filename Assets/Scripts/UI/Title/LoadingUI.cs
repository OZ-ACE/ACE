using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class LoadingUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Loading;

    private CancellationTokenSource _loadingToken;
    private float _loadingProgress = 3f;

    private void OnEnable()
    {
        Text_Loading.text = SetLoadingText();

        LoadingRoutine().Forget();
    }

    private string SetLoadingText()
    {
        int randomNum = UnityEngine.Random.Range(1, GameDataManager.Inst.GetDataList<Loading>().Count - 1);

        string loadingText = GameDataManager.Inst.GetData<Loading>($"Loading_{randomNum}").Content;
        return loadingText;
    }

    private async UniTask LoadingRoutine()
    {
        if (_loadingToken != null)
        {
            CancelLoading();
        }

        _loadingToken = new CancellationTokenSource();

        await UniTask.Delay(TimeSpan.FromSeconds(_loadingProgress), cancellationToken: _loadingToken.Token);

        UIManager.Inst.CloseLoadingUI();
    }

    private void CancelLoading()
    {
        _loadingToken.Cancel();
        _loadingToken.Dispose();
        _loadingToken = null;
    }
}