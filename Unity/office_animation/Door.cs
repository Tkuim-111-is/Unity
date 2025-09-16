using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public Animator animator; 
    public LayerMask doorLayerMask;
    public Transform centerEyeAnchor;

    private bool isOpen = false; 

    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, doorLayerMask)) 
        {
            if (hit.collider.CompareTag("Door") && OVRInput.GetDown(OVRInput.Button.One)) 
            {
                if (isOpen)
                {
                    CloseDoor(); 
                }
                else
                {
                    OpenDoor(); 
                }
            }
        }
    }

    void OpenDoor()
    {
        animator.SetBool("isOpen", true); 
        isOpen = true; 
    }

    void CloseDoor()
    {
        animator.SetBool("isOpen", false); 
        isOpen = false; 
    }
}
