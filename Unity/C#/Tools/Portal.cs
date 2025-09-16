using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public string targetTag = "Portal";

    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer)){
            if (hit.collider.CompareTag(targetTag) && OVRInput.GetDown(OVRInput.Button.One)){
                Vector3 targetPosition = hit.collider.transform.position;
                Vector3 newPlayerPosition = new Vector3(targetPosition.x, player.position.y, targetPosition.z);
                player.position = newPlayerPosition;
            }
        }
    }
}
