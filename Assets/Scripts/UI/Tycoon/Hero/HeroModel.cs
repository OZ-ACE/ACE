using UnityEngine;

public class HeroModel
{
    public string HeroID;
    public string Name;
    public string Description;
    public string DiseaseName;
    public string Age;
    public string Skill;

    public int Affection;
    public int Satisfaction;

    public void LoadHeroData(string heroID)
    {
        var heroData = GameDataManager.Inst.GetData<HeroData>(heroID);

        HeroID = heroID;
        Name = heroData.HeroName;
        Description = heroData.Description;
        DiseaseName = heroData.DiseaseName;
        Age = heroData.Age;
        Skill = heroData.Skill;

        Affection = SaveManager.Inst.CurrentPlayerModel.HeroStats.Affection;
        Satisfaction = SaveManager.Inst.CurrentPlayerModel.HeroStats.Satisfaction;
    }

    public void SaveHeroProgress()
    {
        SaveManager.Inst.CurrentPlayerModel.HeroStats.Affection = Affection;
        SaveManager.Inst.CurrentPlayerModel.HeroStats.Satisfaction = Satisfaction;
    }
}
