using UnityEngine;

public enum State
{
    idle,
    isRun,
    run_end,
    isAttack,
    exAttack,
    evade,
    isHit_Front,
    isHit_Back,
    interim
}

public class PlayerStatesManager : MonoBehaviour
{
    [Header("动画配置（SO 资产）")]
    public AnimationConfigSO animConfig;
    public string entityId = "Player";

    [Header("攻击设置")]
    public GameObject Weapon;
    private BoxCollider weaponCollider;
    public int attackCounter = 0;
    public bool isAttack;
    public State currentState;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        weaponCollider = Weapon.GetComponent<BoxCollider>();
        currentState = State.idle;
    }

    public void ChangeState(State newState)
    {
        ExitStateLogic(currentState);
        currentState = newState;
        EnterStateLogic(currentState);

        int comboIndex = newState == State.isAttack ? Mathf.Max(0, attackCounter - 1) : -1;
        PlayAnim(currentState.ToString(), comboIndex);
    }

    private void PlayAnim(string stateName, int comboIndex = -1)
    {
        if (animator == null || animConfig == null) return;

        var entity = animConfig.GetEntityConfig(entityId);
        var state = entity?.GetStateConfig(stateName);

        string animName;
        float duration;

        if (state != null)
        {
            animName = state.animationState;
            if (comboIndex >= 0 && state.comboAnimations != null && comboIndex < state.comboAnimations.Length)
            {
                string comboAnim = state.comboAnimations[comboIndex];
                if (!string.IsNullOrEmpty(comboAnim))
                    animName = comboAnim;
            }
            duration = state.transitionDuration >= 0f ? state.transitionDuration : animConfig.globalDefaultDuration;
            animator.speed = state.speedMultiplier;
        }
        else
        {
            animName = stateName;
            duration = animConfig.globalDefaultDuration;
        }

        if (!animator.HasState(0, Animator.StringToHash(animName)))
            Debug.LogWarning($"[PlayerStates] 找不到 '{animName}' (SO state='{stateName}')", this);
        animator.CrossFade(animName, duration);
    }

    private void ExitStateLogic(State state)
    {
        switch (state)
        {
            case State.isAttack:
            case State.exAttack:
                isAttack = false;
                if (weaponCollider != null) weaponCollider.enabled = false;
                break;
        }
    }

    private void EnterStateLogic(State state)
    {
        switch (state)
        {
            case State.isAttack:
            case State.exAttack:
                isAttack = true;
                if (weaponCollider != null) weaponCollider.enabled = true;
                break;
        }
    }
}
