using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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