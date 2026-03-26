using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum CameraState
{
    Normal,
    Attack,
    ExAttack
}

public class CameraManager : MonoBehaviour
{
    public CinemachineTargetGroup group;
    public GameStateManager gameStateManager;
    public OnHitEvent onHitEvent;
    public OnAttackEvent onAttackEvent;
    public float distance;
    public CameraState currentCameraState;
    private PlayerInput inputActions;
    private Transform attackerTransform;
    private Transform enemyTransform;
    private CinemachineBlendDefinition normalBlend; // 相机切换的模式
    [Header("相机缩放")]
    public CinemachineFreeLook cinemachineFreeLook;
    public CinemachineFreeLook attackCamera;
    public CinemachineVirtualCamera exAttackCamera;
    public CinemachineBrain brain;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float scrollSensitivity; //缩放灵敏度
    [SerializeField] private float smoothness; //缩放速度
    private float targetDistance;
    private Vector2 Scroll; //鼠标滚轮向量
    [Header("抖动参数")]
    public CinemachineImpulseSource cinemachineImpulseSource;
    public float amplitude = 1f;
    public float frequency = 1f;
    private CinemachineBasicMultiChannelPerlin[] noises = new CinemachineBasicMultiChannelPerlin[3];


    private void Awake()
    {
        inputActions = new PlayerInput();
        onHitEvent.enemyHit += AttackCamera;
        onHitEvent.enemyHit += Reserve;
        onAttackEvent.onAttack += Shake;
        onAttackEvent.onExAttack += ExAttackCamera;
        onAttackEvent.outExAttack += ExitExAttack;
        normalBlend = brain.m_DefaultBlend;
        ChangeCamera(CameraState.Normal);
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    private void OnDestroy()
    {
        if (onHitEvent != null)
        {
            onHitEvent.enemyHit -= AttackCamera;
            onHitEvent.enemyHit -= Reserve;
            onAttackEvent.onAttack -= Shake;
            onAttackEvent.onExAttack -= ExAttackCamera;
        }
    }
    private void LateUpdate()
    {
        ZoomView();
        Detection();
        LockedCinema();
    }
    public void ChangeCamera(CameraState newState)
    {
        if (currentCameraState == newState) return;
        currentCameraState = newState;

        cinemachineFreeLook.gameObject.SetActive(false);
        attackCamera.gameObject.SetActive(false);
        exAttackCamera.gameObject.SetActive(false);
        switch (newState)
        {
            case CameraState.Normal:
                cinemachineFreeLook.gameObject.SetActive(true);
                brain.ManualUpdate();
                brain.m_DefaultBlend = normalBlend; // 重新设置为线性切换
                break;
            case CameraState.Attack:
                attackCamera.gameObject.SetActive(true);
                break;
            case CameraState.ExAttack:
                exAttackCamera.gameObject.SetActive(true);
                break;
        }
    }
    public void AttackCamera(Transform attacker,Transform enemy)
    {
        group.m_Targets = new[]
        {
            new CinemachineTargetGroup.Target { target = attacker, weight = 1, radius = 1 },
            new CinemachineTargetGroup.Target { target = enemy, weight = 1, radius = 1 }
        };
        ChangeCamera(CameraState.Attack);

    }
    public void ExAttackCamera(Transform player)
    {
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut; // 把线性切换设置为瞬间切换
        exAttackCamera.transform.rotation = Quaternion.LookRotation(-player.transform.forward, Vector3.up);
        exAttackCamera.transform.position = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 0.8f;
        ChangeCamera(CameraState.ExAttack);
        brain.ManualUpdate();
        
    }
    private void ExitExAttack()
    {
        ChangeCamera(CameraState.Normal);
    }
    private void Reserve(Transform attacker, Transform enemy) //位置缓存
    {
        attackerTransform = attacker;
        enemyTransform = enemy;
        distance = Vector3.Distance(attackerTransform.position, enemyTransform.position);
    }
    private void Detection() //距离检测
    {
        if (currentCameraState == CameraState.ExAttack) return;
        if (attackerTransform == null || enemyTransform == null)
        {
            ChangeCamera(CameraState.Normal);
            return;
        }
        distance = Vector3.Distance(attackerTransform.position, enemyTransform.position);
        if (distance > 6f)
        {
            ChangeCamera(CameraState.Normal);
        }
    }
    private void ZoomView() //鼠标缩放
    {
        if (gameStateManager.currentState != GameState.Player) return;
        Scroll = inputActions.Player.Mouse.ReadValue<Vector2>();
        targetDistance += -Scroll.y * scrollSensitivity;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        if (brain.ActiveVirtualCamera != null && brain.ActiveVirtualCamera.VirtualCameraGameObject == cinemachineFreeLook.gameObject)
        {
            cinemachineFreeLook.m_Orbits[1].m_Radius = Mathf.Lerp(cinemachineFreeLook.m_Orbits[1].m_Radius, targetDistance, Time.deltaTime * smoothness);
        }
        if (brain.ActiveVirtualCamera != null && brain.ActiveVirtualCamera.VirtualCameraGameObject == attackCamera.gameObject)
        {
            attackCamera.m_Orbits[1].m_Radius = Mathf.Lerp(attackCamera.m_Orbits[1].m_Radius, targetDistance, Time.deltaTime * smoothness);
        }
    }
    private void Shake()
    {
        cinemachineImpulseSource.GenerateImpulse();
    }
    private void LockedCinema()//屏幕锁定
    {
        if (gameStateManager.currentState != GameState.Player)
        {
            exAttackCamera.enabled = false;
            cinemachineFreeLook.enabled = false;
            attackCamera.enabled = false;
        }
        else
        {
            exAttackCamera.enabled = true; 
            cinemachineFreeLook.enabled = true;
            attackCamera.enabled = true;
        }
    }

}
