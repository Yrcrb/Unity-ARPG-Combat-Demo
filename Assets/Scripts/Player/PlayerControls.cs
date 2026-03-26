using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    public GameStateManager gameStateManager;
    public OnAttackEvent onAttackEvent;
    private PlayerInput inputActions;
    private CharacterController characterController;
    private Animator animator;
    private PlayerStatesManager playerStatesManager;
    private AttackData attackData;
    private Vector2 moveValue;
    private Vector3 dir;
    private float rotateSpeed;
    private Transform enemyTransform;
    [Header("重力")]
    [SerializeField] private float gravity = 10f;
    public bool isGround = false;
    [Header("状态")]
    public float goodTime;
    public bool isGoodTime;
    private float nextTime;

    private void Awake()
    {
        inputActions = new PlayerInput();
        animator = GetComponent<Animator>();
        playerStatesManager = GetComponent<PlayerStatesManager>();
        characterController = GetComponent<CharacterController>();
        attackData = GetComponent<AttackData>();
        inputActions.Player.Move.performed += ctx => moveValue = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveValue = Vector2.zero;
        inputActions.Player.Attack.performed += ctx => Attack();
        inputActions.Player.Evade.performed += ctx => Evade();
        inputActions.Player.ExAttack.performed += ctx => ExAttack();
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    void LateUpdate()
    {

    }
    private void OnAnimatorMove()
    {
        if (!animator.applyRootMotion) return;
        Vector3 rootMotion = animator.deltaPosition;

        if (playerStatesManager.currentState == State.isAttack && enemyTransform != null)
        {
            float dist = Vector3.Distance(transform.position, enemyTransform.position);
            if (dist < 0.8f) // 安全距离，根据模型大小调节
            {
                // 如果距离太近，抹除掉向前的位移，只允许原地攻击
                rootMotion = Vector3.ProjectOnPlane(rootMotion, transform.forward);
            }
        }

        characterController.Move(rootMotion);
    }
    void Update()
    {
        if (Time.time >= nextTime)
        { 
            isGoodTime = false;
        }
        Move();
        Gravity();
    }
    #region Move
    public void Move()
    {
        if (gameStateManager.currentState != GameState.Player || playerStatesManager.currentState == State.isAttack || playerStatesManager.currentState == State.exAttack) return;
        Vector3 cameraForword = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized;
        dir = (moveValue.y * cameraForword + moveValue.x * cameraRight).normalized;
        if (moveValue != Vector2.zero)
        {
            rotateSpeed = 1 - Mathf.Exp(-20f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir, Vector3.up),
            rotateSpeed);
            playerStatesManager.ChangeState(State.isRun);
        }
        else if (moveValue == Vector2.zero && playerStatesManager.currentState!=State.isAttack && playerStatesManager.currentState != State.exAttack && playerStatesManager.currentState != State.interim)
        {
            playerStatesManager.ChangeState(State.idle);
        }
    }
    #endregion

    #region Evade
    public void Evade()
    {
        if (playerStatesManager.currentState == State.exAttack)return;
        playerStatesManager.ChangeState(State.evade);
    }
    #endregion
    #region Attack
    public void Attack()
    {
        if (gameStateManager.currentState != GameState.Player || playerStatesManager.currentState == State.exAttack) return;
        if (enemyTransform != null && Vector3.Distance(transform.position, enemyTransform.position) < 6f)
        { 
            AttackDir();
        }
        if (playerStatesManager.currentState != State.isAttack)
        {
            playerStatesManager.attackCounter = 1;
        }
        else
        {
            playerStatesManager.attackCounter++;
            if (playerStatesManager.attackCounter > attackData.maxAttackCounter)
            {
                playerStatesManager.attackCounter = 1;
            }
        }
        AttackDamage();
        playerStatesManager.ChangeState(State.isAttack);
    }
    public void EnemyLocate(Transform transform)
    { 
        enemyTransform = transform;
    }
    private void AttackDir()
    {
        if (enemyTransform == null) return;
        float dist = Vector3.Distance(transform.position, enemyTransform.position);
        if (dist < 0.5f) return; 

        Vector3 diff = enemyTransform.position - transform.position;
        diff.y = 0; // 锁定 Y 轴
        if (diff.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(diff, Vector3.up);
        }
    }
    private void AttackDamage()
    {
        int index = playerStatesManager.attackCounter - 1;
        if (attackData.attackDamage.Count > 0 && index >= 0 && index < attackData.maxAttackCounter)
        {
            attackData.currentAtk = attackData.baseAtk * (attackData.attackDamage[index]);
        }
    }
    private void ExAttack()
    {
        AttackDir();
        onAttackEvent.OnExAttack(transform);
        if (attackData.exDamage.Count > 0)
        {
            for (int index = 0; index < 3; index++)
            { 
                attackData.currentAtk = attackData.baseAtk * (attackData.exDamage[index]);
            }
        }
        playerStatesManager.ChangeState(State.exAttack);
    }
    public void ExitExAttack()
    {
        onAttackEvent.OutExAttack();
    }
    #endregion


    public void OnHit(int state)
    {
        nextTime = Time.time + goodTime;
        if(isGoodTime == true || playerStatesManager.currentState == State.exAttack)return;
        switch (state)
        { 
            case 0:
                playerStatesManager.ChangeState(State.isHit_Front);
                break; 
            case 1:
                playerStatesManager.ChangeState(State.isHit_Back);
                break;
        }
        isGoodTime = true;
        
    }
    public void InterimState()
    {
        playerStatesManager.ChangeState(State.interim);
    }
    public void OnFinished()
    {
        if (moveValue != Vector2.zero)
        {
            animator.Play("run_start");
            playerStatesManager.ChangeState(State.isRun);
        }
        else
        {
            animator.Play("idle");
            playerStatesManager.ChangeState(State.idle);
        }
    }
    private void Gravity()
    {
        isGround = characterController.isGrounded;
        if(isGround==true) return;
        characterController.Move(gravity * Vector3.down * Time.deltaTime);
    }
    
}
