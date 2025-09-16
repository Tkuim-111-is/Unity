using System.Collections;
using System.Collections.Generic;
using Dreamteck;
using Unity.VisualScripting;
using UnityEngine;

public class CheckOpenTime : MonoBehaviour
{
    public RandomErrorPop randomerror;
    public ExplosionController explosionController;
    public GameObject errorplane;
    public GameObject errorCanvas;
    public GameObject usbFull;
    public GameObject NPC2_coordinate;
    public GameObject NPC3_coordinate;
    //public AudioSource explosionAudio;

    private int lastNPC2Status = -1;
    private int lastNPC3Status = -1;
    private bool hasActivated = false;
    private bool NPC2_hastouch = false;
    private bool NPC3_hastouch = false;
    void Update()
    {
        int currentNPC2Status = (int)Variables.ActiveScene.Get("NPC_2_Status");
        int currentNPC3Status = (int)Variables.ActiveScene.Get("NPC_3_Status");

        if(!NPC2_hastouch && (int)Variables.ActiveScene.Get("NPC_1_Status") == 2)
        {
            NPC2_hastouch = true;
            NPC2_coordinate.SetActive(true);
        }

        if(!NPC3_hastouch && (int)Variables.ActiveScene.Get("NPC_2_Status") == 4)
        {
            NPC3_hastouch = true;
            NPC3_coordinate.SetActive(true);
        }

        if (lastNPC2Status != 3 && currentNPC2Status == 3)
        {
            errorplane.SetActive(true);
            errorCanvas.SetActive(true);
            randomerror.StartCoroutine("SpawnErrors");
        }
        lastNPC2Status = currentNPC2Status;

        if (!hasActivated && currentNPC3Status == 2)
        {
            hasActivated = true;
            usbFull.SetActive(true);
        }

        lastNPC3Status = currentNPC3Status;
    }
    
}
