using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectZoom : MonoBehaviour
{
    public OVRRayHelper rayHelper; // OVR �� Ray �U��
    public LayerMask objectLayer; // �u���� 3D ���� Layer
    public string targetTag = "Check"; // ���w�n��j�� 3D ���� Tag
    public float scaleFactor = 1.5f; // ��j���v

    private Transform currentTarget; // ��e���쪺����
    private Vector3 originalScale; // �쥻�j�p

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
                    ResetCurrentTarget(); // �����ª��Y�^
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
