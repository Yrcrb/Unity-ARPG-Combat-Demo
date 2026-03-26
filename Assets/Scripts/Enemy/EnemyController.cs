using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("组件")]
    private EnemysStateManager enemysStateManager;
    //private Rigidbody rb;
    private CharacterController cc;
    private DistanceDetection disDetection;
    public Image bloodImage;
    public OnHitEvent onHitEvent;

    [Header("状态设置")]
    [SerializeField] private float waitTime;
    public bool isWait;
    public bool isHit;
    public float blood;
    public float currentBlood;
    public float maxGoodTime = 0.3f; 
    public float currentGoodTime; //当前无敌时间
    private float nextTime = 0f;

    [Header("重力")]
    [SerializeField] private float gravity = 10f;
    public bool isGround = false;

    [Header("攻击检测")]
    public GameObject attackDetection;
    public float attackRange;

    [Header("速度设置")]
    public float rotationSpeed;
    public float speed;

    public float distance;
    private Vector3 dir;
    void Awake()
    {
        enemysStateManager = GetComponent<EnemysStateManager>();
        cc = GetComponent<CharacterController>();
        //rb = GetComponent<Rigidbody>();
        disDetection = GetComponentInChildren<DistanceDetection>();
        onHitEvent.enemyDamage += OnHit;
        currentBlood = blood;
        attackDetection.SetActive(false);
    }
    private void FixedUpdate()
    {
        
    }
    private void LateUpdate()
    {
        Gravity();
        
        if (Time.time > nextTime)
        {
            isWait = false;
            OnRun();
            OnAttack();
        }
        if (isHit)
        {
            currentGoodTime -= Time.deltaTime;
        }
        if (currentGoodTime <= 0f)
        {
            currentGoodTime = maxGoodTime;
            isHit = false;
        }
    }
    private void OnDestroy()
    {
        onHitEvent.enemyDamage -= OnHit;
    }
    public void OnHit(Transform player,float atk)
    { 
        isHit = true;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed);
        currentGoodTime = maxGoodTime;
        currentBlood -= atk;
        bloodImage.fillAmount = Mathf.Clamp(currentBlood / blood, 0, 1);
        enemysStateManager.ChangeState(EnemyState.isHit);
    }
    public void OnRun()
    {
        if(disDetection.playerTransform == null || enemysStateManager.currentState == EnemyState.isAttack) return;
        Vector3 TempDir = (disDetection.playerTransform.position - transform.position).normalized;
        dir = new Vector3(TempDir.x, 0f, TempDir.z);
        distance = Vector3.Distance(disDetection.playerTransform.position, transform.position);
        if ((Vector3.Angle(dir, transform.forward) < 90 || enemysStateManager.currentState == EnemyState.isHit)&& distance < 8f)
        { 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
            if (distance > 1.5f)
            { 
                Vector3 targetPosition = dir * speed * Time.deltaTime;
                cc.Move(targetPosition);
                enemysStateManager.ChangeState(EnemyState.isRun);  
            }
        }
        if (distance > 9f)
        {
            enemysStateManager.ChangeState(EnemyState.idle);
            disDetection.playerTransform = null;
        }
    }
    public void OnAttack()
    {
        if (distance < attackRange)
        {
            enemysStateManager.ChangeState(EnemyState.isAttack);
        }
    }
    public void AttackDetection()
    { 
        attackDetection.SetActive(true);
    }
    public void Interim()
    {
        isWait = true;
        attackDetection.SetActive(false);
        nextTime = Time.time + waitTime;
        if (distance < attackRange)
        { 
            enemysStateManager.ChangeState(EnemyState.interim);
        }
    }
    private void Gravity()
    {
        isGround = cc.isGrounded;
        if (isGround == true) return;
        cc.Move(gravity * Vector3.down * Time.deltaTime);
    }
}
