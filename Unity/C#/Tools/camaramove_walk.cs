using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camaramove_walk : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public Transform cameraRigTransform;
    public Transform trackingSpace;

    public float amplificationFactor = 4f;
    public float detectionDistance = 1.5f;

    private Vector3 initialLocalPosition;
    private Vector3 originalRigPosition;

    void Start()
    {
        initialLocalPosition = centerEyeAnchor.localPosition;
        originalRigPosition = cameraRigTransform.position;
    }

    void Update()
    {
        if (IsNearObstacle())
        {
            return;
        }

        Vector3 delta = centerEyeAnchor.localPosition - initialLocalPosition;
        Vector3 moveDirection = trackingSpace.right * delta.x + trackingSpace.forward * delta.z;
        Vector3 amplifiedMovement = new Vector3(moveDirection.x, 0f, moveDirection.z) * amplificationFactor;
        cameraRigTransform.position = originalRigPosition + amplifiedMovement;
    }

    bool IsNearObstacle()
    {
        RaycastHit hit;
        Vector3 rayOrigin = centerEyeAnchor.position;
        Vector3 rayDirection = centerEyeAnchor.forward; 

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return true; 
            }
        }

        return false;
    }
}
