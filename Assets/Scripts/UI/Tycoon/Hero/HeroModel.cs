using UnityEngine;

public class HeroModel
{
    public string HeroID;
    public string Name;
    public string Description;
    public string DiseaseName;
    public int Age;
    public string Skill;

    public int Affection;
    public int Satisfaction;

    public void LoadHeroData(string heroID)
    {
        var heroData = GameDataManager.Inst.GetData<HeroBasic>(heroID);

        HeroID = heroID;
        Name = heroData.Name;
        Description = heroData.Description;
        DiseaseName = heroData.DiseaseName;
        Age = heroData.Age;
        Skill = heroData.Skill;

        Affection = SaveManager.Inst.CurrentPlayerModel.Hero.Affection;
        Satisfaction = SaveManager.Inst.CurrentPlayerModel.Hero.Satisfaction;
    }

    public void SaveHeroProgress()
    {
        SaveManager.Inst.CurrentPlayerModel.Hero.Affection = Affection;
        SaveManager.Inst.CurrentPlayerModel.Hero.Satisfaction = Satisfaction;
    }
}
