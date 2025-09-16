using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Context7_Congratulation1 : MonoBehaviour
{
    public GameObject NPC1_finish;
    public GameObject NPC2_finish;
    public GameObject NPC3_finish;
    public GameObject NPC4_finish;
    public GameObject NPC5_finish;
    public GameObject context7_congratulation;
    public VoiceToGPT_7 voiceToGPT;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Check_Finish_or_not()
    {
        if(NPC1_finish.activeSelf && NPC2_finish.activeSelf && NPC3_finish.activeSelf && NPC4_finish.activeSelf && NPC5_finish.activeSelf)
        {
            context7_congratulation.SetActive(true);
            StartCoroutine(voiceToGPT.congratulation());
        }
    }
}
