using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDetection : MonoBehaviour
{
    private Vector3 dir;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag!="Player") return;
        Vector3 tempDir = transform.position - other.transform.position;
        dir = new Vector3(tempDir.x,0f,tempDir.z);
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
