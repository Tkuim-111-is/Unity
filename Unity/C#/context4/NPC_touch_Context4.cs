using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_touch_Context4 : MonoBehaviour
{
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public GameObject NPC1_coordinate;
    public GameObject NPC2_coordinate;

    private bool callNPC1 = true;
    private bool callNPC2 = true;
    // Update is called once per frame
    void Update()
    {
        NPC_action();
    }
    void NPC_action()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag("NPC") && OVRInput.GetDown(OVRInput.Button.One))
            {
                if (callNPC1 && hit.collider.name == "NPC_1")
                {
                    NPC1_coordinate.SetActive(false);
                    Variables.ActiveScene.Set("NPC_1_Status", 1);
                    callNPC1 = false;
                }
                else if (callNPC2 && hit.collider.name == "NPC_2")
                {
                    NPC2_coordinate.SetActive(false);
                    Variables.ActiveScene.Set("NPC_2_Status", 1);
                    callNPC2 = false;
                }
            }
        }

        if (!callNPC1 && (int)Variables.ActiveScene.Get("NPC_1_Status") == -2)
        {
            NPC1_coordinate.SetActive(true);
            callNPC1 = true;
        }

        if (!callNPC2 && (int)Variables.ActiveScene.Get("NPC_2_Status") == -2)
        {
            NPC2_coordinate.SetActive(true);
            callNPC2 = true;
        }
    }
}
