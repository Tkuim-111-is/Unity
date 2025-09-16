using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetVerticalMove : MonoBehaviour
{
    [Header("參考")]
    [SerializeField] Transform rigRoot;        // 建議=玩家根或 OVRCameraRig
    [SerializeField] Transform centerEye;      // 拖 CenterEyeAnchor

    [Header("參數")]
    [SerializeField] float multiplier = 1.0f;  // 放大倍數
    [SerializeField] float maxUp = 1.5f;       // 允許比起始高的最大量
    [SerializeField] float maxDown = 0.5f;     // 允許比起始低的最大量
    [SerializeField] float smoothTime = 0.08f; // 平滑時間(秒)
    [SerializeField] bool useLocal = false;    // true=用 localPosition，false=用 world position
    [SerializeField] bool alignYawOnly = true; // 若你同時在別處處理朝向，保持水平即可

    float baseEyeY, baseRigY, velY;

    void Start()
    {
        if (!rigRoot) rigRoot = transform;

        // 頭盔高度用 local（相對 tracking space）
        baseEyeY = centerEye.localPosition.y;

        // Rig 基準高度可選 local 或 world，保持一致
        baseRigY = useLocal ? rigRoot.localPosition.y : rigRoot.position.y;
    }

    void LateUpdate() // 放 LateUpdate：在 OVR 更新之後再套位置，較不會一幀錯位
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
