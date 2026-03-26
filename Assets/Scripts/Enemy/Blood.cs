using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blood : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 cameraDir;
    private void Awake()
    {
            mainCamera = Camera.main;
    }
    private void LateUpdate()
    {
        cameraDir = mainCamera.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-cameraDir, Vector3.up);
    }
}
