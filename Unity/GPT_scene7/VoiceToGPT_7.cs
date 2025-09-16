using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using TMPro;
using Unity.Mathematics;
using Oculus.Interaction.DebugTree;


















//wifi
public class VoiceToGPT_7 : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public OpenAITTS_7 tts;
    public subtitles_7 subtitles;
    public local_llm_7 llm;
    public historyinformation hf;//歷史紀錄腳本
    public AudioClip recordedClip;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public TextMeshProUGUI tmpText; // 語音字幕
    private Color defaultFaceColor; // 語音字幕的顏色
    private Color defaultOutlineColor; // 語音字幕的邊框顏色
    public AudioSource audioSource; // 音效播放器
    public AudioClip SuccessSound; // 成功音效
    private string audioFilePath;
    private bool isRecording = false;
    private const string WhisperApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    public string OpenAIKey;
    private string whisper_key;
    private const string GPTApiUrl = "https://tkuimaisvc.ethci.app/v1/chat/completions";
    public string userchat, gptchat, chatlog;
    public string round = "round1";
    public string suggetion = "suggestion";
    public string voice;
    public bool npc_trigger=false;//點擊NPC才能開始對話
    private List<Dictionary<string, string>> chatHistory = new List<Dictionary<string, string>>();
    private float timer=0f;  
    private float time_limit=60f;//錯誤計時
    private bool countdown=false;
    public float running_time=0f;//總時長計時
    public string status_prompt;
    public string status_tip;
    public double speed;
    public TextAsset json;
    public int context_7_error = 0; //錯誤次數
    //-------------finish word--------------
    public GameObject NPC1_finish_word;
    public GameObject NPC2_finish_word;
    public GameObject NPC3_finish_word;
    public GameObject NPC4_finish_word;
    public GameObject NPC5_finish_word;
    //-------------改正圖or影片--------------
    public GameObject NPC1_finish;
    public GameObject NPC3_image; //3-1
    public GameObject NPC3_wrong;
    public GameObject NPC4_image; //4-1
    public GameObject NPC4_wrong;
    public GameObject NPC5_image; //5-1
    public GameObject NPC5_wrong;
    public GameObject note_paper;
    public Shredder shredder; //碎紙機腳本
    public Context7_Congratulation1 Context7_Congratulation;
    public End_Control End_Control;
    public Phone_vibrates phone_Vibrates; //手機震動腳本
    public Red_Flash red_Flash;
    private int direction_num; // 方向(左邊或右邊)
    private int prompt_num; // prompt的數字
    float lastActionTime=0f;
    float inactivityThreshold = 10f;
    bool time_set = false;
    //-------------答對與否--------------
    private bool npc2=false;
    public bool npc1_strong_password = false; // 強密碼是否答出
    public bool npc1_take_care_password = false; // 妥善管理密碼是否答出
    private bool npc3 = false;
    private bool npc4 = false;
    private bool npc5 = false;
    void Start()
    {

        voice ="context7_0";
        speed = 0.75;
        status_prompt = Get_prompt(1);     
        status_tip = Get_tip(1);      
        audioFilePath = Path.Combine(Application.dataPath, "recorded_audio.wav");
        OpenAIKey = Get_api_key(1);
        whisper_key = Get_api_key(2);
        subtitles = FindObjectOfType<subtitles_7>();
        hf= FindObjectOfType<historyinformation>();
        tts = FindObjectOfType<OpenAITTS_7>();
        llm = FindObjectOfType<local_llm_7>();
        if (subtitles != null)
        {
            Debug.Log("sub已綁定");
        }
        else
        {
            Debug.Log("sub未綁定");
        }
        // 初始化音頻保存路徑
        // 等待 sub_and_tts 完成
        // yield return sub_and_tts("我這邊有檔案需要你處理，你接收一下");

        // 再啟動 NPC 動作
        // yield return npc1_movent();
       
    }

    void Update()
    {
        running_time += Time.deltaTime;//總時長增加
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (OVRInput.GetDown(OVRInput.Button.Two) && !npc_trigger)
            {
                _ = sub_and_tts("你要先點擊NPC才能開始對話");
                tts.PlayNextInQueue();
            }
           else if (!isRecording && OVRInput.GetDown(OVRInput.Button.Two))
            {
                if (!tts.Get_audio_status())
                {
                    StartRecording();
                }
                else
                {
                    Debug.Log("對話已取消，開始下次錄音");
                    stop_process();
                    StartRecording();
                }
            }

            // 停止录音并开始处理
            else if (isRecording && OVRInput.GetDown(OVRInput.Button.Two))
            {
                if (!tts.Get_audio_status())
                {
                    if (Microphone.IsRecording(null) && isRecording)
                    {
                        StopRecording();
                       
                        try
                        {
                            ProcessAudioToGPT();
                        }
                        catch (NullReferenceException ex)
                        {
                            Debug.LogError("NullReferenceException caught: " + ex.Message);
                        }

                    }
                    else
                    {
                        Debug.LogWarning("尚未錄音");
                    }
                }
                else
                {
                    Debug.Log("此對話尚未結束");
                }
            }
        }
        if (countdown)
        {
            timer += Time.deltaTime; // 每秒累加時間

            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                countdown = false;
                Debug.Log("按下了 R 鍵，重置計時器。");
                // 這裡可以加入玩家按下 R 後的邏輯
            }

            if (timer >= time_limit)
            {
                Debug.Log("20 秒內沒有按下 R 鍵！");
                // 這裡放 20 秒內未按下 R 的處理邏輯
                StartCoroutine(Want_answer());
                countdown = false;
            }
        }
    }
    //情境結束儲存歷史資料
    void OnApplicationQuit()
    {
        // 可以在這裡存檔、關閉網路連線等
    }
    // ----------------------------------------------------------------- 共用區 -----------------------------------------------------------------
    // 等待前一個語音講完再進下一個
    private IEnumerator WaitForTTSToFinish(Action onComplete)
    {
        while (tts.Get_audio_status())
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    // 向左轉或向右轉
    public IEnumerator Turn_Left_or_Right(String npc, int num, String text, int prompt_num)
    {
        voice_switch(prompt_num);//切換聲音
        _ = sub_and_tts(text);
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set(npc, num);
        status_prompt = Get_prompt(prompt_num);
        this.prompt_num = prompt_num;
        direction_num = num;
        npc_trigger = true; // 開放錄音
        countdown = true;
        yield return null;
    }

    // 玩家不知道答案並且想直接知道答案
    private IEnumerator Want_answer()
    {
      
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("｢你要不要再思考一下？\n還是我直接告訴你答案？｣");
        if (prompt_num == 1)
        {
            status_prompt = Get_prompt(6);
            prompt_num = 6;//暫時改6，以免再次載入函式
        }
        else if (prompt_num == 2)
        {
            status_prompt = Get_prompt(7);
            prompt_num = 7;//暫時改0，以免再次載入函式
        }
        else if (prompt_num == 3)
        {
            status_prompt = Get_prompt(8);
            prompt_num = 8;//暫時改8，以免再次載入函式
        }
        else if (prompt_num == 4)
        {
            status_prompt = Get_prompt(9);
            prompt_num = 9;//暫時改9，以免再次載入函式
        }
        else
        {
            status_prompt = Get_prompt(10);
            prompt_num = 10;//暫時改10，以免再次載入函式
        }
 
        tts.PlayNextInQueue();        
        yield return null;
    }

    private IEnumerator Check_which_place()
    {
        context_7_error++;
        if (prompt_num == 6) NPC1_finish_word.SetActive(true);
        else if (prompt_num == 7) NPC2_finish_word.SetActive(true);
        else if (prompt_num == 8) NPC3_finish_word.SetActive(true);
        else if (prompt_num == 9) NPC4_finish_word.SetActive(true);
        else if (prompt_num == 10) NPC5_finish_word.SetActive(true);
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Context7_Congratulation.Check_Finish_or_not();
        });
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record);
        hf.PostHistoryinformation(context_7_error, prompt_num-5, time_seconds);//回傳歷史紀錄
        context_7_error = 0;
    }

    public IEnumerator congratulation()
    {
         
        _ = sub_and_tts("恭喜通關\n請點擊左手搖桿的任意鍵以結束訓練");
        End_Control.enabled = true;
        yield return null;
    }

    // ----------------------------------------------------------------- NPC1 -----------------------------------------------------------------
    // 完全回答正確(NPC1)
    private IEnumerator NPC1_All_correct()
    {
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            NPC1_finish_word.SetActive(true);
            StartCoroutine(NPC1_All_correct_movement());
        });
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record);
        hf.PostHistoryinformation(context_7_error, 71, time_seconds);//回傳歷史紀錄
        context_7_error = 0;
      
    }

    private IEnumerator NPC1_All_correct_movement()
    {
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("為保障帳號安全，密碼不應隨意透露，避免資訊遭有心人士盜取\n並使用高強度密碼，以減少遭猜測或破解的可能。");
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 10); // 方位(21左、22右) + 10
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 20); // 方位(21左、22右) + 20
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", 5); // 撕便條紙
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        note_paper.SetActive(false); // 便條紙
        Variables.ActiveScene.Set("NPC_1_Status", 0); // npc1回打字動作
        // 改密碼
        tts.PlayNextInQueue();
        NPC1_finish.SetActive(true);
        npc1_strong_password = true;
        npc1_take_care_password = true;
        npc_trigger = false;
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            voice_switch(1);//切換聲音到npc1;
            Context7_Congratulation.Check_Finish_or_not();
        });
        
    }

    // 只對一個強密碼(npc1)
    private IEnumerator NPC1_strong_password_correct()
    {
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(5f);
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("你講得非常好\n密碼不應隨意透露，避免資訊遭有心人士盜取\n但還有其他的資安漏洞哦，再找找看吧！");
        voice_switch(1);//切換聲音到npc1;
        context_7_error++;//錯誤+1
        yield return WaitForTTSToFinish(() =>
        {
            StartCoroutine(NPC1_strong_password_correct_movement());
        });
    }

    private IEnumerator NPC1_strong_password_correct_movement()
    {
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 10); // 方位(21左、22右) + 10
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 20); // 方位(21左、22右) + 20
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", 0); // npc1回打字動作
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        // 改密碼影片
        tts.PlayNextInQueue();
        npc1_strong_password = true;
        yield return new WaitForSeconds(2f);
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_1_Status", -2);
        });
    }

    // 只對一個妥善管理密碼(npc1)
    private IEnumerator NPC1_take_care_password_correct()
    {
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(2f);
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("你講得非常好，應該使用高強度密碼，以減少遭猜測或破解的可能，好像還有其他的資安漏洞哦，再找找看吧！");
        voice_switch(1);//切換聲音到npc1;
        context_7_error++;//錯誤+1
        yield return WaitForTTSToFinish(() =>
        {
            StartCoroutine(NPC1_take_care_password_correct_movement());
        });
    }

    private IEnumerator NPC1_take_care_password_correct_movement()
    {
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 10); // 方位(21左、22右) + 10
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 20); // 方位(21左、22右) + 20
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", 5); // 撕便條紙
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", 0); // npc1回打字動作
        tts.PlayNextInQueue();
        npc1_take_care_password = true;
        yield return new WaitForSeconds(2f);
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_1_Status", -2);
        });
    }

    // 都答錯
    private IEnumerator NPC1_wrong_answer()
    {
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("好像不是這個問題哦，再想想看吧！");
        voice_switch(1);//切換聲音到npc1;
        Variables.ActiveScene.Set("NPC_1_Status", direction_num + 20); // 方位(21左、22右) + 20
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1) yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", 0); // npc1回打字動作
        tts.PlayNextInQueue();
        context_7_error++;//錯誤+1
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_1_Status", -2);
        });
    }
  
   
    // ----------------------------------------------------------------- NPC2 -----------------------------------------------------------------
    // 答對(npc2)
    private IEnumerator NPC2_correct_answer()
    {
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set("NPC_2_Status", direction_num+20);//表示驚訝
        while ((int)Variables.ActiveScene.Get("NPC_2_Status") != -1) yield return null;
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("機密文件一旦無需保存\n應立刻以碎紙機銷毀，以防資訊外洩\n切勿隨意棄置於垃圾桶。"); //垃圾桶要問教授
        yield return WaitForTTSToFinish(() =>
        {
            StartCoroutine(NPC2_correct_answer_movement());
        });
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record);
        hf.PostHistoryinformation(context_7_error, 72, time_seconds);//回傳歷史紀錄
        context_7_error = 0;
     
    }

    private IEnumerator NPC2_correct_answer_movement()
    {
        Variables.ActiveScene.Set("NPC_2_Status", direction_num + 30);
        while ((int)Variables.ActiveScene.Get("NPC_2_Status") != -2)
        {
            yield return null;
        }
        StartCoroutine(shredder.Run()); //碎紙機特效
        yield return new WaitForSeconds(5f); // 等碎紙機特效結束
        NPC2_finish_word.SetActive(true);
        tts.PlayNextInQueue();
        npc2=true;
        npc_trigger = false;
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Context7_Congratulation.Check_Finish_or_not();
        });
    }
    private IEnumerator NPC2_wrong_answer()
    {
        Variables.ActiveScene.Set("NPC_2_Status", direction_num + 10);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_2_Status") != -1) yield return null;
        context_7_error++;//錯誤+1
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set("NPC_2_Status", -2);
    }

    // ----------------------------------------------------------------- NPC3 -----------------------------------------------------------------
    private IEnumerator NPC3_correct_answer()
    {
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set("NPC_3_Status", direction_num + 10);//表示驚訝
        while ((int)Variables.ActiveScene.Get("NPC_3_Status") != -1) yield return null;
        voice_switch(0);//切換聲音到npc0;
        _ = sub_and_tts("工作內容屬於機密資訊，不得在社群網站公開\n或向非工作相關人員透露。");
        yield return WaitForTTSToFinish(() =>
        {
            NPC3_finish_word.SetActive(true);
            StartCoroutine(NPC3_correct_answer_movement());
        });
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record);
        hf.PostHistoryinformation(context_7_error, 73, time_seconds);//回傳歷史紀錄
        context_7_error = 0;
      
    }
    private IEnumerator NPC3_correct_answer_movement()
    {
        Variables.ActiveScene.Set("NPC_3_Status", direction_num + 20);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_3_Status") != -1) yield return null;
        tts.PlayNextInQueue();
        npc3 = true;
        npc_trigger = false;
        yield return WaitForTTSToFinish(() =>
        {
            Context7_Congratulation.Check_Finish_or_not();
        });
    }
    private IEnumerator NPC3_wrong_answer()
    {
        Variables.ActiveScene.Set("NPC_3_Status", direction_num + 20);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_3_Status") != -1) yield return null;
        NPC3_wrong.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        red_Flash.StartFlashing();
        yield return new WaitForSeconds(6f); // 等秒數(警報響完)
        tts.PlayNextInQueue();
        context_7_error++;//錯誤+1
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_3_Status", -2);
            NPC3_wrong.SetActive(false);
            NPC3_image.SetActive(true);
        });
    }
    // ----------------------------------------------------------------- NPC4 -----------------------------------------------------------------
    private IEnumerator NPC4_correct_answer()
    {
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set("NPC_4_Status", direction_num + 10);//微微點頭
        while ((int)Variables.ActiveScene.Get("NPC_4_Status") != -1) yield return null;
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            voice_switch(0);//切換聲音到npc0;
            _ = sub_and_tts("請勿在來路不明或未經驗證的網站輸入個人資料\n以避免資訊遭非法取得或外洩。");
            NPC4_finish_word.SetActive(true);
            StartCoroutine(NPC4_correct_answer_movement());
        });
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record);
        hf.PostHistoryinformation(context_7_error, 74, time_seconds);//回傳歷史紀錄
        context_7_error = 0;
    
    }
    private IEnumerator NPC4_correct_answer_movement()
    {
        Variables.ActiveScene.Set("NPC_4_Status", direction_num + 20);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_4_Status") != -1) yield return null;
        tts.PlayNextInQueue();
        npc4 = true;
        npc_trigger = false;
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Context7_Congratulation.Check_Finish_or_not();
        });
    }

    private IEnumerator NPC4_wrong_answer()
    {
        Variables.ActiveScene.Set("NPC_4_Status", direction_num + 20);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_4_Status") != -1) yield return null;
        NPC4_wrong.SetActive(true);
        red_Flash.StartFlashing();
        StartCoroutine(phone_Vibrates.ShakeCoroutine()); //手機震動
        yield return new WaitForSeconds(6f); // 等秒數(警報響完)
        tts.PlayNextInQueue();
        context_7_error++;//錯誤+1
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_4_Status", -2);
            NPC4_wrong.SetActive(false);
            NPC4_image.SetActive(true);
        });
    }
    // ----------------------------------------------------------------- NPC5 -----------------------------------------------------------------
    private IEnumerator NPC5_correct_answer()
    {
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set("NPC_5_Status", direction_num + 10);//一隻手比讚
        while ((int)Variables.ActiveScene.Get("NPC_5_Status") != -1) yield return null;
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            voice_switch(0);//切換聲音到npc0;
            _ = sub_and_tts("請勿輕信或開啟任何來自陌生帳號的訊息、檔案或連結\n以防遭受詐騙、惡意程式或其他安全威脅。");
            NPC5_finish_word.SetActive(true);
            StartCoroutine(NPC5_correct_answer_movement());
        });
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record);
        hf.PostHistoryinformation(context_7_error, 75, time_seconds);//回傳歷史紀錄
        context_7_error = 0;
       
    }
    private IEnumerator NPC5_correct_answer_movement()
    {
        Variables.ActiveScene.Set("NPC_5_Status", direction_num + 20);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_5_Status") != -1) yield return null;
        tts.PlayNextInQueue();
        npc5 = true;
        npc_trigger = false;
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            Context7_Congratulation.Check_Finish_or_not();
        });
    }
    private IEnumerator NPC5_wrong_answer()
    {
        Variables.ActiveScene.Set("NPC_5_Status", direction_num + 20);//轉回座位
        while ((int)Variables.ActiveScene.Get("NPC_5_Status") != -1) yield return null;
        NPC5_wrong.SetActive(true);
        red_Flash.StartFlashing();
        yield return new WaitForSeconds(6f); // 等秒數(警報響完)
        tts.PlayNextInQueue();
        context_7_error++;//錯誤+1
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_5_Status", -2);
            NPC5_wrong.SetActive(false);
            NPC5_image.SetActive(true);
        });
    }
    //-------------------------------------------------------------------------------------------------------------------------------------------------
  
    void stop_process()
    {
        tts.audioSource.Stop();
        subtitles.CancelOperation();
    }

    public void StartRecording()
    {

        if (!isRecording)
        {
            Debug.Log("開始錄音...");
            recordedClip = Microphone.Start(null, false, 10, 16000);
            isRecording = true;
        }


        // 切換按鈕狀態

    }

    // 停止錄音並保存為 .wav 文件
    public void StopRecording()
    {
        Microphone.End(null);
        SaveAudioClipToFile(recordedClip, audioFilePath);
        Debug.Log($"錄音停止 檔案將儲存到 {audioFilePath}");
        isRecording = false;
        // 切換按鈕狀態     
    }
    private void SaveAudioClipToFile(AudioClip clip, string filePath)
    {
        if (clip == null) return;

        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            WriteWavFile(fileStream, clip, samples);
        }
    }

    private void WriteWavFile(Stream stream, AudioClip clip, float[] samples)
    {
        int headerSize = 44;
        int fileSize = headerSize + samples.Length * 2;

        using (var writer = new BinaryWriter(stream))
        {
            // 寫入 WAV 文件頭
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(fileSize - 8);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1); // PCM 格式
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16); // 每個樣本的位深度
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(samples.Length * 2);

            // 寫入音頻數據
            foreach (var sample in samples)
            {
                short intSample = (short)(sample * short.MaxValue);
                writer.Write(intSample);
            }
        }
    }
    //錄音

    public void TranscribeAudio(string filePath, System.Action<string> callback)
    {
        StartCoroutine(SendWhisperRequest(filePath, callback));
    }
    IEnumerator SendWhisperRequest(string filePath, System.Action<string> callback)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        var form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filePath), "audio/wav");
        form.AddField("model", "gpt-4o-transcribe");
        form.AddField("language", "zh");
        using (UnityWebRequest request = UnityWebRequest.Post(WhisperApiUrl, form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + whisper_key);

            request.uploadHandler = new UploadHandlerRaw(form.data);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                responseText = Content_of_whisper(responseText);
                userchat = responseText;

                Debug.Log("Whisper Response: " + responseText);
                callback?.Invoke(responseText); // 返回文本
            }
            else
            {
                Debug.LogError("Whisper API Error: " + request.error);
                Debug.LogError("Response Body: " + request.downloadHandler.text);
                callback?.Invoke(null);
            }
        }
    }
    //用將聲音傳至whisper

    public void GenerateResponse(string prompt,string system_prompt, System.Action<string> callback)
    {
        StartCoroutine(SendGPTRequest(prompt,  system_prompt, callback));
    }

    IEnumerator SendGPTRequest(string prompt, string system_prompt, System.Action<string> callback)
    {
        var requestData = new
        {
            model = "gpt-4o",
            messages = new[]
        {
            new { role = "system", content = system_prompt },
            new { role = "user", content = prompt },
        },
            max_tokens = 2048,
            temperature = 0.5,
            top_p = 0,
            frequency_penalty = 0,
            presence_penalty = 0
        };

        string jsonData = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest(GPTApiUrl, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + OpenAIKey);

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                responseText = Content_of_gptreply(responseText);
                gptchat = responseText;
                callback?.Invoke(responseText);
            }
            else
            {
                Debug.LogError("GPT API Error: " + request.error);
                callback?.Invoke(null);
            }
        }
    }

    private string Get_api_key(int key)
    {
        string APIPath = Path.Combine(Application.streamingAssetsPath, "api_key.json");
        string jsonContent = File.ReadAllText(APIPath);
        // 解析 JSON 並取得 API Key
        JObject config = JObject.Parse(jsonContent);
        if (key == 1)
        {
            return (string)config["API_Key"];
        }
        else if (key == 2)
        {
            return (string)config["whisper_API"];
        }
        return (string)config["API_Key"];

    }
    private string Get_prompt(int index)
    {
        string jsonContent = json.text;
        // 解析 JSON 並取得 API Key
        JObject config = JObject.Parse(jsonContent);
        if (index == 0)
        {
            return (string)config["suggestion"];
        }
        else if (index == 1)
        {
            return (string)config["NPC1"];
        }
        else if (index == 2)
        {
            return (string)config["NPC2"];
        }
        else if (index == 3)
        {
            return (string)config["NPC3"];
        }
        else if (index == 4)
        {
        
            return (string)config["NPC4"];
        }
        else if (index == 5)
        {
            return (string)config["NPC5"];
        }
        else if (index == 6)
        {
            return (string)config["NPC1_tip"];
        }
        else if (index == 7)
        {
            return (string)config["NPC2_tip"];
        }
        else if (index == 8)
        {
            return (string)config["NPC3_tip"];
        }
        else if (index == 9)
        {
            return (string)config["NPC4_tip"];
        }
        else if (index == 10)
        {
            return (string)config["NPC5_tip"];
        }
        else
        {
            return "無效的prompt";
        }

    }
    private string Get_tip(int index)
    {
        string jsonContent = json.text;
        JObject config = JObject.Parse(jsonContent);
        if(index == 1)
        {
            return (string)config["tip1"];
        }
        else if (index == 2)
        {
            return (string)config["tip2"];
        }
        else if (index == 3)
        {
            return (string)config["tip3"];
        }
        else
        {
            return "無效的prompt";
        }
    }
    public string Content_of_gptreply(string reply)
    {
        JObject json = JObject.Parse(reply);

        // 導航到 choices[0].message.content
        string content = (string)json["choices"][0]["message"]["content"];
        return content;
    }
    public string Content_of_whisper(string reply)
    {
        JObject json = JObject.Parse(reply);

        // 導航到 choices[0].message.content
        string content = (string)json["text"];

        return content;
    }
    public string Get_userchat()
    {
        return userchat;
    }
    public string Get_gptchat()
    {
        return gptchat;
    }
    public void Set_scriptgraph(string keyword)
    {
        //偵測回答不知道的回應內容，以偵測是否完成
        if (prompt_num==6)
        {
            Variables.ActiveScene.Set("NPC_1_Status", 1);//切回初始動作
        }
        if (prompt_num == 7)
        {
            Variables.ActiveScene.Set("NPC_2_Status", 1);//切回初始動作
        }
        if (prompt_num == 8)
        {
            Variables.ActiveScene.Set("NPC_3_Status", 1);//切回初始動作
        }
        if (prompt_num == 9)
        {
            Variables.ActiveScene.Set("NPC_4_Status", 1);//切回初始動作
        }
        if (prompt_num == 10)
        {
            Variables.ActiveScene.Set("NPC_5_Status", 1);//切回初始動作
        }
       
    }
    public void voice_switch(int voiceIndex)
    {
        switch (voiceIndex)
        {    
            case 0:
                this.voice = "context7_0";
                speed = 1.1;
                break;
            case 1:
                this.voice = "context7_1";
                speed = 0.75;
                break;
            case 2:
                this.voice = "context7_2";
                speed = 1.3;
                break;
            case 3:
                this.voice = "context7_3";
                speed = 1.5;
                break;
            case 4:
                this.voice = "context7_4";
                speed = 0.9;
                break;
            case 5:
                this.voice = "context7_5";
                speed = 1.1;
                break;
            default:               
                this.voice = "context7_1";
                break;
        }

        Debug.Log($"已切換語音為：{this.voice}");
    }
   

    public async Task sub_and_tts(string keyword, double? setspeed = null)
    {
        // 如果 setspeed 有值，用它；否則用原本的 speed
        double finalSpeed = setspeed.HasValue ? setspeed.Value : speed;
        await tts.f5tts(keyword,voice,finalSpeed);
        // 假設 f5tts 是一個返回 Task 的非同步方法
        Debug.Log(keyword);          // 假設這是同步方法
    }    
    //取得變數
    void ProcessAudioToGPT()
    {
        // 语音转文本
        TranscribeAudio(audioFilePath, (transcribedText) =>
        {
            if (!string.IsNullOrEmpty(transcribedText))
            {
                chatHistory.Add(new Dictionary<string, string> { { "role", "user" }, { "content", transcribedText } });
                //subtitles.ShowSubtitlesInSegments(transcribedText);
                // GPT 生成回复
                GenerateResponse(transcribedText, status_prompt, async (gptResponse) =>
                {
                    if (string.IsNullOrEmpty(gptResponse))
                    {
                        Debug.LogError("GPT Response is null or empty.");
                        return;
                    }
                    else if (gptResponse.Contains("不知道"))
                    {
                        StartCoroutine(Want_answer());
                    }
                    else if (prompt_num == 1)
                    {
                        NPC1(gptResponse);
                    }
                    else if (prompt_num == 2)
                    {
                        NPC2(gptResponse);
                    }
                    else if (prompt_num == 3)
                    {
                        NPC3(gptResponse);
                    }
                    else if (prompt_num == 4)
                    {
                        NPC4(gptResponse);
                    }
                    else if (prompt_num == 5)
                    {
                        NPC5(gptResponse);
                    }
                    else
                    {
                        await tts.f5tts(gptResponse, voice, speed);
                        Set_scriptgraph(gptResponse);
                        if (gptResponse.Contains("不然我們等下再回來看看吧！"))
                        {
                           
                            tts.PlayNextInQueue();
                            int NPC_num = prompt_num-5;
                            Variables.ActiveScene.Set("NPC_"+ NPC_num +"_Status", -2);
                        }
                        else if (gptResponse.Contains("請再說一次"))
                        {
                            tts.PlayNextInQueue();
                        }
                        else
                        {
                            StartCoroutine(Check_which_place());
                        }
                        Debug.Log(gptResponse);
                    }
                });
            }
        });
    }

    private void NPC1(String gptResponse)
    {
        Debug.Log(gptResponse);
        if (gptResponse.Contains("我會改進的") || (npc1_take_care_password && gptResponse.Contains("我之後會把它改掉")) || (npc1_strong_password && gptResponse.Contains("我立刻處理掉")))
        {
            _ = sub_and_tts("真的很抱歉，我會改進的");
            StartCoroutine(NPC1_All_correct());
        }
        else if (!npc1_strong_password && gptResponse.Contains("我之後會把它改掉"))
        {
            _ = sub_and_tts("真的很抱歉，我之後會把它改掉");
            StartCoroutine(NPC1_strong_password_correct());
        }
        else if (!npc1_take_care_password && gptResponse.Contains("我立刻處理掉"))
        {
            _ = sub_and_tts("真的很抱歉，我立刻處理掉");
            StartCoroutine(NPC1_take_care_password_correct());
        }
        else if (gptResponse.Contains("回答錯誤"))
        {
            StartCoroutine(NPC1_wrong_answer());
        }
    }

    private void NPC2(String gptResponse)
    {
        if (gptResponse.Contains("對耶我沒有注意到，謝謝提醒！"))
        {
            _ = sub_and_tts("對耶我沒有注意到，謝謝提醒！");
            StartCoroutine(NPC2_correct_answer());
        }
        else if (gptResponse.Contains("回答錯誤"))
        {
            voice_switch(0);//切換聲音到npc0;
            _ = sub_and_tts("好像不是這個問題哦，再想想看吧！");
            voice_switch(2);//切換聲音到npc2;
            StartCoroutine(NPC2_wrong_answer());
        }
        else
        {
            _ = tts.f5tts(gptResponse, voice, speed);
            tts.PlayNextInQueue();
            Debug.Log(gptResponse);
        }
    }

    private void NPC3(String gptResponse)
    {
        if (gptResponse.Contains("不能發啊？那我現在把它刪掉，下次我會注意！"))
        {
            _ = sub_and_tts("不能發啊？那我現在把它刪掉\n下次我會注意！");
            StartCoroutine(NPC3_correct_answer());
        }
        else if (gptResponse.Contains("回答錯誤"))
        {
            voice_switch(0);//切換聲音到npc0;
            _ = sub_and_tts("好像不是這個問題哦，再想想看吧！");
            voice_switch(3);//切換聲音到npc3;
            StartCoroutine(NPC3_wrong_answer());
        }
        else
        {
            _ = tts.f5tts(gptResponse, voice, speed);
            tts.PlayNextInQueue();
            Debug.Log(gptResponse);
        }
    }

    private void NPC4(String gptResponse)
    {
        if (gptResponse.Contains("謝謝提醒，我現在把它關掉。"))
        {
            _ = sub_and_tts("謝謝提醒，我現在把它關掉。");
            StartCoroutine(NPC4_correct_answer());
        }
        else if (gptResponse.Contains("回答錯誤"))
        {
            voice_switch(0);//切換聲音到npc0;
            _ = sub_and_tts("好像不是這個問題哦，再想想看吧！");
            voice_switch(4);//切換聲音到npc4;
            StartCoroutine(NPC4_wrong_answer());
        }
        else
        {
            _ = tts.f5tts(gptResponse, voice, speed);
            tts.PlayNextInQueue();
            Debug.Log(gptResponse);
        }
    }

    private void NPC5(String gptResponse)
    {
        if (gptResponse.Contains("了解，我馬上刪除郵件並封鎖這個用戶。"))
        {
            _ = sub_and_tts("了解，我馬上刪除郵件並封鎖這個用戶。");
            StartCoroutine(NPC5_correct_answer());
        }
        else if (gptResponse.Contains("回答錯誤"))
        {
            voice_switch(0);//切換聲音到npc0;
            _ = sub_and_tts("好像不是這個問題哦，再想想看吧！");
            voice_switch(5);//切換聲音到npc5;
            StartCoroutine(NPC5_wrong_answer());
        }
        else
        {
            _ = tts.f5tts(gptResponse, voice, speed);
            tts.PlayNextInQueue();
            Debug.Log(gptResponse);
        }
    }

}



