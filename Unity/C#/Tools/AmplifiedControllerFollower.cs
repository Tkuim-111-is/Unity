using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmplifiedControllerFollower : MonoBehaviour
{
    [Header("控制器與倍率")]
    public Transform realController;           // VR 控制器
    public float movementMultiplier = 2.0f;     // 放大倍率

    private Vector3 lastControllerPosition;

    void Start()
    {
        if (realController == null)
        {
            Debug.LogError("請指定 realController！");
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
