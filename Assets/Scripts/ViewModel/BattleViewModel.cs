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

    public List<BattleUnitModel> GetBattleTurnOrder(List<string> heroIds, List<string> enemyIds)
    {
        List<BattleUnitModel> participats = new List<BattleUnitModel>();

        foreach (string heroId in heroIds)
        {
            HeroBattleData data = GameDataManager.Inst.GetData<HeroBattleData>(heroId);

            if (data == null)
            {
                continue;
            }

            BattleUnitModel unit = new BattleUnitModel();
            unit.ID = data.ID;
            unit.IsHero = true;
            unit.Speed = data.Speed;
            unit.AttackPower = data.AttackPower;

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

            participats.Add(unit);
        }

        return TurnManager.Inst.GetTurnOrder(participats);
    }

    //전투 로그 추가
    public void AddBattleLog(string message)
    {
        BattleLogs.Add(message);

        OnPropertyChanged(nameof(BattleLogs));
    }

    //턴 순서대로 유닛을 하나씩 BT에 넘기고, 결과가 올 때까지 기다렸다가 다음 유닛으로 진행한다
    public async UniTask RunRoundAsync(
        List<BattleUnitModel> turnOrder,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        CancellationToken token)
    {
        BattleManager.Inst.BuildActionQueue(turnOrder);

        foreach (BattleUnitModel unit in turnOrder)
        {
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

            BattleManager.Inst.SetUnitAction(createdAction);
            AddBattleLog(BuildUnitActionLogMessage(createdAction));
        }

        RefreshActionQueue();
    }

    //한 유닛의 BT 실행을 요청하고, BattleActionCreated 이벤트가 울릴 때까지 기다린다
    private async UniTask<BattleActionModel> RequestUnitActionAsync(
        BattleBTExecutor executor,
        BattleUnitModel unit,
        List<BattleUnitModel> heroList,
        List<BattleUnitModel> enemyList,
        CancellationToken token)
    {
        UniTaskCompletionSource<BattleActionModel> completionSource = new UniTaskCompletionSource<BattleActionModel>();

        void OnActionCreated(BattleActionModel action)
        {
            completionSource.TrySetResult(action);
        }

        executor.BattleActionCreated += OnActionCreated;

        string firstSkillId = GetFirstSkillId(unit);
        bool isExecuted = executor.ExecuteBattleAction(unit, heroList, enemyList, firstSkillId);

        if (isExecuted == false)
        {
            executor.BattleActionCreated -= OnActionCreated;
            return null;
        }

        try
        {
            return await completionSource.Task.AttachExternalCancellation(token);
        }
        finally
        {
            executor.BattleActionCreated -= OnActionCreated;
        }
    }

    //유닛이 가진 스킬 목록 중 첫 번째를 가져온다. 민건님 BT 쪽 로직과 동일한 선택 기준
    private string GetFirstSkillId(BattleUnitModel unit)
    {
        if (unit.SkillIdList == null || unit.SkillIdList.Count == 0)
        {
            return string.Empty;
        }

        return unit.SkillIdList[0];
    }

    //BattleManager의 실제 액션 큐 상태를 가져와 갱신한다. 큐가 변경되는 시점(라운드 시작, 액션 소비 등)마다 호출되어야 함
    public void RefreshActionQueue()
    {
        ActionQueue = BattleManager.Inst.GetActionQueueSnapshot();

        OnPropertyChanged(nameof(ActionQueue));
    }

    //전투 결과에 따라 기억의파편 보상을 계산해서 실제로 지급하고, 로그를 남긴다
    public void ApplyBattleReward(BattleResult result, int roundCount)
    {
        int rewardAmount = BattleManager.Inst.CalculateReward(result, roundCount);

        if (rewardAmount <= 0)
        {
            return;
        }

        GameManager.Inst.Services.CurrencyService.AddMemoryFragment(rewardAmount);
        AddBattleLog($"기억의파편 {rewardAmount} 획득");
    }

    //유닛의 행동 결과를 배틀 로그 문구로 변환한다
    private string BuildUnitActionLogMessage(BattleActionModel action)
    {
        if (action.ActionType == ActionType.Wait)
        {
            return $"{action.Unit.ID} - 대기";
        }

        return $"{action.Unit.ID} - {action.ActionType} 실행";
    }
}
