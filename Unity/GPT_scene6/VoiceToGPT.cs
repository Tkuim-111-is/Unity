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
using UnityEngine.SceneManagement;
using TMPro;



//隨身碟
public class VoiceToGPT : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public OpenAITTS tts;
    public subtitles subtitles;
    public local_llm llm;
    public AudioClip recordedClip;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public historyinformation hf;//歷史紀錄腳本
    public TextMeshProUGUI tmpText; // 語音字幕
    private string audioFilePath;
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
    public bool npc3_lock_2;
    public bool npc2_fin;
    float lastActionTime=0f;
    float inactivityThreshold = 10f;
    bool time_set = false;
    public int context_error = 0; //錯誤次數
    public float running_time = 0f;//總時長計時
    public End_context_context_6 End_Control;
    void Start()
    {
        npc1_lock = false;
        npc2_lock = true;
        npc3_lock = true;
        npc3_lock_2 = true;
        npc2_fin =false;
        voice = "context6_1";
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
        subtitles = FindObjectOfType<subtitles>();
        hf = FindObjectOfType<historyinformation>();
        tts = FindObjectOfType<OpenAITTS>();
        llm = FindObjectOfType<local_llm>();
        audioSource1 = tts.audio1;
        audioSource2 = tts.audio2;
        if (subtitles != null)
        {
            Debug.Log("sub已綁定");
        }
        else
        {
            Debug.Log("sub未綁定");
        }
        // 初始化音頻保存路徑
        _ = sub_and_tts(status_tip);
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
            /*if (npc_status2 == Variables.Application.Get<int>("NPC_2_Status"))
            {
                if (!time_set)
                {
                    time_set = true;
                    lastActionTime = Time.time;
                }            
                if (Time.time - lastActionTime > inactivityThreshold)
                {
                    _ = sub_and_tts("請勿插入來路不明的隨身碟");
                  
                    Variables.Application.Set("NPC_2_Status", 0);
                }
                if (remind)
                {
                    lastActionTime = 0;
                    Variables.Application.Set("NPC_2_Status", 0);
                    Debug.Log("已組止");
                    remind = false;
                }
            }*/
        }
        if (npc_status1 == (int)Variables.ActiveScene.Get("NPC_1_Status") && (int)Variables.ActiveScene.Get("NPC_1_Status")==1)
        {
            tts.play_audio();
            npc_status1 = 3;
        }


        if ((int)Variables.ActiveScene.Get("NPC_2_Status") == 3 && npc2_lock)
        {
            Debug.Log("插完隨身碟");
            Variables.ActiveScene.Set("NPC_3_Status", 3);
            StartCoroutine(NPC2_TTS_Flow());
            status_prompt = Get_prompt(5);
            voice_switch(3);
            context_error++;//錯誤+1
            npc2_lock = false;
        }

        if ((int)Variables.ActiveScene.Get("NPC_3_Status") == -2 && npc3_lock)
        {
            StartCoroutine(NPC3_TTS_Flow());
            npc3_lock = false;
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

    private IEnumerator WaitForTTSToFinish(Action onComplete)
    {
        while (tts.Get_audio_status())
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    public IEnumerator NPC2_Start_talk()
    {
        _ = sub_and_tts("找我有甚麼事嗎");
        tts.play_audio();
        yield return null;
    }
    private IEnumerator NPC2_TTS_Flow()
    {
        tts.play_audio();
        bool audio_lock = false;
        while (true)
        {
            Debug.Log("tts是否撥放中" + audioSource1.isPlaying + audioSource2.isPlaying);
            if (audioSource1.isPlaying || audioSource2.isPlaying)
            {
                Debug.Log("tts是否撥放中" + audioSource1.isPlaying + audioSource2.isPlaying);
                audio_lock = true;
            }
            if (!audioSource1.isPlaying && audio_lock && !audioSource2.isPlaying)
            {
                Variables.ActiveScene.Set("NPC_3_Status", 4);
                break;
            }
            yield return null;
        }
    }
    public IEnumerator NPC3_Start_talk()
    {
        _ = sub_and_tts("找我有甚麼事嗎");
        tts.play_audio();
        yield return null;
    }
    private IEnumerator NPC3_TTS_Flow()
    {
        tts.UsingFileAudio("girl_scold.mp3");
        Material mat = tmpText.fontMaterial;
        mat.SetColor("_FaceColor", Color.red); // 設定字為紅色
        mat.SetColor("_OutlineColor", Color.red); // 設定邊為紅色
        subtitles.ShowSubtitlesInSegments("來路不明的隨身碟別亂插在電腦上");
        bool audio_lock = false;
        while (true)
        {
            Debug.Log("tts是否撥放中" + audioSource1.isPlaying + audioSource2.isPlaying);
            if (tts.audioSource.isPlaying)
            {
                Debug.Log("tts是否撥放中" + audioSource1.isPlaying + audioSource2.isPlaying);
                audio_lock = true;
            }
            if (!tts.audioSource.isPlaying && audio_lock)
            {
                mat.SetColor("_FaceColor", Color.white); // 設定字為白色
                mat.SetColor("_OutlineColor", Color.black); // 設定邊為黑色
                Variables.ActiveScene.Set("NPC_3_Status", 6);
                _ = sub_and_tts("來路不明的隨身碟應該銷毀");
                StartCoroutine(NPC3_End_move());
                break;
            }
            yield return null;
        }
    }

    private IEnumerator NPC3_End_move()
    {
        while ((int)Variables.ActiveScene.Get("NPC_3_Status") != -1) yield return null;
        tts.play_audio();
    }

    public IEnumerator Context6_Ending()
    {
        voice_switch(1);
        _ = sub_and_tts("幹的好!\n來路不明的隨身碟不能亂使用\n找不到使用者請放置失物招領或銷毀");
        TimeSpan timeSpan = TimeSpan.FromSeconds(running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //總秒數
        Debug.Log("總共用了" + time_record + "秒");
        hf.PostHistoryinformation(context_error + PlayerPrefs.GetInt("Wrong"), 6,time_seconds);
        tts.play_audio();
        yield return new WaitForSeconds(15f);
        StartCoroutine(Change_Context());
    }

    private IEnumerator Change_Context()
    {
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
            return (string)config["usb0"];
        }
        else if (index == 2)
        {
            return (string)config["usb1"];
        }
        else if (index == 3)
        {
            return (string)config["usb3"];
        }
        else if (index == 4)
        {
        
            return (string)config["usb4"];
        }
        else if (index == 5)
        {
            return (string)config["usb5"];
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
         
        if (keyword.Contains("那我們問一下其他人吧"))
        {
            npc1_lock = true;
            Variables.ActiveScene.Set("NPC_1_Status", 2);
            status_prompt = Get_prompt(3);
            status_tip = Get_tip(2);
            Debug.Log("已切換prompt");
            voice_switch(2);
        }       
        else if (keyword.Contains("插到電腦上看看裡面的資料"))
        {
            status_prompt = Get_prompt(4);
            Debug.Log("已切換prompt");                  
        }
        else if (keyword.Contains("你說得對，問問看其他同事吧"))
        {
            Variables.ActiveScene.Set("NPC_2_Status", 4);
            status_prompt = Get_prompt(5);
            npc2_fin = true;
            voice_switch(3);
        }              
        else if (keyword.Contains("請再說一次"))
        {
            _ = sub_and_tts(keyword);
        } 
        else
        {
            status_prompt = Get_prompt(0);
        }
    }
    public void voice_switch(int voiceIndex)
    {
        switch (voiceIndex)
        {
            case 1:
                this.voice = "context6_1";
                break;
            case 2:
                this.voice = "context6_2";
                break;
            case 3:
                this.voice = "context6_3";
                break;
            default:               
                this.voice = "context6_1";
                break;
        }

        Debug.Log($"已切換語音為：{this.voice}");
    }
    public void inputnothing(string keyword)
    {
        GenerateResponse(keyword,Get_prompt(0), async (gptResponse) =>
        {

            if (string.IsNullOrEmpty(gptResponse))
            {
                Debug.LogError("GPT Response is null or empty.");
                return;
            }
            chatHistory.Add(new Dictionary<string, string> { { "role", "system" }, { "content", gptResponse } });
            await tts.f5tts(gptResponse,voice,speed);
            subtitles.ShowSubtitlesInSegments("gpt: " + gptResponse);
            Debug.Log("GPT Response: " + gptResponse);

            endTime = Time.time;
            Debug.Log("總花費" + (endTime - startTime).ToString() + "秒");
            // 在这里可以添加 TTS 播放
        });
    }

    public async Task sub_and_tts(string keyword, double? setspeed = null)
    {
        // 如果 setspeed 有值，用它；否則用原本的 speed
        double finalSpeed = setspeed.HasValue ? setspeed.Value : speed;

        await tts.f5tts(keyword, voice, finalSpeed);
        Debug.Log($"已將語音加入緩存隊列：{keyword}");
    }


    public void correcter(string keyword)
    {
       _=sub_and_tts("這不是適當的回應\n請再試一次\n"+ keyword);
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
                            else if (gptResponse.Contains("任務失敗"))
                            {
                                _ = sub_and_tts("這不是你的隨身碟\n不要捏造事實");
                                context_error++;//錯誤+1
                                Variables.ActiveScene.Set("NPC_1_Status", 3);
                            }
                            else if ((int)Variables.ActiveScene.Get("NPC_2_Status") == 0 && npc1_lock)
                             {
                                 _ = sub_and_tts("請先點擊有浮標的同事");
                                 tts.play_audio();
                             }
                            else if (gptResponse.Contains("行為失敗"))
                            {
                                Variables.ActiveScene.Set("NPC_2_Status", 2);
                               
                                _ = sub_and_tts("電腦怎麼壞了");
                            }
                            else if ((int)Variables.ActiveScene.Get("NPC_3_Status") == 0 && npc2_fin)
                            {
                                _ = sub_and_tts("請先點擊有浮標同事");
                                tts.play_audio();
                            }
                            else if (gptResponse.Contains("建議是直接銷毀"))
                            {
                                await tts.f5tts(gptResponse, voice, speed);
                                tts.play_audio();
                                Variables.ActiveScene.Set("NPC_3_Status", 2);
                            }
                            else
                            {
                                await tts.f5tts(gptResponse, voice, speed);
                                tts.play_audio();
                                Debug.Log(gptResponse);
                                Set_scriptgraph(gptResponse);
                                endTime = Time.time;
                            }
                        });
                    }
                    else
                    {
                        GenerateResponse(transcribedText, Get_prompt(1), async (gptResponse) =>
                        {

                            if (string.IsNullOrEmpty(gptResponse))
                            {
                                Debug.LogError("GPT Response is null or empty.");
                                return;
                            }
                            chatHistory.Add(new Dictionary<string, string> { { "role", "system" }, { "content", gptResponse } });
                            await tts.TextToSpeechAsync(gptResponse, OpenAIKey, this);
                            subtitles.ShowSubtitlesInSegments("gpt: " + gptResponse);
                            Debug.Log("GPT Response: " + gptResponse);

                            endTime = Time.time;
                            Debug.Log("總花費" + (endTime - startTime).ToString() + "秒");
                            // 在这里可以添加 TTS 播放
                        });
                        //使用openai api
                        llm.GenerateResponse(transcribedText, Get_prompt(1), async (gptResponse) =>
                        {

                            if (string.IsNullOrEmpty(gptResponse))
                            {
                                Debug.LogError("GPT Response is null or empty.");
                                return;
                            }
                            chatHistory.Add(new Dictionary<string, string> { { "role", "system" }, { "content", gptResponse } });
                            await tts.TextToSpeechAsync(gptResponse, OpenAIKey, this);
                            subtitles.ShowSubtitlesInSegments("gpt: " + gptResponse);
                            Debug.Log("GPT Response: " + gptResponse);

                            endTime = Time.time;
                            Debug.Log("總花費" + (endTime - startTime).ToString() + "秒");
                            // 在这里可以添加 TTS 播放
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



