using TMPro;
using UnityEngine;
// 결산창 영웅 1명 평가 슬롯
public class SettlementHeroSlot : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI Text_HeroName;
    [SerializeField] private TextMeshProUGUI Text_AffectionGrade;
    [SerializeField] private TextMeshProUGUI Text_SatisfactionGrade;
    // 영웅 1명 평가 표시
    public void SetSlot(HeroEvaluation eval)
    {
        if (eval == null)
        {
            return;
        }
        if (Text_HeroName != null)
        {
            Text_HeroName.text = GameUtil.GetUnitDisplayName(eval.HeroId);
        }
        if (Text_AffectionGrade != null)
        {
            Text_AffectionGrade.text = GradeCalculator.GetText(eval.Affection);
        }
        if (Text_SatisfactionGrade != null)
        {
            Text_SatisfactionGrade.text = GradeCalculator.GetText(eval.Satisfaction);
        }
    }
}