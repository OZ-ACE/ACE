using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";

    /// <summary> 전역 재화 서비스 </summary>
    public ICurrencyService CurrencyService { get; private set; }

    /// <summary> 상점 뷰모델 보관 서비스 </summary>
    public ShopService ShopService { get; private set; }

    /// <summary> 인벤토리 뷰모델 (상점·인벤토리 UI가 공유) </summary>
    public InventoryViewModel InventoryViewModel { get; private set; }

    /// <summary> 건설 뷰모델 보관 서비스 </summary>
    public BuildService BuildService { get; private set; }


    public Action<float> OnChangeBrightness;

    protected override void Awake()
    {
        base.Awake();

        CurrencyService = new CurrencyService();
        ShopService = new ShopService(CurrencyService);
        InventoryViewModel = new InventoryViewModel();
        BuildService = new BuildService(CurrencyService); 
    }

    public void SetDialogueID(string dialogueID)
    {
        CurrentDialogueID = dialogueID;

        ApplySetting().Forget();
    }

    private async UniTask ApplySetting()
    {
        AudioMixer audioMixer = await ResourceManager.Inst.LoadAsset<AudioMixer>("Audio/AudioMixer");

        float bgm = PlayerPrefs.GetFloat("BGM", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFX", 0.5f);

        if (audioMixer != null)
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(Mathf.Max(bgm, 0.0001f)) * 20);
            audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(sfx, 0.0001f)) * 20);
        }

        bool isFull = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        Screen.fullScreen = isFull;
    }
}