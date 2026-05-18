using UnityEngine;

public enum EnemyState
{
    // 原地待机状态。
    idle,

    // 非战斗巡逻状态。
    patrol,

    // 追击玩家的跑动状态。
    isRun,

    // 正式出手攻击的状态。
    isAttack,

    // 警戒时向左平移的状态。
    isVigilant_left,

    // 警戒时向右平移的状态。
    isVigilant_right,

    // 受击硬直状态。
    isHit,

    // 攻击结束后进入的过渡状态。
    interim
}

public class EnemysStateManager : MonoBehaviour
{
    // 当前已经生效的动画状态。
    public EnemyState currentState;

    // 敌人的 Animator 组件。
    private Animator animator;

    // 初始化 Animator，并让敌人从 idle 状态开始。
    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentState = EnemyState.idle;
    }

    // 统一对外的状态切换入口。
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState && newState != EnemyState.isAttack && newState != EnemyState.isHit)
        {
            return;
        }

        ExitCurrentState();
        currentState = newState;
        EnterNewState();
    }

    // 离开旧状态时负责关闭旧状态对应的 Animator 参数。
    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.patrol:
                animator.SetBool("isPatrol", false);
                break;
            case EnemyState.isRun:
                animator.SetBool("isRun", false);
                break;
            case EnemyState.isVigilant_left:
                animator.SetBool("isVigilant_left", false);
                break;
            case EnemyState.isVigilant_right:
                animator.SetBool("isVigilant_right", false);
                break;
            case EnemyState.interim:
                break;
        }
    }

    // 进入新状态时负责打开新状态对应的 Animator 参数。
    private void EnterNewState()
    {
        switch (currentState)
        {
            case EnemyState.idle:
                break;
            case EnemyState.patrol:
                animator.SetBool("isPatrol", true);
                break;
            case EnemyState.isRun:
                animator.SetBool("isRun", true);
                break;
            case EnemyState.isAttack:
                animator.SetTrigger("isAttack");
                break;
            case EnemyState.isVigilant_left:
                animator.SetBool("isVigilant_left", true);
                break;
            case EnemyState.isVigilant_right:
                animator.SetBool("isVigilant_right", true);
                break;
            case EnemyState.isHit:
                animator.SetTrigger("isHit");
                break;
            case EnemyState.interim:
                animator.SetTrigger("Interim");
                break;
        }
    }
}
