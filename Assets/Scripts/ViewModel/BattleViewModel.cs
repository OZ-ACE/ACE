using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//전투 참가자 데이터를 BattleUnitModel로 변환하고 턴 순서를 조립하는 뷰모델
public class BattleViewModel : ViewModelBase
{
    public List<string> BattleLogs = new List<string>();
    public List<BattleActionModel> ActionQueue = new List<BattleActionModel>();

    public event Action<BattleUnitModel> UnitHpChanged;
    public event Action<BattleUnitModel> UnitAttackStarted;
    public event Action<BattleUnitModel> UnitHit;
    public event Action<BattleUnitModel> UnitDied;
    public event Action<BattleUnitModel> UnitHitVfxRequested;
    public event Action<List<BattleUnitModel>> HeroListChanged;

    private const int AttackAnimationDelayMilliseconds = 800;
    private const int HitAnimationDelayMilliseconds = 400;
    private const int ActionQueueStackDelayMilliseconds = 120;

    private UniTaskCompletionSource _interventionCompletionSource;

    //교체는 라운드 종료 후 일괄 반영한다. 순회 중 리스트를 수정하면 예외가 나기 때문에 예약만 해둔다
    private BattleUnitModel _pendingChangeOutUnit;
    private string _pendingChangeInHeroId;

    //영웅 ID로 전투용 BattleUnitModel을 만든다. 데이터가 없으면 null을 반환한다
    private BattleUnitModel CreateHeroBattleUnit(string heroId)
    {
        HeroBattleData data = GameDataManager.Inst.GetData<HeroBattleData>(heroId);

        if (data == null)
        {
            return null;
        }

        BattleUnitModel unit = new BattleUnitModel();
        unit.ID = data.ID;
        unit.IsHero = true;
        unit.Speed = data.Speed;
        unit.AttackPower = data.AttackPower;
        unit.MaxHp = data.MaxHp;
        unit.CurrentHp = data.MaxHp;

        foreach (HeroSkill heroSkill in GameDataManager.Inst.GetDataList<HeroSkill>())
        {
            if (heroSkill.HeroId != unit.ID)
            {
                continue;
            }

            unit.SkillIdList.Add(heroSkill.ID);
        }

        return unit;
    }

    public List<BattleUnitModel> GetBattleTurnOrder(List<string> heroIds, List<string> enemyIds)
    {
        List<BattleUnitModel> participats = new List<BattleUnitModel>();

        foreach (string heroId in heroIds)
        {
            BattleUnitModel unit = CreateHeroBattleUnit(heroId);

            if (unit == null)
            {
                continue;
            }

            participats.Add(unit);
        }

        foreach (string enemyId in enemyIds)
        {
            EnemyBattleData data = GameDataManager.Inst.GetData<EnemyBattleData>(enemyId);

            if (data == null)
            {
                continue;
            }

            BattleUnitModel unit = new BattleUnitModel();
            unit.ID = data.ID;
            unit.IsHero = false;
            unit.Speed = data.Speed;
            unit.AttackPower = data.AttackPower;
            unit.MaxHp = data.MaxHp;
            unit.CurrentHp = data.MaxHp;

            foreach (EnemySkill enemySkill in GameDataManager.Inst.GetDataList<EnemySkill>())
            {
                if (enemySkill.EnemyId != unit.ID)
                {
                    continue;
                }

                unit.SkillIdList.Add(enemySkill.ID);
            }

            participats.Add(unit);
        }

        return TurnManager.Inst.GetTurnOrder(participats);
    }

    //게임화면에 출력되는 배틀로그 추가
    public void AddBattleLog(string message)
    {
        BattleLogs.Add(message);

        OnPropertyChanged(nameof(BattleLogs));
    }

    //배틀 로그를 전부 비우고 View에 갱신을 알린다 (전투 재진입 초기화용)
    public void ClearBattleLog()
    {
        BattleLogs.Clear();
        OnPropertyChanged(nameof(BattleLogs));
    }

