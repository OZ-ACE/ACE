using UnityEngine;

// 전투 공간 프리팹 내부 참조를 모아두는 컴포넌트. ObjectManager가 꺼내서 BattleManager에 넘긴다
public class BattleRootRefs : MonoBehaviour
{
    [SerializeField] private BattleBTExecutor Executor_Hero;
    [SerializeField] private BattleBTExecutor Executor_Enemy;

    public BattleBTExecutor HeroExecutor { get { return Executor_Hero; } }
    public BattleBTExecutor EnemyExecutor { get { return Executor_Enemy; } }
}