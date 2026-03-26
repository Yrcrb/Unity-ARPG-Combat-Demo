using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyState
{
    idle,
    isRun,
    isAttack,
    isHit,
    interim //过渡状态
}

public class EnemysStateManager : MonoBehaviour
{
    public EnemyState currentState;
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
        currentState = EnemyState.idle;
    }

    public void ChangeState(EnemyState newState)
    {
        ExitCurrentState();
        currentState = newState;
        EnterNewState();
    }

    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.isRun:
                animator.SetBool("isRun", false);
                break;
            case EnemyState.isAttack:
                //animator.SetBool("isAttack" , false);
                break;
            case EnemyState.interim:
                animator.SetBool("interim", false);
                break;
        }
    }

    private void EnterNewState()
    {
        switch (currentState)
        {
            case EnemyState.idle:
                break;
            case EnemyState.isRun:
                animator.SetBool("isRun", true);
                break;
            case EnemyState.isAttack:
                animator.SetTrigger("isAttack");
                break;
            case EnemyState.isHit:
                animator.SetTrigger("isHit");  
                break;
            case EnemyState.interim:
                animator.SetBool("interim", true);
                break;
        }
    }
}
