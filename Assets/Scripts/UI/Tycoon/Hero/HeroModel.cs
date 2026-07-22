using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroModel
{
    public string HeroID;
    public string Name;
    public string Description;
    public List<string> DiseaseName = new List<string>();
    public string Age;
    public string Skill;

    private int _affection;
    private int _satisfaction;

    private HeroStat _targetHeroStat;

    public long RoomInstanceID {  get; set; }
    public ScheduleState[] HourlyStates { get; set; } = new ScheduleState[24];

    public Action OnUpdateSchedule;
    public Action OnStatChanged;

    public HeroModel()
    {
        for (int i = 0; i < 24; i++)
        {
            HourlyStates[i] = ScheduleState.Sleep;
        }
    }

    public int Affection
    {
        get => _affection;
        set
        {
            int clampedValue = Mathf.Clamp(value, 0, 100);

            if (_affection != clampedValue)
            {
                _affection = clampedValue;
                SaveHeroProgress();
                OnStatChanged?.Invoke();
            }
        }
    }

    public int Satisfaction
    {
        get => _satisfaction;
        set
        {
            int clampedValue = Mathf.Clamp(value, 0, 100);

            if (_satisfaction != clampedValue)
            {
                _satisfaction = clampedValue;
                SaveHeroProgress();
                OnStatChanged?.Invoke();
            }
        }
    }

    public void LoadHeroData(string heroID)
    {
        var heroData = GameDataManager.Inst.GetData<HeroData>(heroID);

        HeroID = heroID;
        Name = heroData.HeroName;
        Description = heroData.Remarks;

        DiseaseName.Clear();
        foreach (string penaltyID in heroData.PenaltyID)
        {
            DiseaseName.Add(GameDataManager.Inst.GetData<Penalty>(penaltyID).PenaltyName);
        }

        Age = heroData.Age;
        Skill = GameDataManager.Inst.GetData<HeroSkill>(heroData.MainSkillId).SkillName;

        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        _targetHeroStat = null;
        for (int i = 0; i < playerModel.HeroStats.Count; i++)
        {
            if (playerModel.HeroStats[i].HeroID == heroID)
            {
                _targetHeroStat = playerModel.HeroStats[i];
                break;
            }
        }

        if (_targetHeroStat == null)
        {
            _targetHeroStat = new HeroStat { HeroID = heroID, Affection = 50, Satisfaction = 50 };
            playerModel.HeroStats.Add(_targetHeroStat);
        }

        _affection = _targetHeroStat.Affection;
        _satisfaction = _targetHeroStat.Satisfaction;
    }

    public void SaveHeroProgress()
    {
        if (_targetHeroStat != null)
        {
            _targetHeroStat.Affection = Affection;
            _targetHeroStat.Satisfaction = Satisfaction;
        }
    }

    public void ResetSchedule()
    {
        for (int i = 0; i < 24; i++)
        {
            HourlyStates[i] = ScheduleState.Sleep;
        }

        SaveHeroProgress();
        OnUpdateSchedule?.Invoke();
    }
}