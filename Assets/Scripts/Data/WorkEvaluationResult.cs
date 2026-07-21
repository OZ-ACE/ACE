using System.Collections.Generic;
// 업무평가 결과
public class WorkEvaluationResult
{
    public EvaluationGrade Overall;          // 종합
    public EvaluationGrade HeroManageGrade;  // 영웅관리(호감도+만족도)
    public EvaluationGrade GoldGrade;        // 돈
    public EvaluationGrade FragmentGrade;    // 파편
    public List<HeroEvaluation> Heroes = new List<HeroEvaluation>();
}
// 영웅 1명 평가
public class HeroEvaluation
{
    public string HeroId;
    public EvaluationGrade Affection;     // 호감도
    public EvaluationGrade Satisfaction;  // 만족도
}