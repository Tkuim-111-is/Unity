using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_touch_Context6 : MonoBehaviour
{
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public GameObject NPC2_coordinate;
    public GameObject NPC3_coordinate;
    public VoiceToGPT VoiceToGPT;
    
    private bool callNPC2 = true;
    private bool callNPC3 = true;
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
                if (callNPC2 && hit.collider.name == "NPC_2")
                {
                    NPC2_coordinate.SetActive(false);
                    Variables.ActiveScene.Set("NPC_2_Status", 1);
                    StartCoroutine(VoiceToGPT.NPC2_Start_talk());
                    callNPC2 = false;
                }
                else if (callNPC3 && hit.collider.name == "NPC_3" && (int)Variables.ActiveScene.Get("NPC_2_Status") == 4)
                {
                    NPC3_coordinate.SetActive(false);
                    Variables.ActiveScene.Set("NPC_3_Status", 1);
                    StartCoroutine(VoiceToGPT.NPC3_Start_talk());
                    callNPC3 = false;
                }
            }
        }
    }
}
