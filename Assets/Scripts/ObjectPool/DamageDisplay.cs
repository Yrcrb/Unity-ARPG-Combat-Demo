using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageDisplay : MonoBehaviour
{
    public TextMeshProUGUI countText;
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
    public void DamageReset(float damage)
    {
        countText.text = damage.ToString();
    }

}
