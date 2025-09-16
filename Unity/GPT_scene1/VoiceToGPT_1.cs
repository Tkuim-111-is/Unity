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











//wifi
public class VoiceToGPT_1 : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public OpenAITTS_1 tts;
    public subtitles_1 subtitles;
    public local_llm_1 llm;
    public AudioClip recordedClip;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    private string audioFilePath;
    private bool isRecording = false;
    private const string WhisperApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    public string OpenAIKey;
    private const string GPTApiUrl = "https://api.openai.com/v1/chat/completions";
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
   

    void Start()
    {
        npc1_lock = false;
        npc2_lock = false;
        npc3_lock = true;
        npc2_fin=false;
        voice = "voice1";
        remind = false;
        npc_status1 = 1;
        npc_status2 = 3;
        npc_status3 = 3;
        speed = 0.75;
        status_prompt = Get_prompt(1);     
        status_tip = Get_tip(1);      
        audioFilePath = Path.Combine(Application.dataPath, "recorded_audio.wav");
        OpenAIKey = Get_api_key();
        subtitles = FindObjectOfType<subtitles_1>();
        tts = FindObjectOfType<OpenAITTS_1>();
        llm = FindObjectOfType<local_llm_1>();
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

    }

    void Update()
    {
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

    private IEnumerator npc1_movent()
    {
        tts.play_audio();
        bool audio_lock = false;
        while (true)
        {
            Debug.Log("tts是否撥放中" + audioSource1.isPlaying + audioSource2.isPlaying);
            if (audioSource1.isPlaying|| audioSource2.isPlaying)
            {
                Debug.Log("tts是否撥放中" + audioSource1.isPlaying+ audioSource2.isPlaying);
                audio_lock = true;
            }
            if (!audioSource1.isPlaying && audio_lock && !audioSource2.isPlaying)
            {
                Variables.ActiveScene.Set("NPC_1_Status", 1);
                break;
            }
            yield return null;
        }
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
            request.SetRequestHeader("Authorization", "Bearer " + OpenAIKey);

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

    private string Get_api_key()
    {
        string APIPath = Path.Combine(Application.dataPath, "Unity", "GPT_scene1", "api_key.json");

        string jsonContent = File.ReadAllText(APIPath);
        // 解析 JSON 並取得 API Key
        JObject config = JObject.Parse(jsonContent);
        string apiKey = (string)config["API_Key"];
        return apiKey;
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
            case 2:
                this.voice = "voice2";
                break;
            case 3:
                this.voice = "voice3";
                break;
            default:               
                this.voice = "voice1";
                break;
        }

        Debug.Log($"已切換語音為：{this.voice}");
    }
   

    public async Task sub_and_tts(string keyword)
    {
        await tts.f5tts(keyword,voice,speed);
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
                            else
                            {

                                await tts.f5tts(gptResponse, voice, speed);
                                Set_scriptgraph(gptResponse);
                                tts.play_audio();
                                Debug.Log(gptResponse);
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



