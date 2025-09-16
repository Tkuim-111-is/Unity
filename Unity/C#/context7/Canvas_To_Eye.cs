using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Canvas_To_Eye : MonoBehaviour
{
    [Header("OVR Ray")]
    public Transform rightHandPointer;           // �k��p�g/����e��
    public float rayLength = 12f;

    [Header("Destination (CenterEye/Copy_Canvas)")]
    public Canvas destinationCanvas;             // ���V CenterEyeAnchor �U�� Copy_Canvas�]World Space�^
    public string clonePrefix = "[CLONE] ";      // �ƻs�~�R�W�e��

    [Header("Source Filter")]
    public string sourceCanvasTag = "Canvas";    // �u�����a Tag=Canvas ���ӷ�
    public int copyRootDepth = 0;                // ���W���X�h�A�ƻs�]0=�R�������ӡ^

    [Header("Fit Mode")]
    public bool centerInTarget = true;           // �Q���ɬO�_�m��

    // �l�ܷ�e��ܤ����ƻs�~�]�ΨӰ� toggle�^
    private GameObject currentClone;

    void Update()
    {
        // �k�ⰼ����]Grip�^Ĳ�o�FSecondaryHandTrigger = �k��
        if (!OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.RTouch))
            return;

        if (destinationCanvas == null) return;

        // �Y�w�g���ƻs�~ �� �o������N�R���]toggle �����^
        if (currentClone != null)
        {
            Destroy(currentClone);
            currentClone = null;
            return;
        }

        // �S���ƻs�~ �� ���ծھڹp�g�R���h�ƻs
        if (rightHandPointer == null) return;
        if (!TryGetUIUnderOVRRay(out GameObject hitGO)) return;

        RectTransform src = FindSourceRoot(hitGO.transform as RectTransform);
        if (src == null) return;

        RectTransform dstParent = destinationCanvas.transform as RectTransform;

        // ���Ͱƥ��þQ�� Copy_Canvas
        var clone = Instantiate(src.gameObject, dstParent);
        clone.name = clonePrefix + src.gameObject.name;
        var cloneRT = clone.transform as RectTransform;

        // �M��������A�A�קK�ƥ��ߨ�Q EventSystem ���
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        foreach (var sel in clone.GetComponentsInChildren<Selectable>(true)) sel.OnDeselect(null);

        // �Q�� Copy_Canvas
        FitRectTransformFill(cloneRT, dstParent, centerInTarget);

        currentClone = clone; // �O�����ƥ��A��K�U�@������R��
    }

    RectTransform FindSourceRoot(RectTransform hit)
    {
        if (hit == null) return null;

        // ���� copyRootDepth ���W
        RectTransform cur = hit;
        for (int i = 0; i < copyRootDepth && cur.parent is RectTransform; i++)
            cur = cur.parent as RectTransform;

        // �A���W��̪� Tag=Canvas
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

    // ���s���G�Q���ؼС]�����ӷ���ҡ^
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
