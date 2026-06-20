using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    /// <summary>
    /// 非战斗阶段只保留待机和巡逻两个大阶段。
    /// </summary>
    private enum NonCombatPhase
    {
        Idle,
        Patrol
    }

    #region Components
    // 动画状态机组件，专门负责切换动画参数。
    private EnemysStateManager enemysStateManager;

    // NavMeshAgent 负责敌人的全部位移。
    private NavMeshAgent agent;

    // 敌人血条图片，用来显示生命值变化。
    public Image bloodImage;

    // 受击事件资源，用来广播伤害数字和受击特效。
    #endregion

    #region Health Settings
    // 敌人的最大生命值。
    public float blood = 100f;

    // 敌人的当前生命值。
    public float currentBlood;

    // 受击后无敌时间的时长。
    public float maxGoodTime = 0.3f;
    #endregion

    #region Combat Settings
    // 攻击结束后到下一次允许攻击之间的冷却时长。
    [FormerlySerializedAs("waitTime")]
    [SerializeField] private float attackCooldownDuration = 5f;

    // 攻击动画兜底时长，防止动画事件漏掉时卡在攻击状态。
    [SerializeField] private float attackFallbackDuration = 1.5f;

    // 过渡动画兜底时长，防止 interim 动画事件漏掉时卡住。
    [SerializeField] private float interimFallbackDuration = 0.5f;

    // 受击动画兜底时长，防止受击动画事件漏掉时卡住。
    [SerializeField] private float hitFallbackDuration = 0.5f;

    // 攻击判定半径，目标进入该范围后才允许出手。
    public float attackRange = 1.8f;

    // 追击判定半径，目标超过该范围后不再视为战斗目标。
    [SerializeField] private float chaseRange = 9f;
    #endregion

    #region Movement Settings
    // 角色朝向目标时的转身速度。
    public float rotationSpeed = 8f;

    // NavMeshAgent 的移动速度。
    public float speed = 3.5f;

    // 警戒和平移时使用的较慢速度。
    [SerializeField] private float vigilantSpeed = 2f;

    // 巡逻时使用的较慢速度。
    [SerializeField] private float patrolSpeed = 2f;

    // 追击时靠近目标后停止的距离。
    [SerializeField] private float combatStopDistance = 1.25f;

    // 巡逻时可随机采样的位置半径。
    [SerializeField] private float patrolRadius = 4f;

    // 待机状态持续的时长，结束后切去巡逻。
    [SerializeField] private float idleDuration = 2f;

    // 警戒状态左右平移的目标距离。
    [SerializeField] private float vigilantMoveDistance = 1.5f;

    // 警戒移动相对敌人朝向目标方向的单侧最大夹角。
    [SerializeField] private float vigilantAngleRange = 60f;

    // 警戒采样允许的最小有效位移距离。
    [SerializeField] private float minVigilantMoveDistance = 0.5f;
    #endregion

    #region Hitbox
    // 敌人的攻击碰撞体对象，只在出手帧打开。
    public GameObject attackDetection;
    #endregion

    #region Debug Blackboard
    // 调试字段，表示当前是否还在攻击冷却中。
    public bool isWait;

    // 调试字段，表示当前是否还在受击无敌时间中。
    public bool isHit;

    // 调试字段，表示当前剩余的受击无敌时间。
    public float currentGoodTime;

    // 调试字段，表示下一次允许攻击的绝对时间。
    [FormerlySerializedAs("nextTime")]
    public float nextAttackTime;

    // 调试字段，表示当前与目标的实时距离。
    public float distance;

    // 当前战斗目标，会被行为树用于所有战斗判定。
    private Transform target;

    // 最近一次命中敌人的攻击者。
    private Transform lastAttacker;

    // 最近一次受击的位置，用来给特效或调试使用。
    private Vector3 lastHitPoint;

    // 最近一次受击的朝向，用来给特效或调试使用。
    private Vector3 lastHitForward;

    // 敌人的出生点，用来作为巡逻中心。
    private Vector3 homePosition;

    // 当前巡逻的目的地。
    private Vector3 patrolPoint;

    // 当前警戒平移的目的地。
    private Vector3 vigilantPoint;

    // 当前警戒是向右平移还是向左平移。
    private bool vigilantMoveRight;
    #endregion

    #region Runtime Flags
    // 是否有待处理的受击动作，高优先级分支会优先消费它。
    private bool hasPendingHitReaction;

    // 是否有待处理的 interim 过渡动作。
    private bool hasPendingInterim;

    // 是否正处于不可切换的硬锁定动作中。
    private bool isActionLocked;

    // 当前攻击动画是否已经走到收尾事件。
    private bool attackFinished;

    // 当前 interim 动画是否已经走到结束事件。
    private bool interimFinished;

    // 当前受击动画是否已经走到结束事件。
    private bool hitFinished;

    // 当前是否已经生成有效的巡逻目标点。
    private bool hasPatrolPoint;

    // 当前是否已经生成有效的警戒目标点。
    private bool hasVigilantPoint;
    #endregion

    #region Runtime Timers
    // 敌人的无敌时间结束点。
    private float invulnerableEndTime;

    // 受击动作的超时结束点。
    private float hitRecoverEndTime;

    // 攻击动作的超时结束点。
    private float attackRecoverEndTime;

    // interim 动作的超时结束点。
    private float interimRecoverEndTime;

    // 当前非战斗阶段的结束点。
    private float phaseEndTime;
    #endregion

    #region Runtime State
    // 当前非战斗阶段，用来驱动 idle 和 patrol 的切换。
    private NonCombatPhase nonCombatPhase;
    #endregion

    #region Properties
    // 当前是否允许再次掉血。
    public bool CanTakeDamage => Time.time >= invulnerableEndTime;
    #endregion

    #region Unity Lifecycle
    // 初始化核心组件和默认黑板数据。
    private void Awake()
    {
        enemysStateManager = GetComponent<EnemysStateManager>();
        agent = GetComponent<NavMeshAgent>();

        currentBlood = blood;
        homePosition = transform.position;
        nonCombatPhase = NonCombatPhase.Idle;
        phaseEndTime = Time.time + idleDuration;

        // 朝向交给脚本，路径交给 NavMesh。
        agent.updateRotation = false;
        agent.speed = speed;
        agent.stoppingDistance = combatStopDistance;

        if (attackDetection != null)
        {
            attackDetection.SetActive(false);
        }
    }

    // 每帧刷新调试值和行为树会读到的黑板数据。
    public void RefreshBlackboard()
    {
        isWait = Time.time < nextAttackTime;
        isHit = Time.time < invulnerableEndTime;
        currentGoodTime = Mathf.Max(0f, invulnerableEndTime - Time.time);

        if (target == null)
        {
            distance = float.MaxValue;
            return;
        }

        distance = Vector3.Distance(transform.position, target.position);
        if (!target.gameObject.activeInHierarchy)
        {
            ClearTarget(target);
            distance = float.MaxValue;
        }
    }
    #endregion

    #region Target Queries
    // 当前是否存在有效的战斗目标。
    public bool HasTarget()
    {
        return target != null && distance <= chaseRange;
    }

    // 当前目标是否已经进入攻击范围。
    public bool InAttackRange()
    {
        return target != null && distance <= attackRange;
    }

    // 当前目标是否处于追击范围内。
    public bool InChaseRange()
    {
        return target != null && distance <= chaseRange;
    }

    // 当前目标是否已经离开攻击范围。
    public bool NotInAttackRange()
    {
        return !InAttackRange();
    }

    // 感知层或受击层通过它设置当前目标。
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            return;
        }

        target = newTarget;
    }

    // 当前目标失效时通过它清空战斗目标并回到非战斗阶段。
    public void ClearTarget(Transform targetToClear)
    {
        if (targetToClear == null || target != targetToClear)
        {
            return;
        }

        target = null;
        hasVigilantPoint = false;
        hasPatrolPoint = false;
        distance = float.MaxValue;
        SwitchToIdlePhase();
    }
    #endregion

    #region Hit Logic
    // 行为树通过它判断是否需要优先播放受击动作。
    public bool HasPendingHitReaction()
    {
        return hasPendingHitReaction;
    }

    // 外部伤害入口，负责扣血并登记一次待处理的受击。
    public void TakeDamage(Transform attacker, float atk, Vector3 hitPoint, Vector3 hitForward)
    {
        if (!CanTakeDamage)
        {
            return;
        }

        invulnerableEndTime = Time.time + maxGoodTime;
        currentBlood -= atk;
        lastAttacker = attacker;
        lastHitPoint = hitPoint;
        lastHitForward = hitForward;
        hasPendingHitReaction = true;

        if (attacker != null)
        {
            SetTarget(attacker);
            RotateTowards(attacker.position);
        }

        if (bloodImage != null)
        {
            bloodImage.fillAmount = Mathf.Clamp(currentBlood / blood, 0f, 1f);
        }

        EventBus.Instance.Invoke(E.OnDamage, atk);
        EventBus.Instance.Invoke(E.HitVFX, hitPoint, hitForward);
    }

    // 受击动作开始时关闭位移和攻击判定，并锁住行为。
    public void EnterHitReaction()
    {
        hasPendingHitReaction = false;
        hitFinished = false;
        isActionLocked = true;
        hitRecoverEndTime = Time.time + hitFallbackDuration;

        StopAgent();
        SetAttackHitbox(false);

        if (lastAttacker != null)
        {
            RotateTowards(lastAttacker.position);
        }

        enemysStateManager.ChangeState(EnemyState.isHit);
    }

    // 受击动作持续期间只负责等待动画或兜底时间结束。
    public NodeState TickHitReaction()
    {
        if (lastAttacker != null)
        {
            RotateTowards(lastAttacker.position);
        }

        if (hitFinished || Time.time >= hitRecoverEndTime)
        {
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // 受击动作结束后解除锁定，交还给行为树下一阶段决策。
    public void ExitHitReaction()
    {
        hitFinished = false;
        isActionLocked = false;
    }

    // 受击动画尾帧可通过动画事件调用它来精确结束受击。
    public void OnHitFinished()
    {
        hitFinished = true;
    }
    #endregion

    #region Attack Queries
    // 当前是否已经满足开始一次攻击的条件。
    public bool CanStartAttack()
    {
        return HasTarget() && InAttackRange() && !isActionLocked && Time.time >= nextAttackTime;
    }

    // 当前是否已经进入攻击后的冷却阶段。
    public bool IsAttackCoolingDown()
    {
        return HasTarget() && Time.time < nextAttackTime;
    }

    // 当前是否已经登记了待处理的 interim 过渡阶段。
    public bool HasPendingInterim()
    {
        return hasPendingInterim;
    }

    // 当前是否允许从 interim 进入真正的警戒移动。
    public bool CanEnterVigilant()
    {
        return HasTarget() && InAttackRange() && Time.time < nextAttackTime && !isActionLocked && !hasPendingInterim;
    }
    #endregion

    #region Attack Logic
    // 攻击开始时停下位移、触发动画，并锁住后续行为。
    public void EnterAttack()
    {
        attackFinished = false;
        hasPendingInterim = false;
        isActionLocked = true;
        attackRecoverEndTime = Time.time + attackFallbackDuration;

        StopAgent();
        SetAttackHitbox(false);
        FaceCurrentTarget();
        enemysStateManager.ChangeState(EnemyState.isAttack);
    }

    // 攻击期间只等待动画事件或兜底时间结束。
    public NodeState TickAttack()
    {
        FaceCurrentTarget();

        if (attackFinished)
        {
            return NodeState.Success;
        }

        if (Time.time >= attackRecoverEndTime)
        {
            QueueInterim();
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // 攻击动作退出时只做清理，不在这里解锁，后续还要进入 interim。
    public void ExitAttack()
    {
        attackFinished = false;
        SetAttackHitbox(false);
    }

    // 攻击动画出手帧会调用它来打开攻击碰撞体。
    public void AttackDetection()
    {
        SetAttackHitbox(true);
    }

    // 攻击动画收手帧会调用它来关闭攻击碰撞体。
    public void StopAttackDetection()
    {
        SetAttackHitbox(false);
    }

    // 攻击结束并转入 interim
    public void OnAttackFinished()
    {
        QueueInterim();
    }
    #endregion

    #region Interim Logic
    // interim 开始时切到真实的过渡动画，并从这里启动攻击冷却。
    public void EnterInterim()
    {
        hasPendingInterim = false;
        interimFinished = false;
        nextAttackTime = Time.time + attackCooldownDuration;
        interimRecoverEndTime = Time.time + interimFallbackDuration;

        StopAgent();
        FaceCurrentTarget();
        enemysStateManager.ChangeState(EnemyState.interim);
    }

    // interim 期间只负责等待过渡动画结束，不允许 NavMesh 移动。
    public NodeState TickInterim()
    {
        FaceCurrentTarget();

        if (interimFinished || Time.time >= interimRecoverEndTime)
        {
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // interim 结束后解除动作锁，允许后续进入警戒或追击。
    public void ExitInterim()
    {
        interimFinished = false;
        isActionLocked = false;
    }

    // interim 动画尾帧可通过动画事件调用它来精确结束过渡。
    public void OnInterimFinished()
    {
        interimFinished = true;
    }
    #endregion

    #region Vigilant Logic
    // 警戒动作开始时决定左右动画并生成一次侧移目标点。
    public void EnterVigilant()
    {
        vigilantMoveRight = Random.value > 0.5f;
        hasVigilantPoint = TryBuildVigilantPoint(vigilantMoveRight, out vigilantPoint);
        agent.stoppingDistance = 0f;
        agent.speed = vigilantSpeed;
        enemysStateManager.ChangeState(vigilantMoveRight ? EnemyState.isVigilant_right : EnemyState.isVigilant_left);
    }

    // 警戒期间优先把当前侧移走完，再根据距离决定是否切回追击。
    public NodeState TickVigilant()
    {
        // 目标彻底失效时才立刻打断警戒。
        if (!HasTarget())
        {
            return NodeState.Failure;
        }

        //FaceCurrentTarget();

        if (hasVigilantPoint)
        {
            MoveTo(vigilantPoint);
            Debug.DrawLine(transform.position + Vector3.up * 0.2f, vigilantPoint + Vector3.up * 0.2f, Color.cyan);
            if (HasReachedDestination())
            {
                StopAgent();
                hasVigilantPoint = false;
            }
        }
        else
        {
            StopAgent();
            FaceCurrentTarget();

            // 当前这一小段侧移走完后，如果已经离开攻击范围，再交给追击分支接管。
            if (!InAttackRange())
            {
                return NodeState.Failure;
            }
        }

        // 冷却结束后再退出警戒，让上层重新决定下一次攻击。
        if (Time.time >= nextAttackTime)
        {
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // 警戒结束时停掉位移，等待行为树重新决定下一步。
    public void ExitVigilant()
    {
        hasVigilantPoint = false;
        StopAgent();
    }
    #endregion

    #region Chase Logic
    // 追击开始时恢复追击停止距离并切到跑步动画。
    public void EnterChase()
    {
        agent.stoppingDistance = combatStopDistance;
        agent.speed = speed;
        enemysStateManager.ChangeState(EnemyState.isRun);
    }

    // 追击期间持续更新目的地，直到进入攻击范围。
    public NodeState TickChase()
    {
        MoveTo(target.position);
        FaceCurrentTarget();

        if (distance <= attackRange)
        {
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // 追击结束时停掉路径，避免残留移动。
    public void ExitChase()
    {
        StopAgent();
    }
    #endregion

    #region Patrol Logic
    // 当前是否处于巡逻阶段。
    public bool IsPatrolPhase()
    {
        return !HasTarget() && nonCombatPhase == NonCombatPhase.Patrol;
    }

    // 巡逻开始时生成一次巡逻点并切到巡逻动画。
    public void EnterPatrol()
    {
        agent.stoppingDistance = 0f;
        agent.speed = patrolSpeed;
        hasPatrolPoint = TryBuildPatrolPoint(out patrolPoint);

        if (hasPatrolPoint)
        {
            MoveTo(patrolPoint);
        }
        else
        {
            StopAgent();
        }

        enemysStateManager.ChangeState(EnemyState.patrol);
    }

    // 巡逻期间持续前往巡逻点，到达后切回待机阶段。
    public NodeState TickPatrol()
    {
        if (!hasPatrolPoint)
        {
            SwitchToIdlePhase();
            return NodeState.Success;
        }

        MoveTo(patrolPoint);
        RotateTowards(patrolPoint);

        if (HasReachedDestination())
        {
            hasPatrolPoint = false;
            SwitchToIdlePhase();
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // 巡逻退出时清理巡逻点并停掉路径。
    public void ExitPatrol()
    {
        hasPatrolPoint = false;
        StopAgent();
    }
    #endregion

    #region Idle Logic
    // 当前是否处于待机阶段。
    public bool IsIdlePhase()
    {
        return !HasTarget() && nonCombatPhase == NonCombatPhase.Idle;
    }

    // 待机开始时只负责停下并播放 idle。
    public void EnterIdle()
    {
        StopAgent();
        enemysStateManager.ChangeState(EnemyState.idle);
    }

    // 待机期间只等待阶段时间结束，然后切到巡逻阶段。
    public NodeState TickIdle()
    {
        if (Time.time >= phaseEndTime)
        {
            nonCombatPhase = NonCombatPhase.Patrol;
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    // 待机退出时当前没有额外清理逻辑。
    public void ExitIdle()
    {
    }
    #endregion

    #region Navigation Helpers
    // 切回待机阶段时统一重置阶段计时器。
    private void SwitchToIdlePhase()
    {
        nonCombatPhase = NonCombatPhase.Idle;
        phaseEndTime = Time.time + idleDuration;
    }

    // 持续朝当前目标看齐，避免侧移或攻击时朝向错误。
    private void FaceCurrentTarget()
    {
        if (target != null)
        {
            RotateTowards(target.position);
        }
    }

    // 让 NavMeshAgent 前往指定位置。
    private void MoveTo(Vector3 destination)
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        if (!agent.hasPath || Vector3.Distance(agent.destination, destination) > 0.1f)
        {
            agent.SetDestination(destination);
        }
    }

    // 立即停掉 NavMeshAgent 当前路径。
    private void StopAgent()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    // 判断 NavMeshAgent 是否已经到达当前路径终点。
    private bool HasReachedDestination()
    {
        if (agent.pathPending)
        {
            return false;
        }

        // 刚设置目的地时 hasPath 可能暂时还是 false，不能直接当作“已经到达”。
        if (!agent.hasPath)
        {
            return Vector3.Distance(transform.position, agent.destination) <= agent.stoppingDistance + 0.15f;
        }

        return agent.remainingDistance <= agent.stoppingDistance + 0.15f;
    }

    // 让敌人平滑转向世界中的一个位置。
    private void RotateTowards(Vector3 worldPosition)
    {
        Vector3 lookDir = worldPosition - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // 在出生点周围采样一个合法的巡逻点。
    private bool TryBuildPatrolPoint(out Vector3 point)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = homePosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }

        point = transform.position;
        return false;
    }

    // 根据左右方向生成一次警戒目标点。
    private bool TryBuildVigilantPoint(bool moveRight, out Vector3 point)
    {
        if (target == null)
        {
            point = transform.position;
            return false;
        }

        // 先尝试期望方向，失败后自动尝试另一侧。
        if (TryBuildVigilantPointSingleSide(moveRight, out point))
        {
            return true;
        }

        if (TryBuildVigilantPointSingleSide(!moveRight, out point))
        {
            vigilantMoveRight = !moveRight;
            return true;
        }

        point = transform.position;
        return false;
    }

    // 在单侧扇形区域内尝试采样一个真正能走且距离足够的警戒点。
    private bool TryBuildVigilantPointSingleSide(bool moveRight, out Vector3 point)
    {
        if (target == null)
        {
            Debug.Log($"{name} vigilant sample failed: target is null.");
            point = transform.position;
            return false;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0.001f)
        {
            Debug.Log($"{name} vigilant sample failed: target direction is too small.");
            point = transform.position;
            return false;
        }

        // 以敌人朝向目标的方向为中心，在左右各 60 度扇形内选择一个移动方向。
        float signedAngle = moveRight
            ? Random.Range(0f, vigilantAngleRange)
            : Random.Range(-vigilantAngleRange, 0f);

        Vector3 moveDirection = Quaternion.AngleAxis(signedAngle, Vector3.up) * toTarget.normalized;

        // 逐步缩短采样距离，避免一次采样失败就直接回脚下。
        float[] sampleRatios = { 1f, 0.75f, 0.5f };
        for (int i = 0; i < sampleRatios.Length; i++)
        {
            float sampleDistance = vigilantMoveDistance * sampleRatios[i];
            Vector3 candidate = transform.position + moveDirection * sampleDistance;
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                Debug.Log($"{name} vigilant sample failed: no navmesh point. side={(moveRight ? "right" : "left")} distance={sampleDistance:F2}");
                continue;
            }

            Vector3 flatOffset = hit.position - transform.position;
            flatOffset.y = 0f;
            if (flatOffset.magnitude < minVigilantMoveDistance)
            {
                Debug.Log($"{name} vigilant sample failed: point too close. side={(moveRight ? "right" : "left")} offset={flatOffset.magnitude:F2} min={minVigilantMoveDistance:F2}");
                continue;
            }

            // 只有路径完整时才算成功，避免看似采样到了点但实际走不到。
            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.Log($"{name} vigilant sample failed: path incomplete. side={(moveRight ? "right" : "left")} status={path.status}");
                continue;
            }

            Debug.Log($"{name} vigilant sample success: side={(moveRight ? "right" : "left")} point={hit.position}");
            point = hit.position;
            return true;
        }

        Debug.Log($"{name} vigilant sample failed on all attempts. side={(moveRight ? "right" : "left")}");
        point = transform.position;
        return false;
    }

    // 统一管理攻击碰撞体的开关。
    private void SetAttackHitbox(bool active)
    {
        if (attackDetection != null)
        {
            attackDetection.SetActive(active);
        }
    }

    // 把攻击流程切换到真正的 interim 阶段。
    private void QueueInterim()
    {
        SetAttackHitbox(false);
        attackFinished = true;
        hasPendingInterim = true;
    }
    #endregion
}
