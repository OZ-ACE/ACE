using System.Collections.Generic;
using UnityEngine;
public class TutorialService
{
    private TutorialViewModel _tutorialViewModel;
    public TutorialViewModel GetTutorialViewModel()
    {
        if (_tutorialViewModel == null)
        {
            _tutorialViewModel = new TutorialViewModel();
        }
        return _tutorialViewModel;
    }
    public void TryShowTutorial(TutorialTriggerType triggerType, string triggerValue)
    {
        Debug.Log($"[Tutorial] TryShow 진입: {triggerType} / {triggerValue}");
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null)
        {
            Debug.Log("[Tutorial] player 없음");
            return;
        }
        if (player.IsTutorialDisabled == true)
        {
            Debug.Log("[Tutorial] 무시 체크됨 → 스킵");
            return;
        }
        List<Tutorial> all = GameDataManager.Inst.GetDataList<Tutorial>();
        if (all == null || all.Count == 0)
        {
            Debug.Log("[Tutorial] Tutorial 데이터가 없음 (로더 등록/파일명 확인!)");
            return;
        }
        Debug.Log($"[Tutorial] 데이터 {all.Count}개 로드됨");
        string groupId = null;
        for (int i = 0; i < all.Count; i++)
        {
            Tutorial t = all[i];
            if (t.GetTriggerType() != triggerType)
            {
                continue;
            }
            if (triggerType == TutorialTriggerType.QuestReward && t.TriggerValue != triggerValue)
            {
                continue;
            }
            groupId = t.GroupID;
            break;
        }
        if (string.IsNullOrEmpty(groupId) == true)
        {
            Debug.Log($"[Tutorial] 트리거 매칭 그룹 없음: {triggerType}/{triggerValue}");
            return;
        }
        if (player.ShownTutorialIds.Contains(groupId) == true)
        {
            Debug.Log($"[Tutorial] 이미 본 그룹: {groupId}");
            return;
        }
        List<Tutorial> pages = new List<Tutorial>();
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].GroupID == groupId)
            {
                pages.Add(all[i]);
            }
        }
        if (pages.Count == 0)
        {
            Debug.Log($"[Tutorial] 그룹에 페이지 없음: {groupId}");
            return;
        }
        pages.Sort(ComparePageOrder);
        player.ShownTutorialIds.Add(groupId);
        SaveManager.Inst.RequestSaveData(player);
        GetTutorialViewModel().SetTutorial(pages);
        Debug.Log($"[Tutorial] 표시: {groupId}, 페이지 {pages.Count}개");
        UIManager.Inst.OpenTutorialUI();
    }
    private static int ComparePageOrder(Tutorial a, Tutorial b)
    {
        return a.Order.CompareTo(b.Order);
    }

    // 튜토리얼 다시보기 — 전체를 순서대로 표시 (트리거·이미 봤음 무시)
    public void ShowAllTutorials()
    {
        List<Tutorial> all = GameDataManager.Inst.GetDataList<Tutorial>();
        if (all == null || all.Count == 0)
        {
            return;
        }
        List<Tutorial> pages = new List<Tutorial>(all);
        pages.Sort(CompareAllOrder);   // 그룹 → 페이지 순서
        GetTutorialViewModel().SetTutorial(pages);
        UIManager.Inst.OpenTutorialUI();
    }
    private static int CompareAllOrder(Tutorial a, Tutorial b)
    {
        int groupCompare = string.CompareOrdinal(a.GroupID, b.GroupID);
        if (groupCompare != 0)
        {
            return groupCompare;
        }
        return a.Order.CompareTo(b.Order);
    }
}