using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private Button Button_Slot;
    [SerializeField] private Image Image_Portrait;
    [SerializeField] private TextMeshProUGUI Text_HeroName;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_DiseaseName;
    [SerializeField] private TextMeshProUGUI Text_SkillName;
    [SerializeField] private TextMeshProUGUI Text_Age;
    [SerializeField] private TextMeshProUGUI Text_PrimeLevel;

    [SerializeField] private TextMeshProUGUI Text_Affection;
    [SerializeField] private TextMeshProUGUI Text_Satisfaction;
    [SerializeField] private Image Image_Affection;
    [SerializeField] private Image Image_Satisfaction;

    [SerializeField] private Sprite Sprite_Select;
    [SerializeField] private Sprite Sprite_Unselect;

    private const float MAX_STAT_VALUE = 100f;

    public Action<HeroViewModel> OnSlotClick;

    private HeroViewModel _heroVM;
    private string _heroID;

    private void Awake()
    {
        Button_Slot.onClick.AddListener(OnClickSlot);
    }

    private void OnEnable()
    {
        GameManager.Inst.Services.DayService.OnChangeDay += OnDayChanged;
    }

    private void OnDisable()
    {
        GameManager.Inst.Services.DayService.OnChangeDay -= OnDayChanged;
    }
    
    private void OnDayChanged(int newDay)
    {
        _heroVM.Model?.LoadHeroData(_heroID);
        _heroVM.InvokeOnceOnInit();
    }

    public void InitSlot(HeroViewModel viewModel)
    {
        if (_heroVM != null)
        {
            _heroVM.PropertyChanged -= OnPropertyChanged_View;
            _heroVM = null;
        }

        _heroVM = viewModel;
        _heroID = _heroVM.HeroID;

        _heroVM.PropertyChanged += OnPropertyChanged_View;
        _heroVM.InvokeOnceOnInit();
    }

    private void OnClickSlot()
    {
        OnSlotClick?.Invoke(_heroVM);
    }

    private void OnDestroy()
    {
        if (_heroVM != null)
        {
            _heroVM.PropertyChanged -= OnPropertyChanged_View;
            _heroVM = null;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_heroVM.HeroName):
                Text_HeroName.text = _heroVM.HeroName;
                SetPortrait(_heroID).Forget();
                break;

            case nameof(_heroVM.Description):
                Text_Description.text = _heroVM.Description;
                break;

            case nameof(_heroVM.DiseaseName):
                Text_DiseaseName.text = string.Join(", ", _heroVM.DiseaseName);
                break;

            case nameof(_heroVM.Age):
                Text_Age.text = $"연령 : {_heroVM.Age}";
                break;

            case nameof(_heroVM.Skill):
                Text_SkillName.text = $"능력 : {_heroVM.Skill}";
                break;

            case nameof(_heroVM.Affection):
                Text_Affection.text = $"{_heroVM.Affection}";
                Image_Affection.fillAmount = Mathf.Clamp01(_heroVM.Affection / MAX_STAT_VALUE);
                break;

            case nameof(_heroVM.Satisfaction):
                Text_Satisfaction.text = $"{_heroVM.Satisfaction}";
                Image_Satisfaction.fillAmount = Mathf.Clamp01(_heroVM.Satisfaction / MAX_STAT_VALUE);
                break;

            case nameof(_heroVM.IsSelect):
                SetSlotBackground(_heroVM.IsSelect);
                break;

            case nameof(_heroVM.PrimeLevel):
                Text_PrimeLevel.text = $"프라임 레벨 : {_heroVM.PrimeLevel}";
                break;
        }
    }

    private async UniTask SetPortrait(string heroID)
    {
        Image_Portrait.sprite = await ResourceManager.Inst.LoadSprite($"Image/Portrait[{heroID}]");
    }

    private void SetSlotBackground(bool isSelect)
    {
        Image_Background.sprite = isSelect ? Sprite_Select : Sprite_Unselect;
    }
}