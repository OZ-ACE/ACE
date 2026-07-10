using Cysharp.Threading.Tasks;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroUI : UIBase
{
    [SerializeField] private Image Image_Portrait;
    [SerializeField] private TextMeshProUGUI Text_HeroName;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_DiseaseName;
    [SerializeField] private TextMeshProUGUI Text_SkillName;
    [SerializeField] private TextMeshProUGUI Text_Age;

    [SerializeField] private TextMeshProUGUI Text_Affection;
    [SerializeField] private TextMeshProUGUI Text_Satisfaction;
    [SerializeField] private Image Image_Affection;
    [SerializeField] private Image Image_Satisfaction;

    private HeroViewModel _heroVM;

    private void OnEnable()
    {
        if (_heroVM == null)
        {
            HeroModel heroModel = new HeroModel();

            _heroVM = new HeroViewModel();
            _heroVM.Init(heroModel);

            BindViewModel();
        }
    }

    private void BindViewModel()
    {
        _heroVM.PropertyChanged += OnPropertyChanged_View;
        _heroVM.InvokeOnceOnInit();
    }

    private void OnDestroy()
    {
        _heroVM.PropertyChanged -= OnPropertyChanged_View;
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_heroVM.HeroName):
                Text_HeroName.text = _heroVM.HeroName;
                SetPortrait(_heroVM.HeroName).Forget();
                break;

            case nameof(_heroVM.Description):
                Text_Description.text = _heroVM.Description;
                break;

            case nameof(_heroVM.Disease):
                Text_DiseaseName.text = $"질환 : {_heroVM.DiseaseName}";
                break;

            case nameof(_heroVM.Age):
                Text_Age.text = $"연령 : {_heroVM.Age}";
                break;

            case nameof(_heroVM.Skill):
                Text_SkillName.text = $"능력 : {_heroVM.Skill}";
                break;

            case nameof(_heroVM.Affection):
                Text_Affection.text = _heroVM.Affection;
                Image_Affection.fillAmount = _heroVM.Affection;
                break;

            case nameof(_heroVM.Satisfaction):
                Text_Satisfaction.text = _heroVM.Satisfaction;
                Image_Satisfaction.fillAmount = _heroVM.Satisfaction;
                break;
        }
    }

    private async UniTask SetPortrait(string name)
    {
        Image_Portrait.sprite = await ResourceManager.Inst.LoadSprite($"Image/Portrait/{name}");
    }
}