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
            DontDestroyOnLoad(gameObject);  // 確保場景切換時，這個物件不會被銷毀
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 儲存 JSON 資料
    public void SaveJson(string json)
    {
        jsonData = json;
    }

    // 取得 JSON 資料
    public string LoadJson()
    {
        return jsonData;
    }
}
