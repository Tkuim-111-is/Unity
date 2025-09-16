using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Net.Security;
using System;
using System.IO;
using static UnityEngine.Debug;
using System.Text;
using System.Collections.Generic;







public class OpenAITTS_3 : MonoBehaviour
{
    bool f5tts_lock=false;
   public AudioSource audio1;
  public  AudioSource audio2;
    int audio_num = 1;
    public subtitles_3 subtitles;
    string text1;
    string text2;
    private const string API_URL = "https://api.openai.com/v1/audio/speech"; // 假設 OpenAI 的 TTS 端點
    private string apiBaseUrl = "http://163.13.202.122:9880";
    private string googleapi = "AIzaSyCBKD-HCAIAtm4rhmVwEJj2ZbJzt_aC47A";
    private bool isPlaying = false;
    private Queue<(string text, string voice, double speed)> ttsQueue = new Queue<(string, string, double)>();
    public AudioSource audioSource;
    private void Start()
    {       
        subtitles = FindObjectOfType<subtitles_3>();
        audio1 = gameObject.AddComponent<AudioSource>();
        audio2= gameObject.AddComponent<AudioSource>();
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
        if (audioSource == null)
        {
            Log("audioSource未綁定");
        }
        else
        {
            Log("audioSource已綁定");
        }
    }

    /// <summary>
    /// JSON 請求的結構
    /// </summary>
    private class TTSRequest
    {
        public string input { get; set; }
        public string voice { get; set; }
        public string format { get; set; }
        public string model { get; set; }
    }

