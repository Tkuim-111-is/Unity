using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Security.Principal;




public class Payload
{
    public string user_email;
    public int context_id;
    public int err_count;
    public int time_record;
}

public class historyinformation : MonoBehaviour
{
    private string email;
    private string token;
    void Awake()   // 或用 Start()
    {
        //email = "aaa@gmail.com";
        email = PlayerPrefs.GetString("Email");
       // token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MSwiZW1haWwiOiJhYWFAZ21haWwuY29tIiwiZXhwIjoxNzU2MTMxODMxfQ.uJ3ynI_Fn_lP0UGsfqwq3SLT3OsORnS8TKTO77lT9HIEE4";
        token = PlayerPrefs.GetString("AuthToken");
    }
    string url= "https://surecurity.org/api/profile/learn_status";
    //建立回傳錯誤次數機制
    public IEnumerator PostHistory( int contextXError,int context_id,int time_record)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("[PostHistory] URL is empty.");
            yield break;
        }
        if (string.IsNullOrEmpty(token))
            Debug.LogWarning("[PostHistory] AuthToken is empty.");
        if (string.IsNullOrEmpty(email))
            Debug.LogWarning("[PostHistory] Email is empty.");

        // 準備 JSON 內容
        var data = new Payload
        {
            user_email = email,
            context_id = context_id,
            err_count = contextXError,
            time_record= time_record

        };

        string jsonData = JsonConvert.SerializeObject(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        Debug.Log("成功轉換" + jsonData);
        // 建立請求
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        // 發送請求並等待回應
        yield return request.SendWebRequest();

        // 判斷是否成功
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }
    public void PostHistoryinformation(int contextXError, int context_id, int time_record)
    {
        Debug.Log("上傳context"+context_id+"錯誤次數");
        StartCoroutine(PostHistory(contextXError, context_id,time_record));
    }

}
