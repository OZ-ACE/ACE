using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";
    public GameServiceContainer Services { get; private set; }
    public InventoryViewModel InventoryViewModel { get; private set; }

    public Action<float> OnChangeBrightness;

    protected override void Awake()
    {
        base.Awake();

        Services = new GameServiceContainer();
        Services.Initialize();

        InventoryViewModel = new InventoryViewModel();
    }

    private void Start()
    {
        ApplySetting();
        SoundManager.Inst.PlayBGM("Tycoon");
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
}