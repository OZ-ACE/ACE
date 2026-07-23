using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_Name;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_Progress;
    [SerializeField] private TextMeshProUGUI Text_Reward;
    [SerializeField] private Image Image_ProgressFill;
    [SerializeField] private Button Button_Claim;

    private string _questID;
    private int _conditionCount;
    private QuestViewModel _questVM;

    // 퀘스트 데이터로 슬롯 초기화
    public void SetSlotData(QuestData quest, QuestViewModel vm)
    {
        _questID = quest.ID;
        _conditionCount = quest.ConditionCount;
        _questVM = vm;

        Button_Claim.onClick.RemoveListener(OnClickClaim);
        Button_Claim.onClick.AddListener(OnClickClaim);

        Text_Name.text = quest.QuestName;
        Text_Description.text = quest.Description;
        Text_Reward.text = GetRewardText(quest);

        UpdateState();
    }

    // 진행도·버튼 상태 갱신
    public void UpdateState()
    {
        if (_questVM == null)
        {
            return;
        }

        int current = _questVM.GetProgressCount(_questID);

        QuestState state = _questVM.GetState(_questID);

        if (Text_Progress != null)
        {
            Text_Progress.text = $"{current} / {_conditionCount}";
        }

        if (Image_ProgressFill != null)
        {
            Image_ProgressFill.fillAmount = GetFillRatio(current);
        }

        Button_Claim.gameObject.SetActive(state == QuestState.Completed);
    }

    private float GetFillRatio(int current)
    {
        if (_conditionCount <= 0)
        {
            return 0f;
        }
        return Mathf.Clamp01((float)current / _conditionCount);
    }

    private string GetRewardText(QuestData quest)
    {
        QuestRewardType type = quest.GetRewardType();
        if (type == QuestRewardType.Gold)
        {
            return $"{quest.RewardAmount} Gold";
        }
        if (type == QuestRewardType.MemoryFragment)
        {
            return $"파편 {quest.RewardAmount}";
        }
        return string.Empty;
    }

    private void OnClickClaim()
    {
        if (_questVM.TryClaimReward(_questID) == false)
        {
            Debug.Log($"[QuestSlot] 보상 수령 실패: {_questID}");
        }
        else
        {
            SoundManager.Inst.PlaySFX("Success");

            string dialogueID = GameDataManager.Inst.GetData<QuestData>(_questID).DialogueID;
            if (!string.IsNullOrEmpty(dialogueID))
            {
                GameManager.Inst.SetDialogueID(dialogueID);

                if (dialogueID.Contains("Ending"))
                {
                    GameManager.Inst.SetDialogueID(GetEndingID());
                }

                UIManager.Inst.OpenDialogueUI();
                UIManager.Inst.CloseTycoonMainUI();
                ObjectManager.Inst.HideGridView();
            }

            this.gameObject.SetActive(false);
        }
    }

    private string GetEndingID()
    {
        PlayerModel playerModel = SaveManager.Inst.CurrentPlayerModel;

        int totalPrimeLevel = 0;
        int heroCount = playerModel.HeroStats.Count;

        for (int i = 0; i < heroCount; i++)
        {
            string heroID = playerModel.HeroStats[i].HeroID;
            int participateCount = 0;

            if (playerModel.HeroProgressList != null)
            {
                for (int j = 0; j < playerModel.HeroProgressList.Count; j++)
                {
                    var progress = playerModel.HeroProgressList[j];
                    if (progress != null && string.Equals(progress.HeroId, heroID, System.StringComparison.OrdinalIgnoreCase))
                    {
                        participateCount = progress.BattleParticipateCount;
                        break;
                    }
                }
            }

            if (BattleManager.Inst != null)
            {
                totalPrimeLevel += BattleManager.Inst.CalculatePrimeLevel(participateCount);
            }
        }

        float avgPrime = (float)totalPrimeLevel / heroCount;

        if (avgPrime >= 1.5f)
        {
            return "Ending_Happy_01";
        }
        else
        {
            return "Ending_Bad_01";
        }
    }
}