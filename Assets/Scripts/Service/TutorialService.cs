using System.Collections.Generic;
public class TutorialService
{
    private TutorialViewModel _tutorialViewModel;
    public TutorialViewModel GetTutorialViewModel()
    {
        if (_tutorialViewModel == null)
        {
            _tutorialViewModel = new TutorialViewModel();
        }
        return _tutorialViewModel;
    }
    public void TryShowTutorial(TutorialTriggerType triggerType, string triggerValue)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null || player.IsTutorialDisabled == true)
        {
            return;
        }
        string key = triggerType.ToString() + ":" + triggerValue;   // 트리거 단위 '봤음' 키
        if (player.ShownTutorialIds.Contains(key) == true)
        {
            return;
        }
        List<Tutorial> all = GameDataManager.Inst.GetDataList<Tutorial>();
        if (all == null)
        {
            return;
        }
        List<Tutorial> pages = new List<Tutorial>();
        for (int i = 0; i < all.Count; i++)
        {
            Tutorial t = all[i];
            if (t.GetTriggerType() != triggerType)
            {
                continue;
            }
            if (triggerType == TutorialTriggerType.QuestReward && t.TriggerValue != triggerValue)
            {
                continue;
            }
            pages.Add(t);
        }
        if (pages.Count == 0)
        {
            return;
        }
        pages.Sort(ComparePageOrder);   // Order 오름차순
        player.ShownTutorialIds.Add(key);
        SaveManager.Inst.RequestSaveData(player);
        GetTutorialViewModel().SetTutorial(pages);
        UIManager.Inst.OpenTutorialUI();
    }
    private static int ComparePageOrder(Tutorial a, Tutorial b)
    {
        return a.Order.CompareTo(b.Order);
    }
}