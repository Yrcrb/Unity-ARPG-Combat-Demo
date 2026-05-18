using UnityEngine;

public class BulidTree : MonoBehaviour
{
    // 当前敌人的控制器，行为树所有条件和动作都通过它访问。
    private EnemyController enemyController;

    // 行为树根节点，负责驱动整棵树的 Tick。
    private BasicsNode rootNode;

    // 缓存敌人控制器，供后续搭树时绑定条件和动作。
    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    // 在运行开始时搭建完整行为树。
    private void Start()
    {
        Build();
    }

    // 每帧先刷新黑板，再让行为树读取最新数据。
    private void LateUpdate()
    {
        enemyController.RefreshBlackboard();
        rootNode?.Tick();
    }

    // 组件关闭时重置整棵树，避免动作残留。
    private void OnDisable()
    {
        rootNode?.Reset();
    }

    // 按优先级组装敌人的完整行为树结构。
    public void Build()
    {
        // 根节点负责在多个大分支里按优先级挑选当前行为。
        SelectorNode rootSelector = new SelectorNode();

        // 受击分支优先级最高，用来打断其他行为。
        SequenceNode hitSequence = new SequenceNode();

        // 战斗分支按 攻击 -> 过渡 -> 警戒 -> 追击 的顺序执行。
        SelectorNode combatSelector = new SelectorNode();
        SequenceNode attackSequence = new SequenceNode();
        SequenceNode interimSequence = new SequenceNode();
        SequenceNode vigilantSequence = new SequenceNode();
        // 非战斗分支只在战斗目标失效时接管行为。
        SelectorNode nonCombatSelector = new SelectorNode();

        // 受击逻辑只要检测到待处理的受击，就立刻抢占执行。
        hitSequence.AddChild(new Condition(enemyController.HasPendingHitReaction));
        hitSequence.AddChild(new ActionNode(
            enemyController.EnterHitReaction,
            enemyController.TickHitReaction,
            enemyController.ExitHitReaction));

        // 攻击逻辑要求目标贴身、冷却结束且当前没有动作锁。
        attackSequence.AddChild(new Condition(enemyController.HasTarget));
        attackSequence.AddChild(new Condition(enemyController.InAttackRange));
        attackSequence.AddChild(new Condition(enemyController.CanStartAttack));
        attackSequence.AddChild(new ActionNode(
            enemyController.EnterAttack,
            enemyController.TickAttack,
            enemyController.ExitAttack));

        // interim 逻辑只在攻击尾帧登记了过渡请求后才会进入。
        interimSequence.AddChild(new Condition(enemyController.HasPendingInterim));
        interimSequence.AddChild(new ActionNode(
            enemyController.EnterInterim,
            enemyController.TickInterim,
            enemyController.ExitInterim));

        // 警戒逻辑只在攻击冷却中执行，用来播放左右平移动画。
        vigilantSequence.AddChild(new Condition(enemyController.CanEnterVigilant));
        vigilantSequence.AddChild(new ActionNode(
            enemyController.EnterVigilant,
            enemyController.TickVigilant,
            enemyController.ExitVigilant));

        // 追击逻辑：每帧先检查条件，条件通过才执行追击动作。
        SequenceNode chaseCondition = new SequenceNode();
        chaseCondition.AddChild(new Condition(enemyController.HasTarget));
        chaseCondition.AddChild(new Condition(enemyController.InChaseRange));
        chaseCondition.AddChild(new Condition(enemyController.NotInAttackRange));
        PreConditionNode chaseNode = new PreConditionNode(
            chaseCondition,
            new ActionNode(
                enemyController.EnterChase,
                enemyController.TickChase,
                enemyController.ExitChase));

        // 巡逻逻辑：每帧先检查非战斗巡逻阶段，通过才执行巡逻动作。
        PreConditionNode patrolNode = new PreConditionNode(
            new Condition(enemyController.IsPatrolPhase),
            new ActionNode(
                enemyController.EnterPatrol,
                enemyController.TickPatrol,
                enemyController.ExitPatrol));

        // 待机逻辑：每帧先检查非战斗待机阶段，通过才执行待机动作。
        PreConditionNode idleNode = new PreConditionNode(
            new Condition(enemyController.IsIdlePhase),
            new ActionNode(
                enemyController.EnterIdle,
                enemyController.TickIdle,
                enemyController.ExitIdle));

        combatSelector.AddChild(attackSequence);
        combatSelector.AddChild(interimSequence);
        combatSelector.AddChild(vigilantSequence);
        combatSelector.AddChild(chaseNode);

        nonCombatSelector.AddChild(patrolNode);
        nonCombatSelector.AddChild(idleNode);

        rootSelector.AddChild(hitSequence);
        rootSelector.AddChild(combatSelector);
        rootSelector.AddChild(nonCombatSelector);

        rootNode = rootSelector;
    }
}
