using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";
    public GameServiceContainer Services { get; private set; }

    // 인벤토리 뷰모델 (상점·인벤토리 UI가 공유)
    public InventoryViewModel InventoryViewModel { get; private set; }

    public Action<float> OnChangeBrightness;

    protected override void Awake()
    {
        base.Awake();

        Services = new GameServiceContainer();
        Services.Initialize();

        InventoryViewModel = new InventoryViewModel();

        BindSaveEvents();
    }

    private void Start()
    {
        ApplySetting();
    }

    public void SetDialogueID(string dialogueID)
    {
        CurrentDialogueID = dialogueID;
    }

    private void ApplySetting()
    {
        AudioMixer audioMixer = SoundManager.Inst.GetAudioMixer();

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

    private void BindSaveEvents()
    {
        if (SaveManager.Inst == null)
        {
            Debug.LogWarning("GameManager - SaveManager 를 찾을 수 없음.");
            return;
        }

        SaveManager.Inst.OnCompleteLoad += HandleCompleteLoad;
    }

    private void HandleCompleteLoad()
    {
        if (Services == null)
        {
            return;
        }

        Services.InitializeAfterLoad();

        Debug.Log("GameManager - 세이브 로드 후 서비스 초기화 완료");
    }
}