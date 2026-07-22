using UnityEngine;

public class ScheduleEvaluator
{
    private const int SCORE_SUN = 5;
    private const int SCORE_SLEEP = 10;
    private const int SCORE_SHOWER = 5;

    public static void EvaluateDailySchedule(HeroModel heroModel)
    {
        ScheduleState[] states = heroModel.HourlyStates;

        int sunCount = 0;
        int sleepCount = 0;
        int showerCount = 0;

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

        heroModel.Satisfaction = Mathf.Clamp(heroModel.Satisfaction + deltaSatisfaction, 0, 100);
        heroModel.Affection = Mathf.Clamp(heroModel.Affection + deltaAffection, 0, 100);

        heroModel.SaveHeroProgress();

        Debug.Log($"햇빛:{sunCount}, 수면:{sleepCount}, 샤워:{showerCount} 만족:{deltaSatisfaction}, 호감:{deltaAffection}");

        heroModel.ResetSchedule();
    }
}