using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    public GameStateManager gameStateManager;
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

    // 动画事件触发后允许切出攻击
    private bool canExitAttack;
    private bool canExitExAttack;

    private void Awake()
    {
        if (gameStateManager == null) gameStateManager = GameStateManager.Instance;
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
        if (inputActions == null) return;
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
            if (dist < 0.8f)
                rootMotion = Vector3.ProjectOnPlane(rootMotion, transform.forward);
        }

        characterController.Move(rootMotion);
    }
    void Update()
    {
        if (Time.time >= nextTime)
            isGoodTime = false;
        CheckAttackExit();
        Move();
        Gravity();
    }

    #region Move
    public void Move()
    {
        if (gameStateManager.currentState != GameState.Player) return;
        if (playerStatesManager.currentState == State.isAttack || playerStatesManager.currentState == State.exAttack || playerStatesManager.currentState == State.evade) return;

        Vector3 cameraForword = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized;
        dir = (moveValue.y * cameraForword + moveValue.x * cameraRight).normalized;

        if (moveValue != Vector2.zero)
        {
            rotateSpeed = 1 - Mathf.Exp(-20f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), rotateSpeed);
            if (playerStatesManager.currentState != State.isRun)
                playerStatesManager.ChangeState(State.isRun);
        }
        else if (playerStatesManager.currentState == State.isRun)
        {
            playerStatesManager.ChangeState(State.run_end);
        }
    }
    #endregion

    #region CheckAttackExit
    /// <summary>
    /// 攻击允许切出 + 锁定时间到 → 根据输入切到 isRun 或 idle。
    /// </summary>
    private void CheckAttackExit()
    {
        if (canExitAttack && playerStatesManager.currentState == State.isAttack)
        {
            canExitAttack = false;
            if (moveValue != Vector2.zero)
                playerStatesManager.ChangeState(State.isRun);
            else
                playerStatesManager.ChangeState(State.idle);
        }

        if (canExitExAttack && playerStatesManager.currentState == State.exAttack)
        {
            canExitExAttack = false;
            ExitExAttack();
            if (moveValue != Vector2.zero)
                playerStatesManager.ChangeState(State.isRun);
            else
                playerStatesManager.ChangeState(State.idle);
        }
    }
    #endregion

    #region Animation Events
    /// <summary>
    /// 动画事件：攻击 clip 命中帧之后调用，允许切出攻击状态。
    /// </summary>
    public void OnAttackAllowExit()
    {
        canExitAttack = true;
    }

    /// <summary>
    /// 动画事件：exAttack clip 允许切出。
    /// </summary>
    public void OnExAttackAllowExit()
    {
        canExitExAttack = true;
    }

    /// <summary>
    /// 动画事件：run_brake 播完，回到 idle。
    /// </summary>
    public void OnRunEndFinished()
    {
        if (playerStatesManager.currentState == State.run_end)
            playerStatesManager.ChangeState(State.idle);
    }

    /// <summary>
    /// 动画事件：闪避 / 受击 等一次性动画播完后的统一收尾。
    /// </summary>
    public void OnFinished()
    {
        if (moveValue != Vector2.zero)
            playerStatesManager.ChangeState(State.isRun);
        else
            playerStatesManager.ChangeState(State.idle);
    }
    #endregion

    #region Evade
    public void Evade()
    {
        if (playerStatesManager.currentState == State.exAttack) return;
        playerStatesManager.ChangeState(State.evade);
    }
    #endregion

    #region Attack
    public void Attack()
    {
        if (gameStateManager.currentState != GameState.Player || playerStatesManager.currentState == State.exAttack) return;
        if (enemyTransform != null && Vector3.Distance(transform.position, enemyTransform.position) < 6f)
            AttackDir();

        if (playerStatesManager.currentState != State.isAttack)
            playerStatesManager.attackCounter = 1;
        else
        {
            playerStatesManager.attackCounter++;
            if (playerStatesManager.attackCounter > attackData.maxAttackCounter)
                playerStatesManager.attackCounter = 1;
        }
        AttackDamage();
        canExitAttack = false;
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
        diff.y = 0;
        if (diff.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(diff, Vector3.up);
    }

    private void AttackDamage()
    {
        int index = playerStatesManager.attackCounter - 1;
        if (attackData.attackDamage.Count > 0 && index >= 0 && index < attackData.attackDamage.Count)
            attackData.currentAtk = attackData.baseAtk * (attackData.attackDamage[index]);
    }

    private void ExAttack()
    {
        if (gameStateManager.currentState != GameState.Player || playerStatesManager.currentState == State.exAttack)
            return;

        AttackDir();
        currentExAttackStage = 0;
        SetExAttackDamage(currentExAttackStage);
        EventBus.Instance.Invoke(E.OnExAttack, transform);
        canExitExAttack = false;
        playerStatesManager.ChangeState(State.exAttack);
    }

    public void SetExAttackDamage(int stageIndex)
    {
        if (attackData.exDamage.Count == 0) return;
        currentExAttackStage = Mathf.Clamp(stageIndex, 0, attackData.exDamage.Count - 1);
        attackData.currentAtk = attackData.baseAtk * attackData.exDamage[currentExAttackStage];
    }

    public void AdvanceExAttackDamage()
    {
        if (attackData.exDamage.Count == 0) return;
        SetExAttackDamage(currentExAttackStage + 1);
    }

    public void ExitExAttack()
    {
        currentExAttackStage = 0;
        EventBus.Instance.Invoke(E.OutExAttack);
    }
    #endregion

    public void OnHit(int state)
    {
        nextTime = Time.time + goodTime;
        if (isGoodTime == true || playerStatesManager.currentState == State.exAttack) return;
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

    private void Gravity()
    {
        isGround = characterController.isGrounded;
        if (isGround == true) return;
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