    /// <summary>
    /// 從 API 獲取語音並播放
    /// </summary>
    /// <param name="text">需要轉換的文本</param>
    public IEnumerator TextToSpeech(string text, string API_KEY)
    {
        // 準備 JSON 請求數據
        TTSRequest requestBody = new TTSRequest
        {
            model = "gpt-4o-mini-tts",
            input = text,
            voice = "alloy",  // 使用繁體中文語音
            format = "mp3"    // 音頻格式
        };

        // 使用 Newtonsoft.Json 將物件轉換為 JSON 字串
        string jsonData = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip("", AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 從回應中提取音頻並播放
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Log("TTS Error: " + request.error);
            }
        }
    }
    private IEnumerator TextToSpeechCoroutine(string text, string apiKey, System.Action onComplete)
    {
        // 調用 TTS 的具體實現（可能是 API 調用或音頻播放）
        yield return StartCoroutine(TextToSpeech(text, apiKey));

        // 協程完成時調用回調通知
        onComplete?.Invoke();
    }

    public async Task TextToSpeechAsync(string text, string apiKey, MonoBehaviour monoBehaviour)
    {
        bool isCompleted = false;

        // 啟動協程，並在完成時更新 isCompleted
        monoBehaviour.StartCoroutine(TextToSpeechCoroutine(text, apiKey, () => isCompleted = true));

        // 等待協程完成
        while (!isCompleted)
        {
            await Task.Yield(); // 等待协程处理完成
        }
    }

    IEnumerator googlePlay(string inputText, TaskCompletionSource<bool> tcs)
    {
        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-tts:generateContent?key=" + googleapi;

        string requestJson = $@"
{{
  ""contents"": [
    {{
      ""parts"": [
        {{
          ""text"": ""{inputText}""
        }}
      ]
    }}
  ],
  ""generationConfig"": {{
    ""responseModalities"": [""AUDIO""],
    ""speechConfig"": {{
      ""voiceConfig"": {{
        ""prebuiltVoiceConfig"": {{
          ""voiceName"": ""Kore""
        }}
      }}
    }}
  }},
  ""model"": ""gemini-2.5-flash-preview-tts""
}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
              
                // 解析 base64 音訊資料
                string base64Audio = ParseAudioFromJson(responseJson);
                byte[] audioData = System.Convert.FromBase64String(base64Audio);

                // 播放音訊
                AudioClip clip = GeminiPcmUtility.ToAudioClip(audioData);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("TTS Error: " + request.error);
            }
            tcs.SetResult(true);
        }
    }


    public async Task googletts(string text)
    {
        await PlayTTSAndWaitgoogle(text);
    }
    public Task PlayTTSAndWaitgoogle(string text)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(googlePlay(text, tcs));
        return tcs.Task;
    }
    public void play_audio()
    {
        StartCoroutine(WaitAndPlayAudio());
    }
    private IEnumerator WaitAndPlayAudio()
    {
        // 等待 f5tts_lock 為 true
        yield return new WaitUntil(() => f5tts_lock);

        if (audio_num == 2 && audio1.clip != null)
        {
            audio1.Play();
            subtitles.ShowSubtitlesInSegments(text1);
            Debug.Log("audio1播放");
        }
        else if (audio_num == 1 && audio2.clip != null)
        {
            audio2.Play();
            subtitles.ShowSubtitlesInSegments(text2);
            Debug.Log("audio2播放");
        }
        else
        {
            Debug.LogWarning("No audio clip available to play.");
        }
        f5tts_lock = false;
    }

    public async Task f5tts(string text, string voice, double speed)
    {
        // 加入佇列
        ttsQueue.Enqueue((text, voice, speed));

        // 如果已經在播放中，就先返回
        if (isPlaying) return;

        // 不在播放中時開始處理佇列
        while (ttsQueue.Count > 0)
        {
            var item = ttsQueue.Dequeue();
            isPlaying = true;
            await PlayTTSAndWait(item.text, item.voice, item.speed);
            isPlaying = false;
        }
    }

    public Task PlayTTSAndWait(string text, string voice, double speed)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(PlayTTS(text, tcs, voice, speed));
        return tcs.Task;
    }

    IEnumerator PlayTTS(string text, TaskCompletionSource<bool> tcs, string voice, double speed)
    {
        string baseUrl = "http://163.13.202.122:7860/synthesize_speech/";
        string query = $"?text={UnityWebRequest.EscapeURL(text)}&voice={voice}&speed={speed}";
        string fullUrl = baseUrl + query;

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullUrl, AudioType.WAV);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (audio_num == 1)
            {
                text1 = text;
                audio1.clip = clip;
                audio_num = 2;
            }
            else
            {
                text2 = text;
                audio2.clip = clip;
                audio_num = 1;
            }
            f5tts_lock = true; 
        }
        else
        {
            Debug.LogError("TTS 播放失敗: " + www.error);
        }

        tcs.SetResult(true);
    }
    string refAudioPath = "ref/vtuber.wav";
    IEnumerator CallTTSAPI(string text)
    {
        string textLang = "zh";
        string promptLang = "zh";
        string endpoint = "/tts";
        string url = $"{apiBaseUrl}{endpoint}?text={text}&text_lang={textLang}&ref_audio_path={refAudioPath}&prompt_lang={promptLang}&text_split_method=cut5&batch_size=1&media_type=wav&streaming_mode=True&threshold=30";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 音频流返回，您可以在此处理音频流（例如播放音频或保存为文件）
                byte[] audioData = webRequest.downloadHandler.data;
                Log("下载成功，字节长度:" + audioData.Length);
                AudioClip clip = ToAudioClip(audioData);

                // 播放音频
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.Play();
                Log("已撥放");
            }
            else
            {
                // 错误处理
                Log("请求失败: " + webRequest.error);
                Log("Response Body: " + webRequest.downloadHandler.text);
                Log("Response Code: " + webRequest.responseCode); // 输出HTTP响应状态码

            }
        }

    }
    public static AudioClip ToAudioClip(byte[] data)
    {
        int channels = 1;  // 单声道
        int sampleRate = 32000;  // 默认采样率（实际使用中你需要根据 WAV 文件设置正确的采样率）
        int length = data.Length / 2;  // 假设是 16-bit PCM 数据（2 字节）

        AudioClip audioClip = AudioClip.Create("tts", length, channels, sampleRate, false);
        float[] floatData = ConvertBytesToFloatArray(data);
        audioClip.SetData(floatData, 22);

        return audioClip;
    }

    private static float[] ConvertBytesToFloatArray(byte[] data)
    {
        float[] floatData = new float[data.Length / 2];
        for (int i = 0; i < data.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(data, i);
            floatData[i / 2] = sample / 32768.0f;
        }
        return floatData;
    }
    public async Task gpt_sovit_tts(string text)
    {
        StartCoroutine(CallTTSAPI(text));


    }


    string ParseAudioFromJson(string json)
    {
        var wrapper = JsonUtility.FromJson<ResponseWrapper>(json);
        return wrapper.candidates[0].content.parts[0].inlineData.data;
    }

    [System.Serializable]
    public class ResponseWrapper
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    public class Candidate
    {
        public Content content;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class Part
    {
        public InlineData inlineData;
    }

    [System.Serializable]
    public class InlineData
    {
        public string mimeType;
        public string data;
    }
    public bool Get_audio_status()
    {
        if (audioSource.isPlaying)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static class GeminiPcmUtility
    {
        public static AudioClip ToAudioClip(byte[] pcmData, int sampleRate = 24000, string name = "gemini_tts")
        {
            if (pcmData == null || pcmData.Length < 2)
            {
                Debug.LogError("PCM 資料無效或太短！");
                return null;
            }

            int samples = pcmData.Length / 2; // 每個 sample 是 2 bytes
            float[] floatData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                int byteIndex = i * 2;
                if (byteIndex + 1 >= pcmData.Length) break; // 防止越界
                short sample = BitConverter.ToInt16(pcmData, byteIndex);
                floatData[i] = sample / 32768f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(floatData, 0);
            return clip;
        }
    }

}