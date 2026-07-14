using UnityEngine;

/// <summary>
/// DayService 임시 테스트. 빈 오브젝트에 붙이고 인스펙터 버튼처럼 쓴다.
/// 확인 후 삭제.
/// </summary>
public class DayServiceTester : MonoBehaviour
{
    [ContextMenu("전투 완료 표시")]
    private void MarkBattleDone()
    {
        GameManager.Inst.Services.DayService.MarkBattleDone();
    }

    [ContextMenu("다음 날로")]
    private void TryAdvanceDay()
    {
        bool success = GameManager.Inst.Services.DayService.TryAdvanceDay();
        Debug.Log($"[Tester] 다음날 시도: {(success ? "성공" : "실패 - 전투 먼저")}");
    }

    [ContextMenu("현재 상태 출력")]
    private void PrintState()
    {
        DayService day = GameManager.Inst.Services.DayService;
        Debug.Log($"[Tester] Day {day.CurrentDay}, 전투완료: {day.IsBattleDoneToday}");
    }
}