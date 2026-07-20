using System;
using System.Collections.Generic;

public class HeroModel
{
    public string HeroID;
    public string Name;
    public string Description;
    public List<string> DiseaseName = new List<string>();
    public string Age;
    public string Skill;

    public int Affection;
    public int Satisfaction;

    private HeroStat _targetHeroStat;

    public ScheduleState[] HourlyStates { get; set; } = new ScheduleState[24];

    public Action OnUpdateSchedule;

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
            _targetHeroStat = new HeroStat { HeroID = heroID, Affection = 0, Satisfaction = 0 };
            playerModel.HeroStats.Add(_targetHeroStat);
        }

        Affection = _targetHeroStat.Affection;
        Satisfaction = _targetHeroStat.Satisfaction;
    }

    public void SaveHeroProgress()
    {
        if (_targetHeroStat != null)
        {
            _targetHeroStat.Affection = Affection;
            _targetHeroStat.Satisfaction = Satisfaction;
            
            //SaveManager.Inst.RequestSaveData(SaveManager.Inst.CurrentPlayerModel);
        }
    }
}