using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    public Color gizmoColor = new Color(0, 1, 1, 0.5f); // 青色半透明
    private CharacterController cc;

    void OnEnable()
    {
        cc = GetComponent<CharacterController>();
    }
    void OnDrawGizmosSelected()
    {
        if (cc == null) return;

        Gizmos.color = gizmoColor;
        Vector3 center = transform.TransformPoint(cc.center);
        float radius = cc.radius;
        float height = cc.height;

        // 画顶部半球
        Gizmos.DrawWireSphere(center + Vector3.up * (height * 0.5f - radius), radius);
        // 画底部半球
        Gizmos.DrawWireSphere(center - Vector3.up * (height * 0.5f - radius), radius);
        // 画中间柱体连线
        Vector3 top = center + Vector3.up * (height * 0.5f - radius);
        Vector3 bottom = center - Vector3.up * (height * 0.5f - radius);
        Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        Gizmos.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);
        Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
    }
   
}
