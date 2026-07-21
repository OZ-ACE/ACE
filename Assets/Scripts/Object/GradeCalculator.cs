using System.Collections.Generic;
using UnityEngine;
// 등급 평균 계산·표시 유틸
public static class GradeCalculator
{
    // 등급 리스트의 평균 등급 (정수 점수 평균 후 반올림)
    public static EvaluationGrade GetAverage(List<EvaluationGrade> grades)
    {
        if (grades == null || grades.Count == 0)
        {
            return EvaluationGrade.F;
        }
        int sum = 0;
        for (int i = 0; i < grades.Count; i++)
        {
            sum += (int)grades[i];
        }
        float average = (float)sum / grades.Count;
        int rounded = Mathf.RoundToInt(average);
        return (EvaluationGrade)Mathf.Clamp(rounded, 0, 4);
    }
    // 등급 -> 표시 문자
    public static string GetText(EvaluationGrade grade)
    {
        return grade.ToString();
    }
}