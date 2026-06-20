using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private PlayerStatesManager playerStatesManager;
    private Transform enemyTransform;
    private PlayerControls playerControls;
    private AttackData attackData;
    //public LayerMask enemyLayer;
    private int layerMask;
    private void Awake()
    {
        playerStatesManager = transform.root.GetComponent<PlayerStatesManager>();
        playerControls = transform.root.GetComponent<PlayerControls>();
        attackData = transform.root.GetComponent<AttackData>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return;

        //敌方碰撞体与武器碰撞体的向量方向
        Vector3 forward = (transform.position - other.transform.position).normalized;
        //估算碰撞点位置
        Vector3 position = other.ClosestPoint(transform.position);
        if (playerStatesManager.currentState == State.isAttack)
        {
            EventBus.Instance.Invoke(E.EnemyHit, transform.root, other.transform);
        }
        if (playerStatesManager.currentState == State.isAttack || playerStatesManager.currentState == State.exAttack)
        {
            enemy.TakeDamage(transform.root, attackData.currentAtk, position, forward);
        }
        enemyTransform = other.transform;
        playerControls.EnemyLocate(enemyTransform);
       
    }



}
