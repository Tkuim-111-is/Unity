using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Json : MonoBehaviour
{
    public static Json Instance;

    private string jsonData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �T�O���������ɡA�o�Ӫ��󤣷|�Q�P��
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // �x�s JSON ���
    public void SaveJson(string json)
    {
        jsonData = json;
    }

    // ���o JSON ���
    public string LoadJson()
    {
        return jsonData;
    }
}
