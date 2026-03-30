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
    private int currentExAttackStage;

    private void Awake()
    {
        inputActions = SharedPlayerInput.Actions;
        animator = GetComponent<Animator>();
        playerStatesManager = GetComponent<PlayerStatesManager>();
        characterController = GetComponent<CharacterController>();
        attackData = GetComponent<AttackData>();
    }
    private void OnEnable()
    {
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Evade.performed += OnEvadePerformed;
        inputActions.Player.ExAttack.performed += OnExAttackPerformed;
    }
    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Evade.performed -= OnEvadePerformed;
        inputActions.Player.ExAttack.performed -= OnExAttackPerformed;
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
        if (attackData.attackDamage.Count > 0 && index >= 0 && index < attackData.attackDamage.Count)
        {
            attackData.currentAtk = attackData.baseAtk * (attackData.attackDamage[index]);
        }
    }
    private void ExAttack()
    {
        if (gameStateManager.currentState != GameState.Player || playerStatesManager.currentState == State.exAttack)
        {
            return;
        }

        AttackDir();
        currentExAttackStage = 0;
        SetExAttackDamage(currentExAttackStage);
        onAttackEvent.OnExAttack(transform);
        playerStatesManager.ChangeState(State.exAttack);
    }
    public void SetExAttackDamage(int stageIndex)
    {
        if (attackData.exDamage.Count == 0)
        {
            return;
        }

        currentExAttackStage = Mathf.Clamp(stageIndex, 0, attackData.exDamage.Count - 1);
        attackData.currentAtk = attackData.baseAtk * attackData.exDamage[currentExAttackStage];
    }
    public void AdvanceExAttackDamage()
    {
        if (attackData.exDamage.Count == 0)
        {
            return;
        }

        SetExAttackDamage(currentExAttackStage + 1);
    }
    public void ExitExAttack()
    {
        currentExAttackStage = 0;
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

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveValue = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveValue = Vector2.zero;
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        Attack();
    }

    private void OnEvadePerformed(InputAction.CallbackContext ctx)
    {
        Evade();
    }

    private void OnExAttackPerformed(InputAction.CallbackContext ctx)
    {
        ExAttack();
    }
}