    //턴 순서대로 유닛을 하나씩 BT에 넘기고, 결과가 올 때까지 기다렸다가 다음 유닛으로 진행한다
    public async UniTask RunRoundAsync(
        List<BattleUnitModel> turnOrder,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        CancellationToken token)
    {
        LogPenaltyReleases(turnOrder);
        BattleManager.Inst.BuildActionQueue(turnOrder);

        foreach (BattleUnitModel unit in turnOrder)
        {
            if (unit == null || unit.IsDefeated)
            {
                continue;
            }
            
            BattleBTExecutor executor = unit.IsHero ? BattleManager.Inst.HeroExecutor : BattleManager.Inst.EnemyExecutor;

            if (executor == null)
            {
                Debug.LogWarning($"[BattleViewModel] {unit.ID} 진영의 BattleBTExecutor가 연결되지 않음");
                continue;
            }

            BattleActionModel createdAction = await RequestUnitActionAsync(executor, unit, heroList, enemyList, token);

            if (createdAction == null)
            {
                continue;
            }

            bool isEnqueued = BattleManager.Inst.EnqueueUnitAction(createdAction);

            if (isEnqueued == false)
            {
                continue;
            }

            AddBattleLog(BuildUnitActionLogMessage(createdAction));
            RefreshActionQueue();
            await UniTask.Delay(ActionQueueStackDelayMilliseconds, cancellationToken: token);
        }

        BattleManager.Inst.EnqueuePlayerAction();
        RefreshActionQueue();

        await WaitForInterventionEndAsync(token);

        await ResolveActionQueueAsync(heroList, enemyList, token);

        ApplyPendingHeroChange(turnOrder, heroList);

        RefreshActionQueue();
    }


    //개입 턴 종료 버튼이 눌렸을 때 View에서 호출한다
    public void NotifyInterventionEnded()
    {
        _interventionCompletionSource?.TrySetResult();
    }

    //라운드 진행 중 플레이어의 개입 턴 종료 버튼 클릭을 기다린다
    private async UniTask WaitForInterventionEndAsync(CancellationToken token)
    {
        _interventionCompletionSource = new UniTaskCompletionSource();

        await _interventionCompletionSource.Task.AttachExternalCancellation(token);
    }

    //한 유닛의 BT 실행을 요청하고, BattleActionCreated 이벤트가 울릴 때까지 기다린다
    private async UniTask<BattleActionModel> RequestUnitActionAsync(
        BattleBTExecutor executor,
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        CancellationToken token)
    {
        Debug.Log($"[BattleViewModel] {unit.ID} 행동 요청 시작");

        UniTaskCompletionSource<BattleActionModel> completionSource = new UniTaskCompletionSource<BattleActionModel>();

        void OnActionCreated(BattleActionModel action)
        {
            Debug.Log($"[BattleViewModel] {unit.ID} 행동 생성 완료");
            completionSource.TrySetResult(action);
        }

        executor.BattleActionCreated += OnActionCreated;

        bool isExecuted = executor.ExecuteBattleAction(unit, heroList, enemyList);

        Debug.Log($"[BattleViewModel] {unit.ID} ExecuteBattleAction 반환값: {isExecuted}");

        if (isExecuted == false)
        {
            executor.BattleActionCreated -= OnActionCreated;
            return null;
        }

        try
        {
            BattleActionModel createdAction = await completionSource.Task.AttachExternalCancellation(token);

            await UniTask.Yield(PlayerLoopTiming.Update, token);

            return createdAction;
        }
        finally
        {
            executor.BattleActionCreated -= OnActionCreated;
        }
    }

    //BattleManager의 실제 액션 큐 상태를 가져와 갱신한다. 큐가 변경되는 시점(라운드 시작, 액션 소비 등)마다 호출되어야 함
    public void RefreshActionQueue()
    {
        ActionQueue = BattleManager.Inst.GetActionQueueSnapshot();

        OnPropertyChanged(nameof(ActionQueue));
    }

    //전투 결과에 따라 기억의파편 보상을 계산해서 실제로 지급하고 로그를 남긴 뒤 지급된 보상 금액을 반환한다
    public int ApplyBattleReward(BattleResult result, int roundCount)
    {
        int rewardAmount = BattleManager.Inst.CalculateReward(result, roundCount);

        if (rewardAmount <= 0)
        {
            return rewardAmount;
        }

        GameManager.Inst.Services.CurrencyService.AddMemoryFragment(rewardAmount);
        AddBattleLog($"기억의 파편 {rewardAmount} 획득!");

        return rewardAmount;
    }

