using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// left right hand control direct
public class camaramove : MonoBehaviour
{
    public OVRInput.Controller L_controller = OVRInput.Controller.LTouch;
    public OVRInput.Controller R_controller = OVRInput.Controller.RTouch;
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 100.0f;
    public float detectionDistance = 1.5f;

    public Transform player;  // �ޥ� player ����
    public Transform OVRcamara;
    public Transform centerEyeAnchor;

    private BoxCollider playerCollider;  // �s�x player �� BoxCollider

    void Start()
    {
        playerCollider = player.GetComponent<BoxCollider>();  // ���o player �W�� BoxCollider
    }

    void Update()
    {
        // ���౱�� (�k�n��)
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, R_controller);
        float rotationAmount = rotationInput.x * rotationSpeed * Time.deltaTime;
        player.Rotate(0, rotationAmount, 0, Space.Self);  // ���� player ����

        // �p�G�����ê���N�����
        if (IsNearObstacle()) return;

        // ���ʱ��� (���n��)
        Vector2 movementInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, L_controller);
        Vector3 forward = centerEyeAnchor.forward;
        forward.y = 0;  // �O��y�b��0�A�קK��������
        Vector3 right = centerEyeAnchor.right;
        Vector3 moveDirection = forward * movementInput.y + right * movementInput.x;
        moveDirection.y = 0;  // �O��y�b��0�A�קK��������

        player.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);  // ���� player ����
    }

    bool IsNearObstacle()
    {
        RaycastHit hit;
        Vector3 rayOrigin = playerCollider.bounds.center;  // �ϥ� BoxCollider �������I�ӵo�g�g�u
        Vector3 rayDirection = centerEyeAnchor.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Door"))
            {
                return true;
            }
        }

        return false;
    }
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// left right hand control direct
public class camaramove : MonoBehaviour
{
    public OVRInput.Controller L_controller = OVRInput.Controller.LTouch;
    public OVRInput.Controller R_controller = OVRInput.Controller.RTouch;
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 100.0f;
    public float detectionDistance = 1.5f; 

    public Transform cameraRigTransform;
    public Transform centerEyeAnchor;

    void Update()
    {
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, R_controller);
        float rotationAmount = rotationInput.x * rotationSpeed * Time.deltaTime;
        cameraRigTransform.Rotate(0, rotationAmount, 0, Space.World);

        if (IsNearObstacle())
        {
            return;
        }

        Vector2 movementInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, L_controller);
        Vector3 forward = centerEyeAnchor.forward;
        forward.y = 0; 
        Vector3 right = centerEyeAnchor.right;
        Vector3 moveDirection = forward * movementInput.y + right * movementInput.x;
        moveDirection.y = 0;
        cameraRigTransform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    bool IsNearObstacle()
    {
        RaycastHit hit;
        Vector3 rayOrigin = centerEyeAnchor.position; 
        Vector3 rayDirection = centerEyeAnchor.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Door"))
            {
                return true;
            }
        }

        return false; 
    }
}
*/