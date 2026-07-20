using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : UIBase
{
    [SerializeField] private DialogueController DialogueController;
    [SerializeField] private Button Button_Dialogue;
    [SerializeField] private Button Button_Skip;
    [SerializeField] private Toggle Toggle_Auto;
    [SerializeField] private TextMeshProUGUI Text_Content;
    [SerializeField] private TextMeshProUGUI Text_Speaker;
    [SerializeField] private Image Image_NextArrow;
    [SerializeField] private Image Image_Speaker;

    private bool _isTyping = false;
    private float _typingWaitTime = 0.06f;
    private bool _isAuto = false;
    private float _autoWaitTime = 0.5f;

    private CancellationTokenSource _typingToken;
    private CancellationTokenSource _autoWaitToken;

    private DialogueViewModel _dialogueVM;

    private string _currentBGM = string.Empty;
    private string _currentSFX = string.Empty;

    private void Awake()
    {
        Button_Dialogue.onClick.AddListener(OnClickDialogue);
        Toggle_Auto.onValueChanged.AddListener(OnClickAuto);
        Button_Skip.onClick.AddListener(OnClickSkip);
    }

    private void OnEnable()
    {
        _isAuto = false;
        Toggle_Auto.isOn = _isAuto;

        _typingWaitTime = PlayerPrefs.GetFloat("TextSpeed", 0.06f);

        if (_dialogueVM == null)
        {
            _dialogueVM = new DialogueViewModel();
            BindViewModel(_dialogueVM);
        }

        DialogueController.Init(_dialogueVM);
        _dialogueVM.UpdateState(GameManager.Inst.CurrentDialogueID);
    }

    public void BindViewModel(DialogueViewModel dialogueVM)
    {
        _dialogueVM = dialogueVM;
        _dialogueVM.PropertyChanged += OnViewModelPropertyChanged;
        _dialogueVM.InvokeOnceOnInit();
    }

    private void OnDestroy()
    {
        CancelTyping();

        if (_dialogueVM != null)
        {
            _dialogueVM.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DialogueViewModel.Speaker):
                Text_Speaker.text = _dialogueVM.Speaker;
                break;
            case nameof(DialogueViewModel.IsSpeakerActive):
                Image_Speaker.gameObject.SetActive(_dialogueVM.IsSpeakerActive);
                break;
            case nameof(DialogueViewModel.BGM):
                SetBGM(_dialogueVM.BGM);
                break;
            case nameof(DialogueViewModel.SFX):
                SetSFX(_dialogueVM.SFX);
                break;
            case nameof(DialogueViewModel.IsNextArrow):
                Image_NextArrow.gameObject.SetActive(_dialogueVM.IsNextArrow);
                break;
            case nameof(DialogueViewModel.CurrentDialogueID):
                string content = GameDataManager.Inst.GetData<Dialogue>(GameManager.Inst.CurrentDialogueID).Content;
                Typing(content).Forget();
                break;
        }
    }

    private void OnClickDialogue()
    {
        if (_isTyping)
        {
            CancelTyping();
            _isTyping = false;
            Text_Content.maxVisibleCharacters = Text_Content.text.Length;
            _dialogueVM.IsNextArrow = true;
            SoundManager.Inst.StopSFX();

            if (_isAuto)
            {
                WaitAuto().Forget();
            }

            return;
        }

        if (_isAuto)
        {
            CancelAutoWait();
        }

        _dialogueVM.RequestNext();
    }

    private void OnClickAuto(bool isOn)
    {
        if (isOn)
        {
            _isAuto = true;

            if (!_isTyping)
            {
                _dialogueVM.RequestNext();
            }
        }
        else
        {
            _isAuto = false;
        }
    }

    private void OnClickSkip()
    {
        CancelTyping();

        string nextID = GameDataManager.Inst.GetData<Dialogue>(GameManager.Inst.CurrentDialogueID).NextID;

        foreach (var data in GameDataManager.Inst.GetDataList<Dialogue>())
        {
            if (nextID == "0" || string.IsNullOrEmpty(nextID))
            {
                break;
            }

            GameManager.Inst.SetDialogueID(nextID);
            nextID = GameDataManager.Inst.GetData<Dialogue>(GameManager.Inst.CurrentDialogueID).NextID;
        }
;
        _dialogueVM.UpdateState(GameManager.Inst.CurrentDialogueID);
    }

    private async UniTask Typing(string content)
    {
        CancelTyping();
        _typingToken = new CancellationTokenSource();

        _isTyping = true;
        _dialogueVM.IsNextArrow = false;
        Text_Content.text = content;
        Text_Content.maxVisibleCharacters = 0;
        SoundManager.Inst.PlayTypingSound();

        if (_typingWaitTime <= 0f)
        {
            Text_Content.maxVisibleCharacters = content.Length;

            await UniTask.Yield(cancellationToken: _typingToken.Token);
        }
        else
        {
            for (int i = 0; i < content.Length; i++)
            {
                Text_Content.maxVisibleCharacters = i;

                await UniTask.Delay(TimeSpan.FromSeconds(_typingWaitTime), cancellationToken: _typingToken.Token);
            }
        }

        _isTyping = false;
        _dialogueVM.IsNextArrow = true;
        SoundManager.Inst.StopSFX();

        if (_isAuto)
        {
            await WaitAuto();
        }
    }

    private async UniTask WaitAuto()
    {
        CancelAutoWait();
        _autoWaitToken = new CancellationTokenSource();

        await UniTask.Delay(TimeSpan.FromSeconds(_autoWaitTime), cancellationToken: _autoWaitToken.Token);

        _dialogueVM.RequestNext();
    }

    private void CancelTyping()
    {
        if (_typingToken != null)
        {
            _typingToken.Cancel();
            _typingToken.Dispose();
            _typingToken = null;
        }
    }

    private void CancelAutoWait()
    {
        if (_autoWaitToken != null)
        {
            _autoWaitToken.Cancel();
            _autoWaitToken.Dispose();
            _autoWaitToken = null;
        }
    }

    private void SetBGM(string bgm)
    {
        if (string.IsNullOrEmpty(bgm) || _currentBGM == bgm)
        {
            return;
        }

        _currentBGM = bgm;
        SoundManager.Inst.PlayBGM(bgm);
    }

    private void SetSFX(string sfx)
    {
        if (string.IsNullOrEmpty(sfx) || _currentBGM == sfx)
        {
            return;
        }

        _currentSFX = sfx;
        SoundManager.Inst.PlaySFX(sfx);
    }
}