using UnityEngine;

public enum EnemyState
{
    idle,
    patrol,
    isRun,
    isAttack,
    isVigilant_left,
    isVigilant_right,
    isHit,
    interim
}

public class EnemysStateManager : MonoBehaviour
{
    [Header("动画配置（拖入 AnimationConfigSO 资产）")]
    public AnimationConfigSO animConfig;
    public string entityId = "StoneMan";

    public EnemyState currentState;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentState = EnemyState.idle;
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState && newState != EnemyState.isAttack && newState != EnemyState.isHit)
            return;

        currentState = newState;
        PlayAnim(currentState.ToString());
    }

    private void PlayAnim(string stateName)
    {
        if (animator == null || animConfig == null) return;

        var entity = animConfig.GetEntityConfig(entityId);
        var state = entity?.GetStateConfig(stateName);

        string animName;
        float duration;

        if (state != null)
        {
            animName = state.animationState;
            duration = state.transitionDuration >= 0f ? state.transitionDuration : animConfig.globalDefaultDuration;
            animator.speed = state.speedMultiplier;
        }
        else
        {
            animName = stateName;
            duration = animConfig.globalDefaultDuration;
        }

        if (!animator.HasState(0, Animator.StringToHash(animName)))
            Debug.LogWarning($"[EnemyStates] 找不到 '{animName}' (SO state='{stateName}')", this);
        animator.CrossFade(animName, duration);
    }
}
