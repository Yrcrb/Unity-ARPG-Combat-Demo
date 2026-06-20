using Cysharp.Threading.Tasks;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject damagePrefab;
    private ObjectPool<DamageDisplay> pool;
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
        EventBus.Instance.Add<float>(E.OnDamage, OnDamageTaken);
    }
    private void OnDamageTaken(float damageValue)
    {
        ShowDamage(damageValue).Forget();
    }

    private async UniTaskVoid ShowDamage(float damageValue)
    {
        DamageDisplay display = pool.Get();
        if (display == null)
            return;
        display.DamageReset(damageValue);
        await UniTask.Delay(600);
        pool.Release(display);
    }
    private void OnDestroy()
    {
        EventBus.Instance.Remove<float>(E.OnDamage, OnDamageTaken);
    }
}
