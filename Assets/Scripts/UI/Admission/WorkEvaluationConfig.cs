using UnityEngine;
// 업무평가 등급 구간 설정 (인스펙터에서 직접 수정)
[CreateAssetMenu(fileName = "WorkEvaluationConfig", menuName = "Game/WorkEvaluationConfig")]
public class WorkEvaluationConfig : ScriptableObject
{
    [Header("영웅 호감도 (기본 50, 20단위)")]
    public GradeThresholds Affection = new GradeThresholds();
    [Header("영웅 만족도 (기본 50, 20단위)")]
    public GradeThresholds Satisfaction = new GradeThresholds();
    [Header("돈 소지량 (1000단위)")]
    public GradeThresholds Gold = new GradeThresholds();
    [Header("파편 소지량 (1000단위)")]
    public GradeThresholds Fragment = new GradeThresholds();
}