    //전투에 참여한 영웅들의 누적 참여 횟수를 증가시킨다 (교체로 빠진 영웅 포함, 승패 무관, 세이브 반영은 하루 경과 시 일괄 처리 정책을 따름)
    public void UpdateHeroBattleParticipation(List<string> heroIdList)
    {
        PlayerModel player = SaveManager.Inst.CurrentPlayerModel;

        if (player == null)
        {
            return;
        }

        foreach (string heroId in heroIdList)
        {
            HeroProgressModel progress = FindOrCreateHeroProgress(player, heroId);
            progress.BattleParticipateCount++;
        }
    }

    //해당 영웅의 진행 데이터를 찾고, 없으면 새로 만들어 리스트에 추가한다
    private HeroProgressModel FindOrCreateHeroProgress(PlayerModel player, string heroId)
    {
        foreach (HeroProgressModel progress in player.HeroProgressList)
        {
            if (progress.HeroId == heroId)
            {
                return progress;
            }
        }

        HeroProgressModel newProgress = new HeroProgressModel();
        newProgress.HeroId = heroId;
        player.HeroProgressList.Add(newProgress);
        return newProgress;
    }

    //유닛의 행동 결과를 배틀 로그 문구로 변환한다
    private string BuildUnitActionLogMessage(BattleActionModel action)
    {
        string unitName = GameUtil.GetUnitDisplayName(action.Unit.ID);

        if (action.ActionType == ActionType.Wait)
        {
            return $"{unitName} - 대기 예정";
        }

        return $"{unitName} - {action.ActionType} 예정";
    }

    //개입 턴이 끝난 뒤 큐를 순서대로 꺼내며 실제 효과를 적용한다
    private async UniTask ResolveActionQueueAsync(List<BattleUnitModel> heroList, List<BattleUnitModel> enemyList, CancellationToken token)
    {
        while (BattleManager.Inst.HasNextAction())
        {
            token.ThrowIfCancellationRequested();
            
            BattleActionModel action = BattleManager.Inst.GetNextAction();
            RefreshActionQueue();

            if (action.IsPlayerAction)
            {
                continue;
            }

            //행동 실행 전에 유닛이 전투 불능이면 예약된 행동을 취소
            if (action.Unit == null || action.Unit.IsDefeated)
            {
                if (action.Unit != null)
                {
                    string unitName = GameUtil.GetUnitDisplayName(action.Unit.ID);
                    AddBattleLog($"{unitName} - 전투 불능으로 행동 취소");
                }

                continue;
            }

            await ResolveActionAsync(action, heroList, enemyList, token);
        }
    }

    //이번 라운드 시작으로 페널티가 자연 해제될 유닛이 있으면 실제 해제(BuildActionQueue) 전에 미리 배틀로그를 남긴다
    private void LogPenaltyReleases(List<BattleUnitModel> turnOrder)
    {
        foreach (BattleUnitModel unit in turnOrder)
        {
            if (string.IsNullOrEmpty(unit.ActivePenaltyId))
            {
                continue;
            }

            if (unit.PenaltyRemainingRounds > 1)
            {
                continue;
            }

            Penalty penalty = GameDataManager.Inst.GetData<Penalty>(unit.ActivePenaltyId);

            if (penalty == null)
            {
                continue;
            }

            string unitName = GameUtil.GetUnitDisplayName(unit.ID);
            AddBattleLog($"{unitName} - {penalty.PenaltyName} 자연 해제");
        }
    }

    //액션 하나를 개입 결과에 따라 처리한다
    private async UniTask ResolveActionAsync(BattleActionModel action, List<BattleUnitModel> heroList, List<BattleUnitModel> enemyList, CancellationToken token)
    {
        string unitName = GameUtil.GetUnitDisplayName(action.Unit.ID);

        if (action.Result == BattleActionResult.HealUnit)
        {
            ApplyHealUnit(action.Unit, action.SelectedItemId);
            ConsumeInventoryItem(action.SelectedItemId);
            AddBattleLog($"{unitName} - 회복 완료 (현재 HP {action.Unit.CurrentHp}/{action.Unit.MaxHp})");
            return;
        }

        if (action.Result == BattleActionResult.Reinforce)
        {
            await ResolveReinforceActionAsync(
                action, 
                heroList, 
                enemyList,
                token);

            return;
        }

        if (action.Result == BattleActionResult.ChangeUnit)
        {
            if (action.Unit.IsHero == false)
            {
                AddBattleLog($"{unitName} - 적으로는 교체할 수 없습니다.");
                return;
            }

            ReserveHeroChange(action.Unit, action.SelectedChangeHeroId);
            AddBattleLog($"{unitName} - 교체 예약 완료 (이번 라운드 종료 후 반영)");
            return;
        }

        bool isDamageApplied = await ApplyActionDamageAsync(
            action, 
            heroList, 
            enemyList,
            token);

        if (action.ActionType == ActionType.Attack && isDamageApplied == false)
        {
            AddBattleLog($"{unitName} - 공격 가능한 대상이 없어 행동 불발");
            return;
        }

        if (action.ActionType != ActionType.Wait)
        {
            Penalty triggeredPenalty = BattleManager.Inst.UpdatePenaltyGauge(action.Unit, action.SkillId);

            if (triggeredPenalty != null)
            {
                AddBattleLog($"{unitName} - {triggeredPenalty.PenaltyName} 발동");
            }
        }

        AddBattleLog(BuildActionResolvedLogMessage(action));
    }

