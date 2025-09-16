using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;


public class OpenAITTS_4 : MonoBehaviour
{
    public AudioSource audioSource;
    public subtitles_4 subtitles;

    private Queue<(string text, AudioClip clip)> ttsQueue = new Queue<(string, AudioClip)>();
    private bool isPlaying = false;

    private const string API_URL = "https://api.openai.com/v1/audio/speech";
    private string googleapi = "AIzaSyCBKD-HCAIAtm4rhmVwEJj2ZbJzt_aC47A";

    private void Start()
    {
        subtitles = FindObjectOfType<subtitles_4>();
        audioSource = gameObject.AddComponent<AudioSource>();

        ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;
    }

    // -------------------- OpenAI TTS --------------------
    private class TTSRequest
    {
        public string input { get; set; }
        public string voice { get; set; }
        public string format { get; set; }
        public string model { get; set; }
    }

    public IEnumerator TextToSpeech(string text, string API_KEY)
    {
        TTSRequest requestBody = new TTSRequest
        {
            model = "gpt-4o-mini-tts",
            input = text,
            voice = "alloy",
            format = "mp3"
        };

        string jsonData = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip("", AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                ttsQueue.Enqueue((text, clip));
                Debug.Log("OpenAI TTS 生成完成並已緩存");
            }
            else
            {
                Debug.Log("TTS Error: " + request.error);
            }
        }
    }

    // -------------------- Google TTS --------------------
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
                string base64Audio = ParseAudioFromJson(responseJson);
                byte[] audioData = Convert.FromBase64String(base64Audio);

                AudioClip clip = GeminiPcmUtility.ToAudioClip(audioData);
                ttsQueue.Enqueue((inputText, clip));
                Debug.Log("Google TTS 生成完成並已緩存");
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
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(googlePlay(text, tcs));
        await tcs.Task;
    }

    // -------------------- 本地 TTS --------------------
    public async Task f5tts(string text, string voice, double speed)
    {
        AudioClip clip = await GenerateTTS(text, voice, speed);
        if (clip != null)
        {
            ttsQueue.Enqueue((text, clip));
            Debug.Log("本地 TTS 生成完成並已緩存");
        }
    }

    private async Task<AudioClip> GenerateTTS(string text, string voice, double speed)
    {
        string baseUrl = "http://163.13.202.122:7860/synthesize_speech/";
        string query = $"?text={UnityWebRequest.EscapeURL(text)}&voice={voice}&speed={speed}";
        string fullUrl = baseUrl + query;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullUrl, AudioType.WAV))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
            else
            {
                Debug.LogError("TTS error: " + www.error);
                return null;
            }
        }
    }

    // -------------------- 播放控制 --------------------
    public void PlayNextInQueue()
    {
        if (isPlaying)
        {
            Debug.LogWarning("正在播放中，忽略新的 PlayNextInQueue 呼叫");
            return;
        }

        if (ttsQueue.Count == 0)
        {
            Debug.Log("Queue 是空的，啟動等待播放協程");
            StartCoroutine(WaitForClipInQueue());
            return;
        }

        PlayClipFromQueue();
    }

    private IEnumerator WaitForClipInQueue()
    {
        Debug.Log("等待 queue 中有東西...");

        // 持續等到有東西為止（無 timeout）
        while (ttsQueue.Count == 0)
        {
            yield return null;
        }

        Debug.Log("Queue 有東西了，開始播放");
        PlayClipFromQueue();
    }

    private void PlayClipFromQueue()
    {
        var (text, clip) = ttsQueue.Dequeue();
        if (clip == null)
        {
            Debug.LogError("取出的 Clip 是 null，無法播放");
            return;
        }

        audioSource.clip = clip;
        subtitles.ShowSubtitlesInSegments(text);
        audioSource.Play();
        isPlaying = true;
        StartCoroutine(WaitForAudioToFinish(clip.length));
    }

    private IEnumerator WaitForAudioToFinish(float clipLength)
    {
        float startTime = Time.time;
        while (audioSource.isPlaying || Time.time - startTime < clipLength)
        {
            yield return null;
        }

        isPlaying = false;
        Debug.Log("播放完成");
    }


    public void ClearQueue()
    {
        ttsQueue.Clear();
        Debug.Log("已清除所有緩存語音");
    }

    // -------------------- 工具 --------------------
    public string ParseAudioFromJson(string json)
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

    public static class GeminiPcmUtility
    {
        public static AudioClip ToAudioClip(byte[] pcmData, int sampleRate = 24000, string name = "gemini_tts")
        {
            if (pcmData == null || pcmData.Length < 2)
            {
                Debug.LogError("PCM 資料無效或太短！");
                return null;
            }

            int samples = pcmData.Length / 2;
            float[] floatData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                int byteIndex = i * 2;
                if (byteIndex + 1 >= pcmData.Length) break;
                short sample = BitConverter.ToInt16(pcmData, byteIndex);
                floatData[i] = sample / 32768f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(floatData, 0);
            return clip;
        }
    }
    //撥放錄音內容
    public void UsingFileAudio(string filename)
    {
        StartCoroutine(PlayFileAudio(filename));
    }
    IEnumerator PlayFileAudio(string audioFileName)
    {
        string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, audioFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"找不到音檔：{path}");
            yield break;
        }

        using (WWW www = new WWW("file://" + path))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError($"載入音檔錯誤：{www.error}");
                yield break;
            }

            audioSource.clip = www.GetAudioClip();
            audioSource.Play();
            Debug.Log($"正在播放音檔：{audioFileName}");
        }
    }
    public bool Get_audio_status()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}
