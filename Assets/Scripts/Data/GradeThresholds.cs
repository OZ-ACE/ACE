using System;
using UnityEngine;
// 한 지표의 등급 구간 (이 값 이상이면 해당 등급, 내림차순 판정)
[Serializable]
public class GradeThresholds
{
    [Tooltip("이 값 이상이면 A")] public float A = 80f;
    [Tooltip("이 값 이상이면 B")] public float B = 60f;
    [Tooltip("이 값 이상이면 C")] public float C = 40f;
    [Tooltip("이 값 이상이면 D (미만은 F)")] public float D = 20f;
    // 값 -> 등급
    public EvaluationGrade GetGrade(float value)
    {
        if (value >= A)
        {
            return EvaluationGrade.A;
        }
        if (value >= B)
        {
            return EvaluationGrade.B;
        }
        if (value >= C)
        {
            return EvaluationGrade.C;
        }
        if (value >= D)
        {
            return EvaluationGrade.D;
        }
        return EvaluationGrade.F;
    }
}