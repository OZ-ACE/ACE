using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 결과(승패/보상/라운드)를 보여주고 확인 버튼으로 닫히는 팝업
public class BattleResultPopupUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_ResultTitle;
    [SerializeField] private TextMeshProUGUI Text_RewardAmount;
    [SerializeField] private TextMeshProUGUI Text_RoundCount;
    [SerializeField] private Button Button_Confirm;

    public event Action OnConfirmed;

    private void Awake()
    {
        Button_Confirm.onClick.AddListener(OnClickConfirm);
    }

    //전투 결과를 받아 팝업 내용을 채우고 연다
    public void OpenPopup(BattleResult result, int rewardAmount, int roundCount)
    {
        Text_ResultTitle.text = result == BattleResult.Victory ? "전투 승리!" : "전투 패배";
        Text_RewardAmount.text = $"획득 재화: 기억의 파편 {rewardAmount}";
        Text_RoundCount.text = $"소요 라운드 {roundCount}턴";

        gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    private void OnClickConfirm()
    {
        ClosePopup();
        GameManager.Inst.Services.QuestService.ReportProgress(QuestConditionType.Battle, "", 1);
        OnConfirmed?.Invoke();
    }
}