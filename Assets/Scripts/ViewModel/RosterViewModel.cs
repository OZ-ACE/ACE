using System.Collections.Generic;
using UnityEngine;
//보유(입소) 영웅 목록과 선택 파티(최대 3)를 관리하고 세이브에 저장하는 뷰모델
public class RosterViewModel : ViewModelBase
{
    private const int MaxSelectableCount = 3;
    private List<string> _selectedHeroIds = new List<string>();
    public IReadOnlyList<string> SelectedHeroIds { get { return _selectedHeroIds; } }
    public List<string> GetOwnedHeroIds()
    {
        List<string> ownedHeroIds = new List<string>();
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null || player.HeroStats == null)
        {
            return ownedHeroIds;
        }
        foreach (HeroStat heroStat in player.HeroStats)
        {
            ownedHeroIds.Add(heroStat.HeroID);
        }
        return ownedHeroIds;
    }
    //보유 영웅 ID에 해당하는 표시용 HeroData를 반환한다
    public HeroData GetHeroData(string heroId)
    {
        return GameDataManager.Inst.GetData<HeroData>(heroId);
    }
    //로스터를 열 때 이전에 저장된 선택 파티를 복원한다 (보유 목록에서 빠진 영웅은 제외)
    public void LoadSelection()
    {
        _selectedHeroIds.Clear();
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player == null || player.SelectedHeroIds == null)
        {
            OnPropertyChanged(nameof(SelectedHeroIds));
            return;
        }
        List<string> ownedHeroIds = GetOwnedHeroIds();
        foreach (string heroId in player.SelectedHeroIds)
        {
            if (ownedHeroIds.Contains(heroId) == false)
            {
                continue;
            }
            if (_selectedHeroIds.Count >= MaxSelectableCount)
            {
                break;
            }
            _selectedHeroIds.Add(heroId);
        }
        OnPropertyChanged(nameof(SelectedHeroIds));
    }
    public bool IsSelected(string heroId)
    {
        return _selectedHeroIds.Contains(heroId);
    }
    //이미 선택된 영웅이면 해제, 아니면 (3명 미만일 때) 선택한다
    public void ToggleSelect(string heroId)
    {
        if (_selectedHeroIds.Contains(heroId) == true)
        {
            _selectedHeroIds.Remove(heroId);
            OnPropertyChanged(nameof(SelectedHeroIds));
            return;
        }
        if (_selectedHeroIds.Count >= MaxSelectableCount)
        {
            return;
        }
        _selectedHeroIds.Add(heroId);
        OnPropertyChanged(nameof(SelectedHeroIds));
    }
    //정확히 3명을 골랐을 때만 전투 시작 가능
    public bool CanStartBattle()
    {
        return _selectedHeroIds.Count == MaxSelectableCount;
    }

    //선택 파티를 세이브에 저장하고, 확정된 3명 ID 목록을 반환한다
    public List<string> ConfirmSelection()
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;
        if (player != null)
        {
            player.SelectedHeroIds = new List<string>(_selectedHeroIds);
            SaveManager.Inst.RequestSaveData(player);
        }
        return new List<string>(_selectedHeroIds);
    }
}