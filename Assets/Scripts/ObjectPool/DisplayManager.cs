using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject damagePrefab;
    private ObjectPool<DamageDisplay> pool;
    public OnHitEvent onHitEvent;
    private void Awake()
    {
        pool = new ObjectPool<DamageDisplay>(
            () =>
            {
                var go = Instantiate(damagePrefab, transform);
                go.SetActive(false);
                return go.GetComponent<DamageDisplay>();
            },
            (DamageDisplay display) => display.gameObject.SetActive(true),
            (DamageDisplay display) => display.gameObject.SetActive(false),
             100, 10);
        onHitEvent.Damage += OnDamageTaken;
    }
    private void OnDamageTaken(float damageValue)
    {
        StartCoroutine(ShowDamage(damageValue));
    }

    private IEnumerator ShowDamage(float damageValue)
    {
        DamageDisplay display = pool.Get();
        if (display == null)
        { 
            yield break; 
        }
        display.DamageReset(damageValue);
        yield return new WaitForSeconds(0.6f);
        pool.Release(display);
    }
    private void OnDestroy()
    {
        onHitEvent.Damage -= OnDamageTaken;
    }
}
