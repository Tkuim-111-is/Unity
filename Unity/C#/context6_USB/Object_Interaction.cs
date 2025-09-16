using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.VisualScripting;

public class Object_Interaction : MonoBehaviour
{
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public string targetTag = "Check";
    public Transform attachPoint; // 控制器前方空物件
    public string groundTag = "Ground";
    public GameObject changePlaceObject;
    public GameObject backpackUI;
    public GameObject NPC2_coordinate;
    public GameObject NPC3_coordinate;
    public bool isBackpackOpen = false;

    private Transform heldObject;
    private Rigidbody heldRigidbody;
    private bool isHolding = false;
    private bool holdUSB = true;
    private bool callNPC2 = true;
    private bool callNPC3 = true;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            isBackpackOpen = !isBackpackOpen;
            backpackUI.SetActive(isBackpackOpen);
        }

        DetectAndPickup(); // 原本撿東西的功能

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            // 按下 A 前先確認是不是剛剛點了 UI
            if (BackpackManager.Instance.justEquippedViaUI)
            {
                BackpackManager.Instance.justEquippedViaUI = false; // 重設
            }
            else
            {
                BackpackManager.Instance.UnequipCurrentItem(); // 真正執行還回背包
            }
        }
    }
    void DetectAndPickup()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag(targetTag) && OVRInput.GetDown(OVRInput.Button.One))
            {
                GameObject item = hit.collider.gameObject;
                BackpackManager.Instance.AddItem(item); // 加入背包
            }
        }
    }
}

/*
 void DetectAndPickup()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag(targetTag) && OVRInput.GetDown(OVRInput.Button.One))
            {
                heldObject = hit.collider.transform;
                heldRigidbody = heldObject.GetComponent<Rigidbody>();

                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = true;
                }

                // 撿起：設為 HandGrabPoint 的子物件，並位置歸零
                heldObject.SetParent(attachPoint);
                heldObject.localPosition = Vector3.zero;
                heldObject.localRotation = Quaternion.identity;
                if (holdUSB && Variables.Application.Get<int>("NPC_1_Status") != 2 && hit.collider.name == "USB")
                {
                    Variables.Application.Set("NPC_1_Status", 2);
                    changePlaceObject.SetActive(false);
                    //holdUSB = false;
                }
                isHolding = true;
            }
        }
    }
 */


/*void HandleDrop()
{
    if (OVRInput.GetDown(OVRInput.Button.One))
    {
        // 放下物體
        heldObject.SetParent(null);

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
        }

        heldObject = null;
        heldRigidbody = null;
        isHolding = false;
    }
}*/
