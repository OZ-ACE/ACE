using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroViewModel : ViewModelBase
{
    private HeroModel _model;
    public HeroModel Model => _model;

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

    public string PrimeLevel
    {
        get
        {
            PlayerModel playerModel = SaveManager.Inst?.CurrentPlayerModel;
            int participateCount = 0;

            if (playerModel != null && playerModel.HeroProgressList != null)
            {
                for (int i = 0; i < playerModel.HeroProgressList.Count; i++)
                {
                    HeroProgressModel heroProgress = playerModel.HeroProgressList[i];

                    if (heroProgress != null && string.Equals(heroProgress.HeroId, _heroID, StringComparison.OrdinalIgnoreCase))
                    {
                        participateCount = heroProgress.BattleParticipateCount;
                        break;
                    }
                }
            }

            if (BattleManager.Inst != null)
            {
                int level = BattleManager.Inst.CalculatePrimeLevel(participateCount);
                return level.ToString();
            }

            return "0";
        }
    }

    public int Affection
    {
        get => _model.Affection;
        set
        {
            if (_model.Affection != value)
            {
                _model.Affection = value;
                OnPropertyChanged(nameof(Affection));
            }
        }
    }

    public int Satisfaction
    {
        get => _model.Satisfaction;
        set
        {
            if (_model.Satisfaction != value)
            {
                _model.Satisfaction = value;
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
        if (_model != null)
        {
            _model.OnStatChanged -= HandleStatChanged;
        }

        _model = model;

        _heroID = _model.HeroID;
        _heroName = _model.Name;
        _description = _model.Description;
        _diseaseName = _model.DiseaseName;
        _age = _model.Age;
        _skill = _model.Skill;

        _model.OnStatChanged += HandleStatChanged;

        _isSelect = false;
    }

    private void HandleStatChanged()
    {
        OnPropertyChanged(nameof(Affection));
        OnPropertyChanged(nameof(Satisfaction));
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
        OnPropertyChanged(nameof(PrimeLevel));
        OnPropertyChanged(nameof(IsSelect));
    }
}