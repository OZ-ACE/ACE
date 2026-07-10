using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdmissionPaperSlot : UIBase
{
    [Header("Paper")]
    [SerializeField] private GameObject Group_Front;
    [SerializeField] private Image Image_PaperBack;

    [Header("Image")]
    [SerializeField] private Image Image_Hero;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private TextMeshProUGUI Text_Name;
    [SerializeField] private TextMeshProUGUI Text_Age;
    [SerializeField] private TextMeshProUGUI Text_MainSkill;
    [SerializeField] private TextMeshProUGUI Text_Remarks;

    [Header("Button")]
    [SerializeField] private Button Button_Admit;
    [SerializeField] private Button Button_Next;
    [SerializeField] private Button Button_Prev;

    private const float LiftY = 18f;
    private const float ScaleUp = 1.04f;
    private const float FlipHalfTime = 0.14f;
    private const float MoveTime = 0.24f;
    private const float SettleTime = 0.08f;

    private RectTransform _rectTransform;
    private HeroData _heroData;

    private Vector2 _stackedPosition;
    private Vector2 _flippedPosition;
    private Vector3 _stackedRotation;
    private Vector3 _flippedRotation;

    private AdmissionPaperState _state;
    private Sequence _sequence;

    private bool _isPlaying;

    public int PaperIndex { get; private set; }

    public event Action<int> OnClickNext;
    public event Action<int> OnClickPrev;
    public event Action<int> OnFlipComplete;
    public event Action<int> OnReturnComplete;
    public event Action<string> OnClickAdmit;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    public void Initialize(HeroData heroData, int paperIndex)
    {
        _heroData = heroData;
        PaperIndex = paperIndex;

        _isPlaying = false;

        RefreshPaperInfo();
    }

    public void SetPaperLayout(Vector2 stackedPosition, Vector2 flippedPosition, Vector3 stackedRotation, Vector3 flippedRotation)
    {
        _stackedPosition = stackedPosition;
        _flippedPosition = flippedPosition;
        _stackedRotation = stackedRotation;
        _flippedRotation = flippedRotation;

        _rectTransform.anchoredPosition = _stackedPosition;
        _rectTransform.localEulerAngles = _stackedRotation;
        _rectTransform.localScale = Vector3.one;
    }

    public void ApplyState(AdmissionPaperState state, bool canReturn)
    {
        _state = state;

        bool isViewing = _state == AdmissionPaperState.Viewing;
        bool isStacked = _state == AdmissionPaperState.Stacked;
        bool isFlipped = _state == AdmissionPaperState.Flipped;

        Group_Front.SetActive(isViewing || isStacked);
        Image_PaperBack.gameObject.SetActive(isFlipped);

        Button_Next.gameObject.SetActive(isViewing);
        Button_Admit.gameObject.SetActive(isViewing);
        Button_Prev.gameObject.SetActive(isFlipped && canReturn);

        Button_Next.interactable = isViewing && _isPlaying == false;
        Button_Admit.interactable = isViewing && _isPlaying == false;
        Button_Prev.interactable = isFlipped && canReturn && _isPlaying == false;
    }

    public void PlayFlipToLeft()
    {
        if (_state != AdmissionPaperState.Viewing)
        {
            return;
        }

        if (CanPlayAnimation() == false)
        {
            return;
        }

        BeginAnimation();

        _sequence = DOTween.Sequence();

        _sequence.Append(_rectTransform.DOScale(ScaleUp, SettleTime));

        _sequence.Join(_rectTransform.DOAnchorPosY(_stackedPosition.y + LiftY, SettleTime));

        _sequence.Append(_rectTransform.DORotate(new Vector3(0f, 90f, _stackedRotation.z), FlipHalfTime));

        _sequence.AppendCallback(ChangeToBackVisual);

        _sequence.Append(_rectTransform.DOAnchorPos(_flippedPosition + new Vector2(0f, LiftY), MoveTime));

        _sequence.Join(_rectTransform.DORotate(new Vector3(0f, 180f, _flippedRotation.z), MoveTime));

        _sequence.AppendCallback(NormalizeFlippedRotation);

        _sequence.Append(_rectTransform.DOAnchorPos(_flippedPosition, SettleTime));

        _sequence.Join(_rectTransform.DOScale(Vector3.one, SettleTime));

        _sequence.OnComplete(CompleteFlipAnimation);
    }

    public void PlayReturnToStack()
    {
        if (_state != AdmissionPaperState.Flipped)
        {
            return;
        }

        if (CanPlayAnimation() == false)
        {
            return;
        }

        BeginAnimation();

        _rectTransform.SetAsLastSibling();

        _sequence = DOTween.Sequence();

        _sequence.Append(_rectTransform.DOScale(ScaleUp, SettleTime));

        _sequence.Join(_rectTransform.DOAnchorPosY(_flippedPosition.y + LiftY, SettleTime));

        _sequence.Append(_rectTransform.DOAnchorPos(_stackedPosition + new Vector2(0f, LiftY), MoveTime));

        _sequence.Join(_rectTransform.DORotate(new Vector3(0f, 90f, _stackedRotation.z), MoveTime));

        _sequence.AppendCallback(ChangeToFrontVisual);

        _sequence.Append(_rectTransform.DORotate(_stackedRotation, FlipHalfTime));

        _sequence.Append(_rectTransform.DOAnchorPos(_stackedPosition, SettleTime));

        _sequence.Join(_rectTransform.DOScale(Vector3.one, SettleTime));

        _sequence.OnComplete(CompleteReturnAnimation);
    }

    public string GetHeroId()
    {
        if (_heroData == null)
        {
            return string.Empty;
        }

        return _heroData.ID;
    }

    private void RefreshPaperInfo()
    {
        if (_heroData == null)
        {
            ClearPaperInfo();
            return;
        }

        Text_Title.text = "입소 신청서";
        Text_Name.text = _heroData.HeroName;
        Text_Age.text = $"나이 : {_heroData.Age}";
        Text_MainSkill.text = $"대표 능력 : {_heroData.MainSkillId}";
        Text_Remarks.text = $"특이사항 : {_heroData.Remarks}";

        RefreshHeroImage();
    }

    private void RefreshHeroImage()
    {
        if (Image_Hero == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_heroData.ProfileImage) == true)
        {
            Image_Hero.sprite = null;
            Image_Hero.gameObject.SetActive(false);
            return;
        }

        Image_Hero.gameObject.SetActive(true);

        ResourceManager.Inst.LoadSprite(_heroData.ProfileImage, SetHeroImage);
    }

    private void SetHeroImage(Sprite sprite)
    {
        if (Image_Hero == null)
        {
            return;
        }

        if (sprite == null)
        {
            Image_Hero.sprite = null;
            Image_Hero.gameObject.SetActive(false);
            return;
        }

        Image_Hero.sprite = sprite;
        Image_Hero.gameObject.SetActive(true);
    }

    private void ClearPaperInfo()
    {
        Text_Title.text = string.Empty;
        Text_Name.text = string.Empty;
        Text_Age.text = string.Empty;
        Text_MainSkill.text = string.Empty;
        Text_Remarks.text = string.Empty;

        if (Image_Hero == null)
        {
            return;
        }

        Image_Hero.sprite = null;
        Image_Hero.gameObject.SetActive(false);
    }

    private void BindButtonEvents()
    {
        Button_Admit.onClick.RemoveListener(OnClickAdmitButton);
        Button_Next.onClick.RemoveListener(OnClickNextButton);
        Button_Prev.onClick.RemoveListener(OnClickPrevButton);

        Button_Admit.onClick.AddListener(OnClickAdmitButton);
        Button_Next.onClick.AddListener(OnClickNextButton);
        Button_Prev.onClick.AddListener(OnClickPrevButton);
    }

    private void UnbindButtonEvents()
    {
        Button_Admit.onClick.RemoveListener(OnClickAdmitButton);
        Button_Next.onClick.RemoveListener(OnClickNextButton);
        Button_Prev.onClick.RemoveListener(OnClickPrevButton);
    }

    private void OnClickAdmitButton()
    {
        if (_isPlaying == true)
        {
            return;
        }

        if (_state != AdmissionPaperState.Viewing)
        {
            return;
        }

        if (_heroData == null)
        {
            return;
        }

        OnClickAdmit?.Invoke(_heroData.ID);
    }

    private void OnClickNextButton()
    {
        if (_isPlaying == true)
        {
            return;
        }

        if (_state != AdmissionPaperState.Viewing)
        {
            return;
        }

        OnClickNext?.Invoke(PaperIndex);
    }

    private void OnClickPrevButton()
    {
        if (_isPlaying == true)
        {
            return;
        }

        if (_state != AdmissionPaperState.Flipped)
        {
            return;
        }

        OnClickPrev?.Invoke(PaperIndex);
    }

    private bool CanPlayAnimation()
    {
        return _isPlaying == false;
    }

    private void BeginAnimation()
    {
        _isPlaying = true;

        KillSequence();

        Button_Next.interactable = false;
        Button_Admit.interactable = false;
        Button_Prev.interactable = false;
    }

    private void ChangeToBackVisual()
    {
        Group_Front.SetActive(false);
        Image_PaperBack.gameObject.SetActive(true);

        _rectTransform.SetAsLastSibling();
    }

    private void NormalizeFlippedRotation()
    {
        _rectTransform.localEulerAngles = _flippedRotation;
    }

    private void CompleteFlipAnimation()
    {
        _isPlaying = false;
        _state = AdmissionPaperState.Flipped;

        OnFlipComplete?.Invoke(PaperIndex);
    }

    private void ChangeToFrontVisual()
    {
        Group_Front.SetActive(true);
        Image_PaperBack.gameObject.SetActive(false);
    }

    private void CompleteReturnAnimation()
    {
        _isPlaying = false;
        _state = AdmissionPaperState.Viewing;

        OnReturnComplete?.Invoke(PaperIndex);
    }

    private void KillSequence()
    {
        if (_sequence == null)
        {
            return;
        }

        _sequence.Kill();
        _sequence = null;
    }

    private void OnDestroy()
    {
        KillSequence();
        UnbindButtonEvents();

        OnClickNext = null;
        OnClickPrev = null;
        OnFlipComplete = null;
        OnReturnComplete = null;
        OnClickAdmit = null;
    }
}