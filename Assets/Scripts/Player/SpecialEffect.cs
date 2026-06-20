using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEffect : MonoBehaviour
{
    public GameObject[] attackEffects;
    public GameObject hitEffects;
    public Transform parentObject;
    private void Awake()
    {
        EventBus.Instance.Add<Vector3, Vector3>(E.HitVFX, HitVFX);
    }
    public void AttackVFX(int vfxIndex)
    {
        if (attackEffects == null || vfxIndex < 0 || vfxIndex >= attackEffects.Length) return;
        if (parentObject == null) return;
        EventBus.Instance.Invoke(E.OnAttack);
        GameObject vfx = Instantiate(attackEffects[vfxIndex]);
        vfx.transform.SetParent(parentObject, false);
        Destroy(vfx, 1f);
    }
    public void HitVFX(Vector3 position,Vector3 forward)
    {
        GameObject vfx = Instantiate(hitEffects,position,Quaternion.LookRotation(forward,Vector3.up));
        Destroy(vfx, 1f);
    }
}
