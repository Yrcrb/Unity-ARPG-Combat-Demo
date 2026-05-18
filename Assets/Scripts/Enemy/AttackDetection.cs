using UnityEngine;

public class AttackDetection : MonoBehaviour
{
    // 敌人与玩家之间的平面方向，用来判断前后受击。
    private Vector3 dir;

    // 攻击碰撞体命中玩家后，根据朝向决定玩家受击方向。
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            return;
        }

        Vector3 tempDir = transform.position - other.transform.position;
        dir = new Vector3(tempDir.x, 0f, tempDir.z);

        if (Vector3.Angle(dir, other.transform.forward) > 90)
        {
            other.GetComponent<PlayerControls>().OnHit(0);
        }

        if (Vector3.Angle(dir, other.transform.forward) <= 90)
        {
            other.GetComponent<PlayerControls>().OnHit(1);
        }
    }
}
