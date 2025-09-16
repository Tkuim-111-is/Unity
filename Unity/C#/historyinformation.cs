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
    void Awake()   // �Υ� Start()
    {
        //email = "aaa@gmail.com";
        email = PlayerPrefs.GetString("Email");
       // token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MSwiZW1haWwiOiJhYWFAZ21haWwuY29tIiwiZXhwIjoxNzU2MTMxODMxfQ.uJ3ynI_Fn_lP0UGsfqwq3SLT3OsORnS8TKTO77lT9HIEE4";
        token = PlayerPrefs.GetString("AuthToken");
    }
    string url= "https://surecurity.org/api/profile/learn_status";
    //�إߦ^�ǿ��~���ƾ���
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

        // �ǳ� JSON ���e
        var data = new Payload
        {
            user_email = email,
            context_id = context_id,
            err_count = contextXError,
            time_record= time_record

        };

        string jsonData = JsonConvert.SerializeObject(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        Debug.Log("���\�ഫ" + jsonData);
        // �إ߽ШD
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + token);

        // �o�e�ШD�õ��ݦ^��
        yield return request.SendWebRequest();

        // �P�_�O�_���\
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
        Debug.Log("�W��context"+context_id+"���~����");
        StartCoroutine(PostHistory(contextXError, context_id,time_record));
    }

}
