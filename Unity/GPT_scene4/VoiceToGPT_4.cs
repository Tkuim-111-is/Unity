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












//wifi
public class VoiceToGPT_4 : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public OpenAITTS_4 tts;
    public subtitles_4 subtitles;
    public local_llm_4 llm;
    public AudioClip recordedClip;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public getting_wifi getting_Wifi;
    public NPC_touch_Context4 NPC_touch;
    public ProgressBar progressBar;
    public Start_Button startButton;
    public TextMeshProUGUI tmpText; // 語音字幕
    private Color defaultFaceColor; // 語音字幕的顏色
    private Color defaultOutlineColor; // 語音字幕的邊框顏色
    public AudioSource audioSource;
    public AudioClip SuccessSound; // 成功音效
    private string audioFilePath;
    public historyinformation hf;//歷史紀錄腳本
    public AudioClip FailSound; // 失敗音效
    public GameObject password;
    private bool isRecording = false;
    private const string WhisperApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    public string OpenAIKey;
    public string whisper_key;
    private const string GPTApiUrl = "https://tkuimaisvc.ethci.app/v1/chat/completions";
    public string userchat, gptchat, chatlog;
    public string round = "round1";
    public string suggetion = "suggestion";
    public string voice;
    private List<Dictionary<string, string>> chatHistory = new List<Dictionary<string, string>>();
    private float startTime;  // 開始時間
    private float endTime;
    public string status_prompt;
    public string status_tip;
    public int npc_status1;
    public int npc_status2;
    public int npc_status3;
    public double speed;
    public TextAsset json;
    public bool remind;
    public bool npc1_lock;
    public bool npc2_lock;
    public bool npc3_lock;
    public bool npc2_fin;
    float lastActionTime=0f;
    float inactivityThreshold = 10f;
    bool time_set = false;
    private bool npc1_active_1 = true; // 說"怎麼了嗎"的鑰匙
    private bool npc2_active_1 = true; // 說"你好，有甚麼可以幫忙你的嗎"的鑰匙
    public int context_error = 0; //錯誤次數
    public float running_time = 0f;//總時長計時
    public End_control_context4 End_Control;
    IEnumerator Start()
    {
        npc1_lock = false;
        npc2_lock = false;
        npc3_lock = true;
        npc2_fin=false;
        voice = "context4_1";
        remind = false;
        npc_status1 = 1;
        npc_status2 = 3;
        npc_status3 = 3;
        speed = 0.75;
        status_prompt = Get_prompt(1);     
        status_tip = Get_tip(1);      
        audioFilePath = Path.Combine(Application.dataPath, "recorded_audio.wav");
        OpenAIKey = Get_api_key(1);
        whisper_key = Get_api_key(2);
        subtitles = FindObjectOfType<subtitles_4>();
        tts = FindObjectOfType<OpenAITTS_4>();
        llm = FindObjectOfType<local_llm_4>();
        hf = FindObjectOfType<historyinformation>();
        if (subtitles != null)
        {
            Debug.Log("sub已綁定");
        }
        else
        {
            Debug.Log("sub未綁定");
        }
        // 初始化音頻保存路徑
        //_ = sub_and_tts("我這邊有檔案需要你處理，你接收一下");
        //StartCoroutine(npc1_movent());
        // 等待 sub_and_tts 完成
        yield return sub_and_tts("我這邊有檔案需要你處理，你接收一下");

        // 再啟動 NPC 動作
        yield return npc1_movent();
    }

    void Update()
    {
        running_time += Time.deltaTime;//總時長增加
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (!isRecording && OVRInput.GetDown(OVRInput.Button.Two))
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
                        startTime = Time.time;
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
        if (npc2_active_1 &&  (int)Variables.ActiveScene.Get("NPC_2_Status") == 1)
        {
            voice_switch(2);
            _ = sub_and_tts("你好，有甚麼可以幫忙你的嗎");
            tts.PlayNextInQueue();
            npc2_active_1 = false;
            status_prompt = Get_prompt(3);
        }
        if (npc1_active_1 && (int)Variables.ActiveScene.Get("NPC_1_Status") == 1)
        {       
            _ = sub_and_tts("怎麼了嗎?");
            tts.PlayNextInQueue();
            npc1_active_1 = false;
            status_prompt = Get_prompt(1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRecording();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopRecording();
            ProcessAudioToGPT();
        }
    }

    // 等待前一個語音講完再進下一個
    private IEnumerator WaitForTTSToFinish(Action onComplete)
    {
        while (tts.Get_audio_status())
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    private IEnumerator npc1_movent()
    {

        tts.PlayNextInQueue();
        yield return new WaitForSeconds(5f);
        Variables.ActiveScene.Set("NPC_1_Status", 2);
        yield return new WaitForSeconds(3f);
        Variables.ActiveScene.Set("NPC_1_Status", 0);
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1)
            yield return null;
        Variables.ActiveScene.Set("NPC_1_Status", -2);
        NPC_touch.enabled = true;
        npc1_active_1 = true;
    }
    private IEnumerator npc2_movent()
    {
        Variables.ActiveScene.Set("NPC_2_Status", 2);
        tts.PlayNextInQueue();
        password.SetActive(true);
        yield return new WaitForSeconds(5f);
        /*yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_2_Status", 3);
        });*/
        Variables.ActiveScene.Set("NPC_2_Status", 3);
        yield return new WaitForSeconds(3f);
        Variables.ActiveScene.Set("NPC_2_Status", 0);
        yield return new WaitForSeconds(2f);
        Variables.ActiveScene.Set("NPC_2_Status", -2);
        npc2_active_1 = true;
    }

    // 點選錯誤網路
    public IEnumerator Choose_wrong_wifi()
    {
        voice_switch(4); // 切換生氣語氣
        _ = sub_and_tts("怎麼會用無密碼的wifi，你看我給你的檔案被偷了吧");//撥放張做成罵人
        audioSource.PlayOneShot(FailSound);
        yield return new WaitForSeconds(FailSound.length);
        Material mat = tmpText.fontMaterial;
        mat.SetColor("_FaceColor", Color.red); // 設定字為紅色
        mat.SetColor("_OutlineColor", Color.red); // 設定邊為紅色
        tts.PlayNextInQueue();
        Variables.ActiveScene.Set("NPC_1_Status", 3);
        context_error++;//錯誤+1
        // 之後允許了話動作還需要轉1-5
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1)
            yield return null;
        yield return new WaitForSeconds(2f);
        yield return WaitForTTSToFinish(() =>
        {
            voice_switch(1);
            StartCoroutine(Correct_talk());
        });
    }

    private IEnumerator Correct_talk()
    {
        Material mat = tmpText.fontMaterial;
        _ = sub_and_tts("我再把檔案傳給你一次，這次使用安全網路下載");
        yield return new WaitForSeconds(1f);
        getting_Wifi.do_again();
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(3f);
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_1_Status", 4);
            progressBar.enabled = false;
            progressBar.enabled = true;
            startButton.enabled = false;
            startButton.enabled = true;
            mat.SetColor("_FaceColor", Color.white); // 設定字為白色
            mat.SetColor("_OutlineColor", Color.black); // 設定邊為黑色
            // 切換原本的語氣
        });
    }
    // 成功通關該講的話
    public IEnumerator Correct_PDF()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("共錯了" + context_error +"次");
        hf.PostHistoryinformation(context_error,4,time_seconds);
        _ = sub_and_tts("幹的好!!!\n記得不安全的網路不能亂連以免檔案遺失喔~");
        yield return new WaitForSeconds(1f);
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(5f);
        yield return WaitForTTSToFinish(() =>
        {
            audioSource.PlayOneShot(SuccessSound);
            StartCoroutine(Change_back_to_Choose_Object());
        });
    }
    // 成功通關後延遲4秒切還大情境繼續尋找物件
    private IEnumerator Change_back_to_Choose_Object()
    {
        // yield return new WaitForSeconds(4f);
        End_Control.enabled = true;
        yield return null;
    }

    private void OnApplicationQuit()
    {
        SaveToJson();
        Debug.Log("Unity 關閉，正在儲存對話紀錄...");
    }

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
            return (string)config["prompt1"];
        }
        else if (index == 2)
        {
            return (string)config["prompt2"];
        }
        else if (index == 3)
        {
            return (string)config["prompt3"];
        }
        else if (index == 4)
        {
        
            return (string)config["prompt4"];
        }
        else if (index == 5)
        {
            return (string)config["prompt5"];
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

        if (keyword.Contains("你在說甚麼?是要問WIFI密碼嗎?"))
        {
            status_prompt = Get_prompt(2);         
            Debug.Log("已切換prompt");
           
        }
        if (keyword.Contains("我不清楚你在說甚麼，是要問WIFI密碼嗎?"))
        {
            status_prompt = Get_prompt(4);
            Debug.Log("已切換prompt");
        }
    }
    public void voice_switch(int voiceIndex)
    {
        switch (voiceIndex)
        {
            case 1:
                this.voice = "context4_1";
                break;
            case 2:
                this.voice = "context4_2";
                break;
            case 3:
                this.voice = "context4_3";
                break;
            case 4:
                this.voice = "context4_1_scold";
                break;
            default:               
                this.voice = "context4_1";
                break;
        }

        Debug.Log($"已切換語音為：{this.voice}");
    }


    public async Task sub_and_tts(string keyword, double? setspeed = null)
    {
        // 如果 setspeed 有值，用它；否則用原本的 speed
        double finalSpeed = setspeed.HasValue ? setspeed.Value : speed;

        await tts.f5tts(keyword, voice, finalSpeed);
        Debug.Log($"已將語音加入緩存隊列：{keyword}");
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
                            else if ((int)Variables.ActiveScene.Get("NPC_1_Status") == -2 && (int)Variables.ActiveScene.Get("NPC_2_Status") == -2)
                            {
                                _ = sub_and_tts("請先點擊NPC以進行對話");
                                tts.PlayNextInQueue();
                            }
                            else if (gptResponse.Contains("我忘記密碼是什麼了，問問看店員吧"))
                            {
                                tts.f5tts(gptResponse, voice, speed);
                                StartCoroutine(npc1_movent());
                            }
                            else if (gptResponse.Contains("密碼在那裡"))
                            {
                                tts.f5tts(gptResponse, voice, speed);
                                StartCoroutine(npc2_movent());
                            }
                            else
                            {
                                await tts.f5tts(gptResponse, voice, speed);
                                Set_scriptgraph(gptResponse);
                                tts.PlayNextInQueue();
                                Debug.Log(gptResponse);
                                endTime = Time.time;

                            }
                        });
                    }
                });
        }
        void SaveToJson()
        {
            string filePath = Path.Combine(Application.dataPath, "whiteboard_dataset.jsonl");
            var chatEntry = new { messages = chatHistory };
            // 將新的 chatHistory 轉換成 JSON
            string newEntry = JsonConvert.SerializeObject(chatEntry, Formatting.None);
            // 追加到檔案（換行確保 JSONL 格式）
            try
            {
                File.AppendAllText(filePath, newEntry + "\n");
            }
            catch (Exception ex)
            {
                Debug.LogError("寫入 JSONL 失敗：" + ex.Message);
            }

            chatHistory.Clear();
        }
        //儲存對話紀錄
    }



