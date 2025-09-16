using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using TMPro;

public class VoiceToGPT_Try : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public OpenAITTS_Try tts;
    public subtitles_2 subtitles;
    public local_llm_2 llm;
    public AudioClip recordedClip;
    public historyinformation hf;//歷史紀錄腳本
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
    private float startTime;
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
    private bool first_time_open_door = false;
    public camaramove camaramove;
    public TextMeshProUGUI tmpText; // 語音字幕
    public int context_error = 0; //錯誤次數
    public float running_time = 0f;//總時長計時
    void Start()
    {
        npc1_lock = false;
        npc2_lock = false;
        npc3_lock = true;
        npc2_fin = false;
        voice = "context2_1";
        remind = false;
        npc_status1 = 1;
        npc_status2 = 3;
        npc_status3 = 3;
        speed = 0.75;
        status_prompt = Get_prompt(1);
        status_tip = Get_tip(1);

        audioFilePath = Path.Combine(Application.dataPath, "recorded_audio.wav");
        OpenAIKey = Get_api_key(1);
        whisper_key= Get_api_key(2);
        subtitles = FindObjectOfType<subtitles_2>();
        tts = FindObjectOfType<OpenAITTS_Try>();
        llm = FindObjectOfType<local_llm_2>();
        hf = FindObjectOfType<historyinformation>();
        Debug.Log(subtitles != null ? "sub已綁定" : "sub未綁定");

        _ = sub_and_tts("需要我們幫忙收拾嗎？");
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
            else if (isRecording && OVRInput.GetDown(OVRInput.Button.Two))
            {
                if (!tts.Get_audio_status())
                {
                    if (Microphone.IsRecording(null) && isRecording)
                    {
                        StopRecording();
                        startTime = Time.time;
                        ProcessAudioToGPT();
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

            if (!first_time_open_door && (int)Variables.ActiveScene.Get("Meeting_Door_Status") == 1)
            {
                first_time_open_door = true;
                Variables.ActiveScene.Set("NPC_1_Status", 1);
                StartCoroutine(npc1_putdown());
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

    // 等待前一個語音講完再進下一個
    private IEnumerator WaitForTTSToFinish(Action onComplete)
    {
        while (tts.Get_audio_status())
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    // 需要我們幫忙收拾嗎？的播放區
    private IEnumerator npc1_putdown()
    {
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1)
        {
            yield return null;
        }
        tts.PlayNextInQueue();
    }

    // 走進去會議室後又走出來
    public IEnumerator npc1_is_not_clean(String text1, String text2)
    {
        Material mat = tmpText.fontMaterial;
        mat.SetColor("_FaceColor", Color.red); // 設定字為紅色
        mat.SetColor("_OutlineColor", Color.red); // 設定邊為紅色
        Variables.ActiveScene.Set("NPC_1_Status", 2);
        context_error++;
        _ = sub_and_tts(text1,1.2);
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1)
            yield return null;

        tts.PlayNextInQueue();
        _ = sub_and_tts(text2,1.2);

        yield return WaitForTTSToFinish(() =>
        {
            mat.SetColor("_FaceColor", Color.white); // 設定字為白色
            mat.SetColor("_OutlineColor", Color.black); // 設定邊為黑色
            StartCoroutine(npc1_close_door());
            // 切換原本的語氣
        });
    }

    // 如果需要幫忙，可以開門找我們的播放區
    private IEnumerator npc1_close_door()
    {
        tts.PlayNextInQueue();
        yield return WaitForTTSToFinish(() =>
        {
            Variables.ActiveScene.Set("NPC_1_Status", 3);
        });
    }

    // 沒清完會議室
    public IEnumerator Did_not_finish_cleaning(int movenum, String text)
    {
        voice_switch(4);// 切換生氣語氣
        context_error++;
        _ = sub_and_tts("你在幹什麼");
        Variables.ActiveScene.Set("NPC_1_Status", movenum+30); // NPC1走進會議室中，到了定點後將水桶放到地上，放下後起來
        Variables.ActiveScene.Set("NPC_2_Status", movenum); // NPC2從門後走到文件前並將手機偷偷拿手機出來拍照
        yield return new WaitForSeconds(12f);
        Variables.ActiveScene.Set("NPC_1_Status", movenum + 40); // 做出指人(NPC_2)警告的動作
        tts.PlayNextInQueue();
        _ = sub_and_tts(text); // 不應該讓無關者看到，收好再叫我(生氣語氣)
        Variables.ActiveScene.Set("NPC_2_Status", movenum+10); // 嚇到後跑出會議室
        while ((int)Variables.ActiveScene.Get("NPC_2_Status") != -1)
        {
            yield return null;
        }
        Variables.ActiveScene.Set("NPC_1_Status", movenum + 50); // NPC1從1_41~1_44的定點位置拿起水桶後走向玩家
        yield return new WaitForSeconds(4.6f); // 延遲4.6秒讓前一個動作結束
        Variables.ActiveScene.Set("NPC_1_Status", movenum + 60); // 對著玩家揮手警告
        Material mat = tmpText.fontMaterial;
        mat.SetColor("_FaceColor", Color.red); // 設定字為紅色
        mat.SetColor("_OutlineColor", Color.red); // 設定邊為紅色
        tts.PlayNextInQueue();
        yield return new WaitForSeconds(4f);
        yield return WaitForTTSToFinish(() =>
        {
            mat.SetColor("_FaceColor", Color.white); // 設定字為白色
            mat.SetColor("_OutlineColor", Color.black); // 設定邊為黑色
            Variables.ActiveScene.Set("NPC_1_Status", movenum + 70); // 走出會議室並關門
            camaramove.enabled = true; // 玩家可以動了
            // 切換原本的語氣
        });
        voice_switch(1);
    }

    private void OnApplicationQuit()
    {
       
        Debug.Log("Unity 關閉，正在儲存對話紀錄...");
    }

    void stop_process()
    {
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
    }

    public void StopRecording()
    {
        Microphone.End(null);
        SaveAudioClipToFile(recordedClip, audioFilePath);
        Debug.Log($"錄音停止 檔案將儲存到 {audioFilePath}");
        isRecording = false;
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
        using (var writer = new BinaryWriter(stream))
        {
            int headerSize = 44;
            int fileSize = headerSize + samples.Length * 2;

            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(fileSize - 8);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16);
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(samples.Length * 2);

            foreach (var sample in samples)
            {
                short intSample = (short)(sample * short.MaxValue);
                writer.Write(intSample);
            }
        }
    }

    public void TranscribeAudio(string filePath, Action<string> callback)
    {
        StartCoroutine(SendWhisperRequest(filePath, callback));
    }

    IEnumerator SendWhisperRequest(string filePath, Action<string> callback)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        var form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filePath), "audio/wav");
        form.AddField("model", "gpt-4o-transcribe");
        form.AddField("language", "zh");

        using (UnityWebRequest request = UnityWebRequest.Post(WhisperApiUrl, form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + whisper_key);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = Content_of_whisper(request.downloadHandler.text);
                userchat = responseText;
                Debug.Log("Whisper Response: " + responseText);
                callback?.Invoke(responseText);
            }
            else
            {
                Debug.LogError("Whisper API Error: " + request.error);
                Debug.LogError("Response Body: " + request.downloadHandler.text);
                callback?.Invoke(null);
            }
        }
    }

    public void GenerateResponse(string prompt, string system_prompt, Action<string> callback)
    {
        StartCoroutine(SendGPTRequest(prompt, system_prompt, callback));
    }

    IEnumerator SendGPTRequest(string prompt, string system_prompt, Action<string> callback)
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
            temperature = 0.5
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
                string responseText = Content_of_gptreply(request.downloadHandler.text);
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
        JObject config = JObject.Parse(File.ReadAllText(APIPath));
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
        JObject config = JObject.Parse(json.text);
        return (string)config[$"prompt{index}"];
    }

    private string Get_tip(int index)
    {
        JObject config = JObject.Parse(json.text);
        return (string)config[$"tip{index}"];
    }

    public string Content_of_gptreply(string reply)
    {
        JObject json = JObject.Parse(reply);
        return (string)json["choices"][0]["message"]["content"];
    }

    public string Content_of_whisper(string reply)
    {
        JObject json = JObject.Parse(reply);
        return (string)json["text"];
    }

    public void voice_switch(int voiceIndex)
    {
        voice = voiceIndex switch
        {
            2 => "context2_2",
            3 => "context2_3",
            4 => "context2_1_scold",
            _ => "context2_1"
        };
        Debug.Log($"已切換語音為：{voice}");
    }

    // ===== 修改：只加入隊列，不立即播放 =====
    public async Task sub_and_tts(string keyword, double? setspeed = null)
    {
        // 如果 setspeed 有值，用它；否則用原本的 speed
        double finalSpeed = setspeed.HasValue ? setspeed.Value : speed;

        await tts.f5tts(keyword, voice, finalSpeed);
        Debug.Log($"已將語音加入緩存隊列：{keyword}");
    }

    // ===== 播放控制 =====
    public void PlayNextTTS()
    {
        tts.PlayNextInQueue();
    }

    public void PlayAllTTS()
    {
        StartCoroutine(PlayAllTTS_Queue());
    }

    private IEnumerator PlayAllTTS_Queue()
    {
        while (ttsQueueNotEmpty())
        {
            tts.PlayNextInQueue();
            yield return new WaitWhile(() => tts.Get_audio_status());
        }
    }

    private bool ttsQueueNotEmpty()
    {
        var field = typeof(OpenAITTS_Try).GetField("ttsQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var queue = (Queue<(string, AudioClip)>)field.GetValue(tts);
        return queue.Count > 0;
    }

    // ===== GPT 處理流程 =====
    void ProcessAudioToGPT()
    {
        TranscribeAudio(audioFilePath, (transcribedText) =>
        {
            if (!string.IsNullOrEmpty(transcribedText))
            {
                chatHistory.Add(new Dictionary<string, string> { { "role", "user" }, { "content", transcribedText } });

                GenerateResponse(transcribedText, status_prompt, async (gptResponse) =>
                {
                    if (string.IsNullOrEmpty(gptResponse))
                    {
                        Debug.LogError("GPT Response is null or empty.");
                        return;
                    }
                    else if (gptResponse.Contains("第一階段失敗"))
                    {
                        context_error++;//錯誤+1
                        StartCoroutine(npc1_is_not_clean("裡面還有許多資料沒收欸，我們進去不好吧", "如果需要幫忙，可以開門找我們"));
                    }
                    else if (gptResponse.Contains("好的"))
                    {
                        await sub_and_tts("好的~");
                        await sub_and_tts("如果需要幫忙，可以開門找我們");
                        tts.PlayNextInQueue();
                        StartCoroutine(WaitForTTSToFinish(() =>
                        {
                            StartCoroutine(npc1_close_door());
                        }));
                    }
                    else
                    {
                        await sub_and_tts(gptResponse);
                        Debug.Log(gptResponse);
                        endTime = Time.time;
                    }

                    // 可選擇自動播放下一段
                    // PlayNextTTS();
                });
            }
        });
    }

   
}
