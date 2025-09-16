using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class local_llm_2 : MonoBehaviour
{
    private string apiUrl = "http://127.0.0.1:11434/api/chat";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GenerateResponse(string prompt, string system_prompt, System.Action<string> callback)
    {
        StartCoroutine(SendGPTRequest(prompt, system_prompt, callback));
        Debug.Log("使用ollama");
    }

    IEnumerator SendGPTRequest(string prompt,string system_prompt, System.Action<string> callback)
    {
        var requestData = new
        {
            model = "Yu-Feng/Llama-3.1-TAIDE-LX-8B-Chat:FP16",
            messages = new[]
        {
                new { role = "system", content = system_prompt },
                new { role = "user", content = prompt },
            },
            stream = false
        };

        string jsonData = JsonConvert.SerializeObject(requestData);    
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");          
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;          
                if (string.IsNullOrEmpty(responseText))
                {
                    Debug.LogError("GPT API Error: Empty response");
                    callback?.Invoke(null);
                    yield break;
                }
                responseText = Content_of_gptreply(responseText);            
                callback?.Invoke(responseText);
            }
            else
            {
                Debug.LogError("GPT API Error: " + request.error);
                callback?.Invoke(null);
            }
        }
    }
    public string Content_of_gptreply(string reply)
    {
        JObject json = JObject.Parse(reply);

        // 導航到 choices[0].message.content
        string content = (string)json["message"]["content"];      
        return content;
    }

}
