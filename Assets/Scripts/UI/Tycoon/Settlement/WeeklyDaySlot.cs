using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 주간창 하루치 행 (일자 / 등급 / 상세보기)
public class WeeklyDaySlot : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI Text_Day;
    [SerializeField] private TextMeshProUGUI Text_Grade;
    [Header("버튼")]
    [SerializeField] private Button Button_Detail;
    private DailyEvaluationRecord _record;
    public event Action<DailyEvaluationRecord> OnClickDetail;
    private void OnEnable()
    {
        Button_Detail.onClick.RemoveListener(OnClickDetailButton);
        Button_Detail.onClick.AddListener(OnClickDetailButton);
    }
    private void OnDisable()
    {
        Button_Detail.onClick.RemoveListener(OnClickDetailButton);
    }
    // 하루치 행 세팅
    public void SetSlot(DailyEvaluationRecord record)
    {
        _record = record;
        if (record == null)
        {
            return;
        }
        if (Text_Day != null)
        {
            Text_Day.text = $"{record.Day}일차";
        }
        if (Text_Grade != null)
        {
            Text_Grade.text = GradeCalculator.GetText((EvaluationGrade)record.OverallGrade);
        }
    }
    private void OnClickDetailButton()
    {
        if (OnClickDetail != null)
        {
            OnClickDetail.Invoke(_record);
        }
    }
}