    //라운드 종료 후 일괄 반영하기 위해 교체 내용을 예약해둔다
    private void ReserveHeroChange(BattleUnitModel outUnit, string changeInHeroId)
    {
        _pendingChangeOutUnit = outUnit;
        _pendingChangeInHeroId = changeInHeroId;
    }

    private void ClearPendingHeroChange()
    {
        _pendingChangeOutUnit = null;
        _pendingChangeInHeroId = null;
    }

    //예약된 교체를 실제 참가자 목록에 반영한다. 라운드가 완전히 끝난 뒤에만 호출해야 한다
    private void ApplyPendingHeroChange(List<BattleUnitModel> turnOrder, List<BattleUnitModel> heroList)
    {
        if (_pendingChangeOutUnit == null || string.IsNullOrEmpty(_pendingChangeInHeroId))
        {
            return;
        }

        BattleUnitModel newHeroUnit = CreateHeroBattleUnit(_pendingChangeInHeroId);

        if (newHeroUnit == null)
        {
            AddBattleLog("영웅 교체 실패: 투입할 영웅의 전투 데이터를 찾을 수 없습니다.");
            ClearPendingHeroChange();
            return;
        }

        string outHeroName = GameUtil.GetUnitDisplayName(_pendingChangeOutUnit.ID);
        string inHeroName = GameUtil.GetUnitDisplayName(newHeroUnit.ID);

        turnOrder.Remove(_pendingChangeOutUnit);
        heroList.Remove(_pendingChangeOutUnit);

        turnOrder.Add(newHeroUnit);
        heroList.Add(newHeroUnit);

        SortTurnOrderInPlace(turnOrder);

        ClearPendingHeroChange();

        AddBattleLog($"{outHeroName} 영웅이 물러나고 {inHeroName} 영웅이 투입되었습니다.");

        HeroListChanged?.Invoke(heroList);
    }

    //TurnManager가 내부 리스트 하나를 재사용하는 구조라, 복사본을 거쳐야 원본이 비워지지 않는다
    private void SortTurnOrderInPlace(List<BattleUnitModel> turnOrder)
    {
        List<BattleUnitModel> sourceCopy = new List<BattleUnitModel>(turnOrder);
        List<BattleUnitModel> sortedCopy = new List<BattleUnitModel>(TurnManager.Inst.GetTurnOrder(sourceCopy));

        turnOrder.Clear();
        turnOrder.AddRange(sortedCopy);
    }

