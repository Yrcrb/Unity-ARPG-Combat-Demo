using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public enum State
{
    //idle_akf,
    //walk,
    idle,
    isRun,
    isAttack,
    exAttack,
    evade,
    isHit_Front,
    isHit_Back,
    interim //过渡状态
}

public class PlayerStatesManager : MonoBehaviour
{
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
        ExitCurrentState();
        currentState = newState;
        EnterNewState();
    }

    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case State.isRun:
                animator.SetBool("isRun", false);
                break;
            case State.isAttack:
                isAttack = false;
                weaponCollider.enabled = false;
                break;
            case State.exAttack:
                isAttack = false;
                weaponCollider.enabled = false;
                break;
        }
    }

    private void EnterNewState()
    {
        switch (currentState)
        {
            case State.idle:
                break;
            case State.isRun:
                animator.SetBool("isRun", true);
                break;
            case State.isAttack:
                isAttack = true;
                weaponCollider.enabled = true;
                animator.SetInteger("AttackCounter", attackCounter);
                animator.SetTrigger("isAttack");
                break;
            case State.exAttack:
                isAttack = true;
                weaponCollider.enabled = true;
                animator.SetTrigger("exAttack");
                break;
            case State.evade:
                animator.SetTrigger("isEvade");
                break;
            case State.isHit_Front:
                animator.SetTrigger("isHit_Front");
                break;
            case State.isHit_Back:
                animator.SetTrigger("isHit_Back");
                break;
            case State.interim:
                break;
        }
    }
    
}
