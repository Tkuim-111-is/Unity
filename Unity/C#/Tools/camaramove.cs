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

    public Transform player;  // 引用 player 物件
    public Transform OVRcamara;
    public Transform centerEyeAnchor;

    private BoxCollider playerCollider;  // 存儲 player 的 BoxCollider

    void Start()
    {
        playerCollider = player.GetComponent<BoxCollider>();  // 取得 player 上的 BoxCollider
    }

    void Update()
    {
        // 旋轉控制 (右搖桿)
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, R_controller);
        float rotationAmount = rotationInput.x * rotationSpeed * Time.deltaTime;
        player.Rotate(0, rotationAmount, 0, Space.Self);  // 旋轉 player 物件

        // 如果接近障礙物就停止移動
        if (IsNearObstacle()) return;

        // 移動控制 (左搖桿)
        Vector2 movementInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, L_controller);
        Vector3 forward = centerEyeAnchor.forward;
        forward.y = 0;  // 保證y軸為0，避免垂直移動
        Vector3 right = centerEyeAnchor.right;
        Vector3 moveDirection = forward * movementInput.y + right * movementInput.x;
        moveDirection.y = 0;  // 保證y軸為0，避免垂直移動

        player.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);  // 移動 player 物件
    }

    bool IsNearObstacle()
    {
        RaycastHit hit;
        Vector3 rayOrigin = playerCollider.bounds.center;  // 使用 BoxCollider 的中心點來發射射線
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