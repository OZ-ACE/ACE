using System;
using UnityEngine;

public class DialogueViewModel : ViewModelBase
{
    private readonly DialogueService _dialogueService = new DialogueService();

    private string _speaker;
    private string _content;
    private string _background;
    private string _BGM;
    private string _SFX;

    private bool _isSpeakerActive = false;
    private bool _isNextArrow = false;
    private string _currentDialogueID;

    public string Speaker
    {
        get => _speaker;
        set
        {
            if (_speaker != value)
            {
                _speaker = value;
                OnPropertyChanged(nameof(Speaker));
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    public string Background
    {
        get => _background;
        set
        {
            if (_background != value)
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }
    }

    public string BGM
    {
        get => _BGM;
        set
        {
            if (_BGM != value)
            {
                _BGM = value;
                OnPropertyChanged(nameof(BGM));
            }
        }
    }

    public string SFX
    {
        get => _SFX;
        set
        {
            if (_SFX != value)
            {
                _SFX = value;
                OnPropertyChanged(nameof(SFX));
            }
        }
    }

    public bool IsSpeakerActive
    {
        get => _isSpeakerActive;
        set
        {
            if (_isSpeakerActive != value)
            {
                _isSpeakerActive = value;
                OnPropertyChanged(nameof(IsSpeakerActive));
            }
        }
    }

    public bool IsNextArrow
    {
        get => _isNextArrow;
        set
        {
            if (_isNextArrow != value)
            {
                _isNextArrow = value;
                OnPropertyChanged(nameof(IsNextArrow));
            }
        }
    }

    public string CurrentDialogueID
    {
        get => _currentDialogueID;
        set
        {
            if (_currentDialogueID != value)
            {
                _currentDialogueID = value;
                OnPropertyChanged(nameof(CurrentDialogueID));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Speaker));
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(Background));
        OnPropertyChanged(nameof(BGM));
        OnPropertyChanged(nameof(SFX));
        OnPropertyChanged(nameof(IsSpeakerActive));
        OnPropertyChanged(nameof(IsNextArrow));
    }

    public void UpdateState(string dialogueID)
    {
        Dialogue data = _dialogueService.GetDialogueData(dialogueID);

        if (data == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(data.Speaker))
        {
            IsSpeakerActive = false;
        }
        else
        {
            IsSpeakerActive = true;
            Speaker = data.Speaker;
        }

        Background = data.Background;
        Content = data.Content;
        BGM = data.BGM;
        SFX = data.SFX;
        IsNextArrow = false;
        CurrentDialogueID = dialogueID;
    }

    public void RequestNext()
    {
        string nextID = _dialogueService.GetNextDialogueID(GameManager.Inst.CurrentDialogueID);

        if (nextID == "0")
        {
            UIManager.Inst.CloseDialogueUI();
            return;
        }

        GameManager.Inst.SetDialogueID(nextID);
        UpdateState(nextID);
    }
}