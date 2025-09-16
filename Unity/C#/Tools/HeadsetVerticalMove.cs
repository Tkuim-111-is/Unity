using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetVerticalMove : MonoBehaviour
{
    [Header("�Ѧ�")]
    [SerializeField] Transform rigRoot;        // ��ĳ=���a�ک� OVRCameraRig
    [SerializeField] Transform centerEye;      // �� CenterEyeAnchor

    [Header("�Ѽ�")]
    [SerializeField] float multiplier = 1.0f;  // ��j����
    [SerializeField] float maxUp = 1.5f;       // ���\��_�l�����̤j�q
    [SerializeField] float maxDown = 0.5f;     // ���\��_�l�C���̤j�q
    [SerializeField] float smoothTime = 0.08f; // ���Ʈɶ�(��)
    [SerializeField] bool useLocal = false;    // true=�� localPosition�Afalse=�� world position
    [SerializeField] bool alignYawOnly = true; // �Y�A�P�ɦb�O�B�B�z�¦V�A�O�������Y�i

    float baseEyeY, baseRigY, velY;

    void Start()
    {
        if (!rigRoot) rigRoot = transform;

        // �Y�����ץ� local�]�۹� tracking space�^
        baseEyeY = centerEye.localPosition.y;

        // Rig ��ǰ��ץi�� local �� world�A�O���@�P
        baseRigY = useLocal ? rigRoot.localPosition.y : rigRoot.position.y;
    }

    void LateUpdate() // �� LateUpdate�G�b OVR ��s����A�M��m�A�����|�@�V����
    {
        float eyeY = centerEye.localPosition.y;
        float deltaY = (eyeY - baseEyeY) * multiplier;

        float unclamped = baseRigY + deltaY;
        float targetY = Mathf.Clamp(unclamped, baseRigY - maxDown, baseRigY + maxUp);

        if (useLocal)
        {
            var lp = rigRoot.localPosition;
            lp.y = Mathf.SmoothDamp(lp.y, targetY, ref velY, smoothTime);
            rigRoot.localPosition = lp;
        }
        else
        {
            var p = rigRoot.position;
            p.y = Mathf.SmoothDamp(p.y, targetY, ref velY, smoothTime);
            rigRoot.position = p;
        }
    }
}