    //지원하기 개입 처리 - 페널티로 막혔던 원래 스킬을 되살려서 실제로 적용한다
    private async UniTask ResolveReinforceActionAsync(BattleActionModel action, List<BattleUnitModel> heroList, List<BattleUnitModel> enemyList, CancellationToken token)
    {
        BattleUnitModel unit = action.Unit;
        string unitName = GameUtil.GetUnitDisplayName(unit.ID);

        if (string.IsNullOrEmpty(unit.ActivePenaltyId))
        {
            AddBattleLog($"{unitName} - 지원하기 실패 (페널티에 걸려있지 않습니다.)");
            return;
        }

        Penalty penalty = GameDataManager.Inst.GetData<Penalty>(unit.ActivePenaltyId);

        if (penalty == null)
        {
            Debug.LogWarning($"[BattleViewModel] {unitName} 지원하기 실패, 페널티 ID는 있는데 데이터 테이블에서 못 찾음 (penaltyId: {unit.ActivePenaltyId})"); //콘솔로그
            AddBattleLog($"{unitName} - 지원하기 효과 없음"); //배틀로그
            return;
        }

        BattleManager.Inst.RemovePenalty(unit);
        ConsumeInventoryItem(action.SelectedItemId);

        BattleActionModel revivedAction;

        bool isCreated = BattleActionFactory.TryCreateSkillActionFromId(
            unit,
            penalty.TriggerSkillId,
            heroList,
            enemyList,
            out revivedAction);

        if (isCreated == false)
        {
            Debug.LogWarning($"[BattleViewModel] {unitName} 지원하기 스킬 재구성 실패 (skillId: {penalty.TriggerSkillId})"); //콘솔로그
            AddBattleLog($"{unitName} - 페널티는 해제되었으나 행동은 불발되었습니다."); //배틀로그
            return;
        }

        bool isDamageApplied = await ApplyActionDamageAsync(
            revivedAction, 
            heroList, 
            enemyList,
            token);

        if (revivedAction.ActionType == ActionType.Attack && isDamageApplied == false)
        {
            AddBattleLog($"{unitName} - 페널티는 해제되었으나 공격 가능한 대상이 없습니다.");
            return;
        }

        SupportItem usedItem = GameDataManager.Inst.GetData<SupportItem>(action.SelectedItemId);
        string itemName = usedItem != null ? usedItem.ItemName : "지원 아이템";

        AddBattleLog($"{unitName} - '{itemName}' 사용해 '{penalty.PenaltyName}' 페널티 해제 성공, {revivedAction.ActionType} 성공");
    }

    //액션이 실제로 처리된 결과를 배틀 로그 문구로 변환한다
    private string BuildActionResolvedLogMessage(BattleActionModel action)
    {
        string unitName = GameUtil.GetUnitDisplayName(action.Unit.ID);

        if (action.ActionType == ActionType.Wait)
        {
            return $"{unitName} - 대기";
        }

        return $"{unitName} - {action.ActionType} 실행";
    }

    //액션 결과를 대상(들)의 HP에 실제로 반영한다. 공격 타입 스킬에만 적용
    private async UniTask<bool> ApplyActionDamageAsync(
        BattleActionModel action,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        CancellationToken token)
    {
        if (action.ActionType != ActionType.Attack)
        {
            return false;
        }

        int power = GetSkillPower(action.Unit, action.SkillId);

        if (action.TargetType == TargetType.Single)
        {
            bool hasValidTarget = TryRetargetSingleEnemy(action, heroList, enemyList);

            if (hasValidTarget == false)
            {
                return false;
            }

            UnitAttackStarted?.Invoke(action.Unit);

            await UniTask.Delay(
                AttackAnimationDelayMilliseconds,
                cancellationToken: token);
                
            ApplyDamageToUnit(action.Target, power);

            await UniTask.Delay(
                HitAnimationDelayMilliseconds,
                cancellationToken: token);

            return true;
        }

        if (action.TargetType == TargetType.Multi)
        {
            bool hasValidTarget = TryRetargetMultiEnemy(action, heroList, enemyList);

            if (hasValidTarget == false)
            {
                return false;
            }

            UnitAttackStarted?.Invoke(action.Unit);

            await UniTask.Delay(
                AttackAnimationDelayMilliseconds,
                cancellationToken: token);

            foreach (BattleUnitModel target in action.TargetList)
            {
                ApplyDamageToUnit(target, power);
            }

            await UniTask.Delay(
                HitAnimationDelayMilliseconds,
                cancellationToken: token);

            return true;
        }

        return false;
    }

    private bool TryRetargetSingleEnemy(
        BattleActionModel action,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList)
    {
        if (action.Target != null && action.Target.IsDefeated == false)
        {
            return true;
        }

        bool isEnemyTarget =
            action.TargetSelectType == TargetSelectType.RandomEnemy ||
            action.TargetSelectType == TargetSelectType.LowestHpEnemy;

        if (isEnemyTarget == false)
        {
            return false;
        }

        List<BattleUnitModel> selectedTargets = BattleTargetSelector.SelectTargets(
            action.Unit,
            heroList,
            enemyList,
            action.TargetSelectType,
            action.TargetCount);

        if (selectedTargets == null || selectedTargets.Count == 0)
        {
            return false;
        }

        action.Target = selectedTargets[0];
        return action.Target != null && action.Target.IsDefeated == false;
    }

