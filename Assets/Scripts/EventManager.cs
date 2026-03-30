using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Hit Event")]
public class OnHitEvent : ScriptableObject
{
    public event Action<Transform,Transform> enemyHit;
    public event Action<Vector3, Vector3> hitSpecialEffect;
    public event Action<float> Damage;
    public event Action onHit;
    public void EnemyHit(Transform attacker,Transform hiter) => enemyHit?.Invoke(attacker,hiter);
    public void HitSpecialEffect(Vector3 position,Vector3 forward) => hitSpecialEffect?.Invoke(position, forward);
    public void OnDamage(float damage) => Damage?.Invoke(damage);
    public void OnHit() => onHit?.Invoke();
}
[CreateAssetMenu(menuName = "Events/Attack Event")]
public class OnAttackEvent : ScriptableObject
{
    public event Action onAttack;
    public event Action<Transform> onExAttack;
    public event Action outExAttack;
    public void OnAttack() => onAttack?.Invoke();
    public void OnExAttack(Transform player) => onExAttack?.Invoke(player);
    public void OutExAttack() => outExAttack?.Invoke();
}
