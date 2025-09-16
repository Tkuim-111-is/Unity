using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SetActive : MonoBehaviour
{
    //---------------VoiceToGPT---------------
    public GameObject NPC1_finish_word;
    public GameObject NPC2_finish_word;
    public GameObject NPC3_finish_word;
    public GameObject NPC4_finish_word;
    public GameObject NPC5_finish_word;
    //---------------NPC1的物件---------------
    public GameObject NPC1_start;
    public GameObject NPC1_video; // 影片
    public GameObject NPC1_video_end;
    public GameObject NPC1_finish;
    //---------------NPC2的物件---------------
    public GameObject machineLoop;
    //---------------NPC3的物件---------------
    public GameObject NPC3_start;
    public GameObject NPC3_video; // 影片
    public GameObject NPC3_video_end;
    public GameObject NPC3_wrong;
    //---------------NPC4的物件---------------
    public GameObject NPC4_start;
    public GameObject NPC4_Wrong_video; // 影片
    public GameObject NPC4_Wrong_video_end;
    public GameObject NPC4_phone; // 手機畫面
    //---------------NPC5的物件---------------
    public GameObject NPC5_start;
    public GameObject NPC5_end;
    //---------------其他---------------------
    public GameObject context7_congratulation;
    public End_Control End_Control;

    void Start()
    {
        //---------------VoiceToGPT---------------
        NPC1_finish_word.SetActive(false);
        NPC2_finish_word.SetActive(false);
        NPC3_finish_word.SetActive(false);
        NPC4_finish_word.SetActive(false);
        NPC5_finish_word.SetActive(false);
        //---------------NPC1的物件---------------
        NPC1_start.SetActive(true); // 起始圖
        NPC1_video.SetActive(false);
        NPC1_video_end.SetActive(false);
        NPC1_finish.SetActive(false);
        //---------------NPC2的物件---------------
        machineLoop.SetActive(false);
        //---------------NPC3的物件---------------
        NPC3_start.SetActive(true);
        NPC3_video.SetActive(false);
        NPC3_video_end.SetActive(false);
        NPC3_wrong.SetActive(false);
        //---------------NPC4的物件---------------
        NPC4_start.SetActive(true);
        NPC4_Wrong_video.SetActive(false);
        NPC4_Wrong_video_end.SetActive(false);
        NPC4_phone.SetActive(false);
        //---------------NPC5的物件---------------
        NPC5_start.SetActive(true);
        NPC5_end.SetActive(false);
        //---------------其他---------------------
        context7_congratulation.SetActive(false);
        End_Control.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
