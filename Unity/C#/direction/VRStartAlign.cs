using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRStartAlign : MonoBehaviour
{
    public OVRCameraRig rig;
    public Transform target;

    void Start()
    {
        if (rig == null || target == null) return;

        // ��X HMD �P Rig ���۹�첾
        Vector3 headOffset = rig.centerEyeAnchor.localPosition;

        // �� Rig ���� target ��m�A�ýվ㭱�V
        rig.transform.position = target.position - headOffset;
        rig.transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }
}
