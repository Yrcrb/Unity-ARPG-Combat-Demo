using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceDetection : MonoBehaviour
{
    [SerializeField] private EnemysStateManager enemysStateManager;
    public Transform playerTransform;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            playerTransform = other.transform;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        //playerTransform = null;
    }



















}
