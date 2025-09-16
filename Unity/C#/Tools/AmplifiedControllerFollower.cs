using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmplifiedControllerFollower : MonoBehaviour
{
    [Header("����P���v")]
    public Transform realController;           // VR ���
    public float movementMultiplier = 2.0f;     // ��j���v

    private Vector3 lastControllerPosition;

    void Start()
    {
        if (realController == null)
        {
            Debug.LogError("�Ы��w realController�I");
            enabled = false;
            return;
        }

        lastControllerPosition = realController.position;
    }

    void Update()
    {
        Vector3 controllerDelta = realController.position - lastControllerPosition;
        transform.position += controllerDelta * movementMultiplier;
        lastControllerPosition = realController.position;
    }
}
