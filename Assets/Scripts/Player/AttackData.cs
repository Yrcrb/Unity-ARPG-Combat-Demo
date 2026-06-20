using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData : MonoBehaviour
{
    public int maxAttackCounter;
    public float baseAtk;
    public float currentAtk;
    public List<float> attackDamage;
    public List<float> exDamage;
}
