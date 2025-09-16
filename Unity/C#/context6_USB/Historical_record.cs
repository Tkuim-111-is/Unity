using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.VisualScripting;

[System.Serializable]
public class LearnStatusPayload
{
    public string user;
    public string context;
    public int err_count;

    public LearnStatusPayload(string User, string Context, int wrongNum)
    {
        user = User;
        context = Context;
        err_count = wrongNum;
    }
}

[System.Serializable]
public class LoginUser
{
    public int id;
    public string username;
}

[System.Serializable]
public class LoginResponse
{
    public bool success;
    public string token;
    public LoginUser user;
}


public class Historical_record : MonoBehaviour
{
    int err_count = 0;
    bool hasSent = false;

    /*void Start()
    {
        if (!PlayerPrefs.HasKey("TimerStart"))
        {
            PlayerPrefs.SetFloat("TimerStart", Time.realtimeSinceStartup);
            PlayerPrefs.Save();
        }

        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        while (true)
        {
            float start = PlayerPrefs.GetFloat("TimerStart", Time.realtimeSinceStartup);
            float elapsed = Time.realtimeSinceStartup - start;

            Debug.Log($"�w�g�g�L {elapsed:F2} ��");

            // �p�GĲ�o�������N����
            if (Variables.Application.Get<int>("NPC_3_Status") == 2)
            {
                Debug.Log("NPC_3_Status == 2�A����p��");

                // �M�� PlayerPrefs �����ɶ�
                PlayerPrefs.DeleteKey("TimerStart");
                PlayerPrefs.Save();
                break;
            }

            yield return new WaitForSeconds(1f);
        }*/

    void Update(){
        // �� NPC_3_Status �� 2 �B�|���e�X��Ĳ�o
        if (Variables.Application.Get<int>("NPC_3_Status") == 2 && !hasSent)
        {
            hasSent = true;
            StartCoroutine(SendLearnStatus());
        }
    }

    IEnumerator SendLearnStatus()
    {
        // ���o�n�J�^���øѪR
        string user = PlayerPrefs.GetString("Email", "");

        // �ʸ˸��
        LearnStatusPayload data = new LearnStatusPayload(user,"6",err_count);
        string json = JsonUtility.ToJson(data);

        UnityWebRequest www = new UnityWebRequest("http://163.13.201.86:8000/api/learn_status", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // �[�J token ����������
        string token = PlayerPrefs.GetString("AuthToken", "");
        if (!string.IsNullOrEmpty(token))
        {
            www.SetRequestHeader("Authorization", "Bearer " + token);
        }

        www.certificateHandler = new BypassCertificate();
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error sending learn status: " + www.error);
        }
        else
        {
            Debug.Log("Learn status sent successfully! Response: " + www.downloadHandler.text);
        }
    }
    public void NPC_check()
    {
        if (Variables.Application.Get<int>("NPC_1_Status") == 3 || Variables.Application.Get<int>("NPC_2_Status") == 2)
        {
            err_count++;
        }
    }
}
