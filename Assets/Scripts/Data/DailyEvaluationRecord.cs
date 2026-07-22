using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 하루치 업무평가 저장(주간 평균 및 상세 재열람 용도)
public class DailyEvaluationRecord
{
    public int Day;
    public int TodayFragment;
    public int TotalFragment;
    public int Gold;
    public int OverallGrade;
    public int HeroManageGrade;
    public int GoldGrade;
    public int FragmentGrade;

    public List<HeroDailyEvaluation> Heroes = new List<HeroDailyEvaluation>();

}
