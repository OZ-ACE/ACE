using System.Collections.Generic;
using Unity.VisualScripting;

public class HeroViewModel : ViewModelBase
{
    private HeroModel _model;

    private string _heroID;
    public string HeroID => _heroID;

    private string _heroName;
    public string HeroName => _heroName;

    private string _description;
    public string Description => _description;

    private List<string> _diseaseName;
    public List<string> DiseaseName => _diseaseName;

    private string _age;
    public string Age => _age;

    private string _skill;
    public string Skill => _skill;

    private int _affection;
    public int Affection
    {
        get => _affection;
        set
        {
            if (_affection != value)
            {
                _affection = value;
                OnPropertyChanged(nameof(Affection));
            }
        }
    }

    private int _satisfaction;
    public int Satisfaction
    {
        get => _satisfaction;
        set
        {
            if (_satisfaction != value)
            {
                _satisfaction = value;
                OnPropertyChanged(nameof(Satisfaction));
            }
        }
    }

    private bool _isSelect;
    public bool IsSelect
    {
        get => _isSelect;
        set
        {
            if (_isSelect != value)
            {
                _isSelect = value;
                OnPropertyChanged(nameof(IsSelect));
            }
        }
    }

    public void Init(HeroModel model)
    {
        _model = model;

        _heroID = _model.HeroID;
        _heroName = _model.Name;
        _description = _model.Description;
        _diseaseName = _model.DiseaseName;
        _age = _model.Age;
        _skill = _model.Skill;
        
        _affection = _model.Affection;
        _satisfaction = _model.Satisfaction;

        _isSelect = false;
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(HeroName));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(DiseaseName));
        OnPropertyChanged(nameof(Age));
        OnPropertyChanged(nameof(Skill));
        OnPropertyChanged(nameof(Satisfaction));
        OnPropertyChanged(nameof(Affection));
        OnPropertyChanged(nameof(IsSelect));
    }

    public void AddAffection(int amount)
    {
        Affection += amount;

        _model.Affection = Affection;
        _model.SaveHeroProgress();
    }

    public void AddSatisfaction(int amount)
    {
        Satisfaction += amount;

        _model.Satisfaction = Satisfaction;
        _model.SaveHeroProgress();
    }
}