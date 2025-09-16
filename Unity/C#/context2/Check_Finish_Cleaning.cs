using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Check_Finish_Cleaning : MonoBehaviour
{
    public Transform player; // 玩家
    public OVRRayHelper rayHelper; // 玩家的右手
    public LayerMask objectLayer;  // 右手上的雷射
    public GameObject Check_Canvas; // 確認畫面
    public GameObject Document1; // 桌上文件1
    public GameObject Document2; // 桌上文件2
    public GameObject Document3; // 櫃上文件
    public GameObject Laptop; // 筆電
    public VoiceToGPT_Try VoiceToGPT_Try; // 語音連接動作邏輯腳本
    public camaramove camaramove; // 移動的腳本
    public GameObject Congratulation; // 結算恭喜畫面
    public AudioSource audioSource;
    public AudioClip SuccessSound; // 成功音效
    public AudioClip ChineseSound; // 燈燈燈燈燈燈燈~
    public AudioClip FinishSound; // 結束提示詞音效
    public GameObject door; // 該死的門檔我按鈕
    private bool isShowingCheckCanvas = false;
    public historyinformation hf;//歷史紀錄腳本
    public End_control_context2 End_Control;
    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (!isShowingCheckCanvas && Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag("Door") && OVRInput.GetDown(OVRInput.Button.One))
            {
                door.GetComponent<BoxCollider>().enabled = false;
                Check_Canvas.SetActive(true);
                isShowingCheckCanvas = true;
            }
        }
    }

    public void Finish_Cleaning()
    {
        door.GetComponent<BoxCollider>().enabled = true;
        Check_Canvas.SetActive(false);
        isShowingCheckCanvas = false;
        Variables.ActiveScene.Set("Meeting_Door_Status", 1);
        if(Document1.activeSelf || Document2.activeSelf)
        {
            // 切換生氣語氣
            StartCoroutine(VoiceToGPT_Try.npc1_is_not_clean("你連桌上的資料都沒收，怎麼能隨意放我們進來", "如果需要幫忙，可以開門找我們"));
        }
        else if (Laptop.activeSelf)
        {
            camaramove.enabled = false; // 鎖定移動，不讓你動
            player.position = new Vector3(-18.66639f, 7.05f, 52.88611f);
            player.rotation = Quaternion.Euler(0f, 104f, 0f);
            StartCoroutine(VoiceToGPT_Try.Did_not_finish_cleaning(14, "你的筆電螢幕沒關，不應該讓無關者看到\n收好再叫我"));
        }
        else if (Document3.activeSelf)
        {
            camaramove.enabled = false;
            player.position = new Vector3(-18.66639f, 7.05f, 52.88611f);
            player.rotation = Quaternion.Euler(0f, 104f, 0f);
            StartCoroutine(VoiceToGPT_Try.Did_not_finish_cleaning(13, "你的文件還留在會議室中，不應該讓無關者進來\n收好再叫我"));
        }
        else
        {
            player.position = new Vector3(-18.66639f, 7.05f, 52.88611f);
            player.rotation = Quaternion.Euler(0f, 104f, 0f);
            Variables.ActiveScene.Set("NPC_1_Status", 9);
            StartCoroutine(Finish_movement());
        }
        Debug.Log("執行了");
    }

    // 等NPC1執行完1_9的動作
    private IEnumerator Finish_movement()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(VoiceToGPT_Try.running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record + "秒");
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1)
        {
            yield return null;
        }
        Congratulation.SetActive(true);
        // audioSource.PlayOneShot(SuccessSound);
        StartCoroutine(Change_back_to_Choose_Object());
        hf.PostHistoryinformation(VoiceToGPT_Try.context_error, 2,time_seconds);
        Debug.Log("錯了"+ VoiceToGPT_Try.context_error+"次");
    }

    // 等待4秒傳送回大情境
    private IEnumerator Change_back_to_Choose_Object()
    {
        audioSource.PlayOneShot(ChineseSound);
        yield return new WaitForSeconds(ChineseSound.length);
        audioSource.PlayOneShot(FinishSound);
        yield return new WaitForSeconds(FinishSound.length);
        // yield return new WaitForSeconds(4f);
        // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        End_Control.enabled = true;
    }

    public void Not_Yet()
    {
        door.GetComponent<BoxCollider>().enabled = true;
        Check_Canvas.SetActive(false);
        isShowingCheckCanvas = false;
        Debug.Log("執行了，Canvas 狀態：" + Check_Canvas.activeSelf);
    }
}
