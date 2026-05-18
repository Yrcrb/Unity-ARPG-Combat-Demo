using UnityEngine;

public class DistanceDetection : MonoBehaviour
{
    // 敌人控制器，感知到目标后通过它同步黑板。
    private EnemyController enemyController;

    // 当前停留在感知范围里的玩家引用。
    public Transform playerTransform;

    // 缓存父物体上的敌人控制器。
    private void Awake()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    // 玩家进入感知范围时把目标同步给敌人控制器。
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerTransform = other.transform;
        enemyController.SetTarget(playerTransform);
    }

    // 玩家离开感知范围时清理当前目标引用。
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (playerTransform != other.transform)
        {
            return;
        }

        playerTransform = null;
        enemyController.ClearTarget(other.transform);
    }
}
