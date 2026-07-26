using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroRequestSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_RequestName;
    [SerializeField] private TMP_Text Text_Description;

    [SerializeField] private TMP_Text Text_Reward;
    [SerializeField] private Image Image_RewardIcon;

    [SerializeField] private Sprite Sprite_Affection;
    [SerializeField] private Sprite Sprite_Satisfaction;

    [SerializeField] private TMP_Text Text_Time;
    [SerializeField] private TMP_Text Text_Progress;

    [SerializeField] private Image Image_Progress;
    [SerializeField] private Image Image_Profile;

    [SerializeField] private Button Button_Shortcut;

    public Action OnClickShortcut;

    private const string EVENT_LABEL = "[돌발 이벤트]";

    private void Awake()
    {
        Button_Shortcut.onClick.AddListener(OnClickGoToSchedule);
    }

    private void OnDestroy()
    {
        Button_Shortcut.onClick.RemoveListener(OnClickGoToSchedule);
    }

    public void OnClickGoToSchedule()
    {
        Debug.Log("HeroRequestSlot - 일정 바로가기 클릭");

        OnClickShortcut?.Invoke();
    }

    public void SetData(HeroRequestModel requestModel, HeroRequestData requestData, int currentHour)
    {
        if (requestModel == null || requestData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (requestData.RewardAffection > 0)
        {
            Image_RewardIcon.sprite = Sprite_Affection;
            Text_Reward.text = $"+ {requestData.RewardAffection}";
        }
        else if (requestData.RewardSatisfaction > 0)
        {
            Image_RewardIcon.sprite = Sprite_Satisfaction;
            Text_Reward.text = $"+ {requestData.RewardSatisfaction}";
        }
        else
        {
            Image_RewardIcon.enabled = false;
            Text_Reward.text = string.Empty;
        }

        Text_RequestName.text = $"{EVENT_LABEL} {requestData.RequestName}";
        Text_Description.text = requestData.Description;

        Text_Progress.text = $"{requestModel.CurrentProgress}h / {requestData.RequiredHour}h";

        if (requestData.RequiredHour > 0)
        {
            Image_Progress.fillAmount = (float)requestModel.CurrentProgress / requestData.RequiredHour;
        }
        else
        {
            Image_Progress.fillAmount = 0f;
        }

        int remainingHour = Mathf.Max(0, requestModel.ExpireHour - currentHour);

        Text_Time.text = $"{remainingHour}시간 남음";

        SetProfileImage(requestData.HeroId);

        switch ((HeroRequestState)requestModel.State)
        {
            case HeroRequestState.InProgress:
                break;

            case HeroRequestState.Rewarded:
                Text_Time.text = "완료";
                break;

            case HeroRequestState.Failed:
                Text_Time.text = "실패";
                // [TODO] 빨간색 텍스트 > 패널위에 실패/완료 네모난 도장아이콘 찍어주고 싶음
                break;
        }
    }

    private void SetProfileImage(string heroId)
    {
        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(heroId);

        if (heroData == null || string.IsNullOrEmpty(heroData.ProfileImage))
        {
            Image_Profile.enabled = false;
            return;
        }

        Image_Profile.enabled = false;

        ResourceManager.Inst.LoadSprite(heroData.ProfileImage, sprite =>
        {
            if (sprite == null)
            {
                Image_Profile.enabled = false;
                return;
            }

            Image_Profile.sprite = sprite;
            Image_Profile.enabled = true;
        });
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }
}