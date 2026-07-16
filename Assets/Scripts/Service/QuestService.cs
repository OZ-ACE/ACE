






// 퀘스트 뷰모델을 생성 및 보관, 진행도 보고
public class QuestService
{
    private QuestViewModel _questViewModel;

    public QuestService(ICurrencyService currencyService)
    {
        _questViewModel = new QuestViewModel(currencyService);
        _questViewModel.InitQuest();
    }

    public QuestViewModel GetQuestViewModel()
    {
        return _questViewModel;
    }

    //진행도 보고
    public void ReportProgress(QuestConditionType type, string targetID, int amount)
    {
        if (_questViewModel == null)
        {
            return;
        }
        _questViewModel.ReportProgress(type, targetID, amount);
    }

    // 슬롯 전환시 현재 슬롯 데이터 재 로드
    public void ReloadQuest()
    {
        if (_questViewModel == null)
        {
            return;
        }
        _questViewModel.ReloadQuest();
}



}