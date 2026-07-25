using TMPro;
using UnityEngine;

public class HeroRequestSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_RequestName;
    [SerializeField] private TMP_Text Text_Description;
    [SerializeField] private TMP_Text Text_Reward;
    [SerializeField] private TMP_Text Text_Time;

    public void SetData(HeroRequestModel requestModel, HeroRequestData requestData, int currentHour)
    {
        if (requestModel == null || requestData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        Text_RequestName.text = requestData.RequestName;
        Text_Description.text = requestData.Description;
        Text_Reward.text = $"+{requestData.RewardAffection}";
        
        int remainingHour = Mathf.Max(0, requestModel.ExpireHour - currentHour);
        Text_Time.text = $"{remainingHour}시간 남음";

        switch ((HeroRequestState)requestModel.State)
        {
            case HeroRequestState.InProgress:
                // 기본 상태
                break;

            case HeroRequestState.Rewarded:
                Text_Time.text = "완료";
                break;

            case HeroRequestState.Failed:
                Text_Time.text = "실패";

                Text_RequestName.color = Color.gray;
                Text_Description.color = Color.gray;
                Text_Reward.color = Color.gray;
                Text_Time.color = Color.red;
                break;
        }
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }
}