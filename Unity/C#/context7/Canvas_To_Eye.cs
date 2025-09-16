using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Canvas_To_Eye : MonoBehaviour
{
    [Header("OVR Ray")]
    public Transform rightHandPointer;           // 右手雷射/控制器前端
    public float rayLength = 12f;

    [Header("Destination (CenterEye/Copy_Canvas)")]
    public Canvas destinationCanvas;             // 指向 CenterEyeAnchor 下的 Copy_Canvas（World Space）
    public string clonePrefix = "[CLONE] ";      // 複製品命名前綴

    [Header("Source Filter")]
    public string sourceCanvasTag = "Canvas";    // 只接受帶 Tag=Canvas 的來源
    public int copyRootDepth = 0;                // 往上取幾層再複製（0=命中的那個）

    [Header("Fit Mode")]
    public bool centerInTarget = true;           // 鋪滿時是否置中

    // 追蹤當前顯示中的複製品（用來做 toggle）
    private GameObject currentClone;

    void Update()
    {
        // 右手側邊鍵（Grip）觸發；SecondaryHandTrigger = 右手
        if (!OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.RTouch))
            return;

        if (destinationCanvas == null) return;

        // 若已經有複製品 → 這次按鍵就刪掉（toggle 關閉）
        if (currentClone != null)
        {
            Destroy(currentClone);
            currentClone = null;
            return;
        }

        // 沒有複製品 → 嘗試根據雷射命中去複製
        if (rightHandPointer == null) return;
        if (!TryGetUIUnderOVRRay(out GameObject hitGO)) return;

        RectTransform src = FindSourceRoot(hitGO.transform as RectTransform);
        if (src == null) return;

        RectTransform dstParent = destinationCanvas.transform as RectTransform;

        // 產生副本並鋪滿 Copy_Canvas
        var clone = Instantiate(src.gameObject, dstParent);
        clone.name = clonePrefix + src.gameObject.name;
        var cloneRT = clone.transform as RectTransform;

        // 清掉選取狀態，避免副本立刻被 EventSystem 選取
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        foreach (var sel in clone.GetComponentsInChildren<Selectable>(true)) sel.OnDeselect(null);

        // 鋪滿 Copy_Canvas
        FitRectTransformFill(cloneRT, dstParent, centerInTarget);

        currentClone = clone; // 記住本次副本，方便下一次按鍵刪除
    }

    RectTransform FindSourceRoot(RectTransform hit)
    {
        if (hit == null) return null;

        // 先依 copyRootDepth 往上
        RectTransform cur = hit;
        for (int i = 0; i < copyRootDepth && cur.parent is RectTransform; i++)
            cur = cur.parent as RectTransform;

        // 再往上找最近的 Tag=Canvas
        while (cur != null)
        {
            if (cur.gameObject.CompareTag(sourceCanvasTag))
                return cur;
            cur = cur.parent as RectTransform;
        }
        return null;
    }

    bool TryGetUIUnderOVRRay(out GameObject go)
    {
        go = null;

        var ovrData = new OVRPointerEventData(EventSystem.current)
        {
            worldSpaceRay = new Ray(rightHandPointer.position, rightHandPointer.forward)
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ovrData, results);

        float bestDist = Mathf.Infinity;
        GameObject best = null;
        foreach (var r in results)
        {
            if (r.distance > rayLength) continue;
            if (r.distance < bestDist)
            {
                bestDist = r.distance;
                best = r.gameObject;
            }
        }

        if (best != null)
        {
            go = best;
            return true;
        }
        return false;
    }

    // ★新版：鋪滿目標（忽略來源比例）
    void FitRectTransformFill(RectTransform obj, RectTransform target, bool center)
    {
        obj.SetParent(target, worldPositionStays: false);
        obj.anchorMin = new Vector2(0f, 0f);
        obj.anchorMax = new Vector2(1f, 1f);
        obj.pivot = new Vector2(0.5f, 0.5f);

        obj.offsetMin = Vector2.zero;
        obj.offsetMax = Vector2.zero;
        obj.localScale = Vector3.one;
        obj.localRotation = Quaternion.identity;

        if (center) obj.anchoredPosition = Vector2.zero;
    }
}
