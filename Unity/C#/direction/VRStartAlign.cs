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

        // 找出 HMD 與 Rig 的相對位移
        Vector3 headOffset = rig.centerEyeAnchor.localPosition;

        // 把 Rig 移到 target 位置，並調整面向
        rig.transform.position = target.position - headOffset;
        rig.transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }
}
