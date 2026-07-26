using UnityEngine;

public class ScheduleEvaluator
{
    private const int SCORE_SUN = 5;
    private const int SCORE_SLEEP = 10;
    private const int SCORE_SHOWER = 5;
    private const int SCORE_BEDROOM_ADJACENT = 5;
    private const int SCORE_DINING = 5;
    private const int SCORE_COUNSEL = 5;

    public static void EvaluateDailySchedule(HeroModel heroModel, bool isBedroomAdjacent, bool hasDiningRoom, bool hasCounselRoom)
    {
        ScheduleState[] states = heroModel.HourlyStates;

        int sunCount = 0;
        int sleepCount = 0;
        int showerCount = 0;
        int restCount = 0;

        for (int i = 0; i < states.Length; i++)
        {
            switch (states[i])
            {
                case ScheduleState.Sun:
                    sunCount++;
                    break;

                case ScheduleState.Sleep:
                    sleepCount++;
                    break;

                case ScheduleState.Shower:
                    showerCount++;
                    break;

                case ScheduleState.Rest:
                    restCount++;
                    break;
            }
        }

        int deltaSatisfaction = 0;
        int deltaAffection = 0;

        if (sunCount >= 1)
        {
            deltaSatisfaction += SCORE_SUN;
            deltaAffection += SCORE_SUN;
        }
        else
        {
            deltaSatisfaction -= SCORE_SUN;
            deltaAffection -= SCORE_SUN;
        }

        if (sleepCount >= 7)
        {
            deltaSatisfaction += SCORE_SLEEP;
            deltaAffection += SCORE_SLEEP;
        }
        else
        {
            deltaSatisfaction -= SCORE_SLEEP;
            deltaAffection -= SCORE_SLEEP;
        }

        if (showerCount >= 1)
        {
            deltaSatisfaction += SCORE_SHOWER;
            deltaAffection += SCORE_SHOWER;
        }
        else
        {
            deltaSatisfaction -= SCORE_SHOWER;
            deltaAffection -= SCORE_SHOWER;
        }

        if (isBedroomAdjacent == true)
        {
            deltaSatisfaction += SCORE_BEDROOM_ADJACENT;
        }
        if (hasDiningRoom == true)
        {
            deltaAffection += SCORE_DINING;
        }

        if (hasCounselRoom == true)
        {
            deltaAffection += SCORE_COUNSEL;
        }

        heroModel.Satisfaction = Mathf.Clamp(heroModel.Satisfaction + deltaSatisfaction, 0, 100);
        heroModel.Affection = Mathf.Clamp(heroModel.Affection + deltaAffection, 0, 100);

        GameManager.Inst.Services.HeroRequestService.EvaluateScheduleRequest(heroModel, sunCount, sleepCount, showerCount, restCount);
        heroModel.SaveHeroProgress();

        Debug.Log($"[{heroModel.HeroID}] 침실인접:{isBedroomAdjacent}, 식당:{hasDiningRoom}, 상담실:{hasCounselRoom} → 만족:{deltaSatisfaction}/{heroModel.Satisfaction}, 호감:{deltaAffection}/{heroModel.Affection}");

        heroModel.ResetSchedule();
    }
}