using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : SingletonBase<GameManager>
{
    public string CurrentDialogueID { get; private set; } = "Opening_01";
    public GameServiceContainer Services { get; private set; }

    // 인벤토리 뷰모델 (상점·인벤토리 UI가 공유)
    public InventoryViewModel InventoryViewModel { get; private set; }

    private readonly Queue<string> _episodePlayQueue = new Queue<string>();
    private bool _isPlayingEpisode;

    public Action<float> OnChangeBrightness;

    protected override void Awake()
    {
        base.Awake();

        Services = new GameServiceContainer();
        Services.Initialize();

        InventoryViewModel = new InventoryViewModel();

        BindSaveEvents();
        BindDialogueEvents();
        BindEpisodeEvents();
    }

    private void Start()
    {
        InitializeLoadedServices();
        ApplySetting().Forget();
        ApplySetting();
    }

    // 이미 로드된 플레이어 데이터 기준으로 게임 서비스 초기화
    private void InitializeLoadedServices()
    {
        if (SaveManager.Inst == null || SaveManager.Inst.CurrentPlayerModel == null || Services == null)
        {
            return;
        }

        Services.InitializeAfterLoad();
    }

    private void OnDestroy()
    {
        if (SaveManager.Inst != null)
        {
            SaveManager.Inst.OnCompleteLoad -= HandleCompleteLoad;
        }

        if (Services != null && Services.DialogueService != null)
        {
            Services.DialogueService.OnCompleteDialogue -= HandleCompleteDialogue;
        }

        if (Services != null && Services.EpisodeService != null)
        {
            Services.EpisodeService.OnRequestPlayEpisode -= HandleRequestPlayEpisode;
        }

        _episodePlayQueue.Clear();
        _isPlayingEpisode = false;

        Services?.Release();
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

    private void BindDialogueEvents()
    {
        if (Services == null || Services.DialogueService == null)
        {
            return;
        }

        Services.DialogueService.OnCompleteDialogue += HandleCompleteDialogue;
    }

    private void BindEpisodeEvents()
    {
        if (Services == null || Services.EpisodeService == null)
        {
            return;
        }

        Services.EpisodeService.OnRequestPlayEpisode += HandleRequestPlayEpisode;
    }

    private void HandleCompleteLoad()
    {
        if (Services == null)
        {
            return;
        }

        Services.InitializeAfterLoad();
    }

    private void HandleCompleteDialogue(DialoguePlayContext playContext)
    {
        if (playContext == null)
        {
            return;
        }

        switch (playContext.Source)
        {
            case DialoguePlaySource.Episode:
                HandleCompleteEpisodeDialogue(playContext);
                break;
        }
    }
    private void HandleCompleteEpisodeDialogue(DialoguePlayContext playContext)
    {
        if (playContext.EpisodePlayMode == EpisodePlayMode.Normal)
        {
            Services.EpisodeService.CompleteEpisode(playContext.SourceDataId);
            _isPlayingEpisode = false;

            TryPlayNextEpisode();
        }
    }

    private void HandleRequestPlayEpisode(EpisodeData episodeData)
    {
        if (episodeData == null)
        {
            return;
        }

        EnqueueEpisode(episodeData.ID);
        TryPlayNextEpisode();
    }


    private void EnqueueEpisode(string episodeDataId)
    {
        if (string.IsNullOrEmpty(episodeDataId))
        {
            return;
        }

        if (_episodePlayQueue.Contains(episodeDataId))
        {
            return;
        }

        _episodePlayQueue.Enqueue(episodeDataId);
    }

    private void TryPlayNextEpisode()
    {
        if (_isPlayingEpisode)
        {
            return;
        }

        while (_episodePlayQueue.Count > 0)
        {
            string episodeDataId = _episodePlayQueue.Dequeue();
            string dialogueId = Services.EpisodeService.RequestPlayEpisode(episodeDataId, EpisodePlayMode.Normal);

            if (string.IsNullOrEmpty(dialogueId))
            {
                continue;
            }

            DialoguePlayContext playContext = new DialoguePlayContext(DialoguePlaySource.Episode, episodeDataId, EpisodePlayMode.Normal);

            Services.DialogueService.BeginDialogue(playContext);

            _isPlayingEpisode = true;

            SetDialogueID(dialogueId);

            UIManager.Inst.CloseEpisodeArchive();
            UIManager.Inst.OpenDialogueUI();

            Debug.Log($"GameManager - 자동 에피소드 재생 시작 : {episodeDataId}");

            return;
        }
    }
}