    //Multi 타겟 액션에서 이미 사망한 대상을 걸러내고, 부족한 수만큼 살아있는 새 대상으로 채운다
    private bool TryRetargetMultiEnemy(
        BattleActionModel action,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList)
    {
        RemoveDefeatedTargets(action.TargetList);

        int missingCount = action.TargetCount - action.TargetList.Count;

        if (action.TargetCount <= 0 || missingCount <= 0)
        {
            return action.TargetList.Count > 0;
        }

        bool isEnemyTarget =
            action.TargetSelectType == TargetSelectType.RandomEnemy ||
            action.TargetSelectType == TargetSelectType.LowestHpEnemy;

        if (isEnemyTarget == false)
        {
            return action.TargetList.Count > 0;
        }

        List<BattleUnitModel> remainingHeroList = BuildExcludedUnitList(heroList, action.TargetList);
        List<BattleUnitModel> remainingEnemyList = BuildExcludedUnitList(enemyList, action.TargetList);

        List<BattleUnitModel> replacementTargets = BattleTargetSelector.SelectTargets(
            action.Unit,
            remainingHeroList,
            remainingEnemyList,
            action.TargetSelectType,
            missingCount);

        if (replacementTargets != null)
        {
            action.TargetList.AddRange(replacementTargets);
        }

        return action.TargetList.Count > 0;
    }

    //대상 목록에서 이미 전투불능인 유닛을 제거한다
    private void RemoveDefeatedTargets(List<BattleUnitModel> targetList)
    {
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (targetList[i] == null || targetList[i].IsDefeated)
            {
                targetList.RemoveAt(i);
            }
        }
    }

    //원본 목록에서 excludeList에 포함된 유닛을 제외한 새 목록을 만든다
    private List<BattleUnitModel> BuildExcludedUnitList(List<BattleUnitModel> sourceList, List<BattleUnitModel> excludeList)
    {
        List<BattleUnitModel> resultList = new List<BattleUnitModel>();

        foreach (BattleUnitModel unit in sourceList)
        {
            if (excludeList.Contains(unit))
            {
                continue;
            }

            resultList.Add(unit);
        }

        return resultList;
    }

    private void ApplyDamageToUnit(BattleUnitModel target, int power)
    {
        if (target == null)
        {
            return;
        }

        target.CurrentHp -= power;

        if (target.CurrentHp < 0)
        {
            target.CurrentHp = 0;
        }
        
        UnitHpChanged?.Invoke(target);
        UnitHitVfxRequested?.Invoke(target);

        if (target.CurrentHp <= 0)
        {
            UnitDied?.Invoke(target);
        }
        else
        {
            UnitHit?.Invoke(target);
        }

        Debug.Log($"[BattleViewModel] {target.ID} 피격, 데미지 {power}, 남은 HP {target.CurrentHp}");
    }

    //대상 유닛의 HP를 회복시킨다. MaxHp를 넘지 않도록 제한
    private void ApplyHealUnit(BattleUnitModel unit, string itemId)
    {
        SupportItem item = GameDataManager.Inst.GetData<SupportItem>(itemId);

        if (item == null)
        {
            Debug.LogWarning($"[BattleViewModel] 회복 아이템 데이터를 찾을 수 없음 (itemId: {itemId})");
            return;
        }

        unit.CurrentHp += item.HealAmount;

        if (unit.CurrentHp > unit.MaxHp)
        {
            unit.CurrentHp = unit.MaxHp;
        }

        UnitHpChanged?.Invoke(unit);
    }

    //인벤토리에서 해당 아이템을 1개 소모한다
    private void ConsumeInventoryItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return;
        }

        List<ItemModel> inventory = SaveManager.Inst.CurrentPlayerModel.Inventory;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].ItemID == itemId)
            {
                inventory[i].ItemCount--;

                if (inventory[i].ItemCount <= 0)
                {
                    inventory.RemoveAt(i);
                }

                return;
            }
        }
    }

    //유닛 진영에 맞는 스킬 데이터에서 Power 값을 가져온다
    private int GetSkillPower(BattleUnitModel unit, string skillId)
    {
        if (unit == null || string.IsNullOrEmpty(skillId))
        {
            return 0;
        }

        if (unit.IsHero)
        {
            HeroSkill heroSkill = GameDataManager.Inst.GetData<HeroSkill>(skillId);
            return heroSkill != null ? heroSkill.Power : 0;
        }

        EnemySkill enemySkill = GameDataManager.Inst.GetData<EnemySkill>(skillId);
        return enemySkill != null ? enemySkill.Power : 0;
    }
}

