using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectZoom : MonoBehaviour
{
    public OVRRayHelper rayHelper; // OVR 的 Ray 助手
    public LayerMask objectLayer; // 只偵測 3D 物件的 Layer
    public string targetTag = "Check"; // 指定要放大的 3D 物件 Tag
    public float scaleFactor = 1.5f; // 放大倍率

    private Transform currentTarget; // 當前指到的物件
    private Vector3 originalScale; // 原本大小

    void Update()
    {
        Detect3DObject();
    }

    void Detect3DObject()
    {
        if (rayHelper == null) return;

        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider != null && (hit.collider.CompareTag(targetTag) /*|| hit.collider.CompareTag("Portal")*/))
            {
                if (currentTarget != hit.collider.transform)
                {
                    ResetCurrentTarget(); // 先把舊的縮回
                    currentTarget = hit.collider.transform;
                    originalScale = currentTarget.localScale;
                    currentTarget.localScale = originalScale * scaleFactor;
                }
            }
            else
            {
                ResetCurrentTarget();
            }
        }
        else
        {
            ResetCurrentTarget();
        }
    }

    void ResetCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.localScale = originalScale;
            currentTarget = null;
        }
    }
}
