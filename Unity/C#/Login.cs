using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.IO;
using System;

[System.Serializable]
public class LoginData
{
    public string email;
    public string password;

    // 構造函數來初始化 email 和 password
    public LoginData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}
[System.Serializable]
public class Loginresponse
{
    public bool success;
    public string token;
    
}

public class Login : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField passwordField;
    public Button submitButton;
    public TextMeshProUGUI wrongText;

    private TMP_InputField currentField;
    private bool isValidEmail = false;

    void Start()
    {
        wrongText.gameObject.SetActive(false);
        PlayerPrefs.DeleteKey("inputname");
        PlayerPrefs.DeleteKey("inputpassword");
        // 檢查 input field 是否存在
        if (nameField == null)
        {
            UnityEngine.Debug.LogError("Name field is not assigned!");
        }

        if (passwordField == null)
        {
            UnityEngine.Debug.LogError("Password field is not assigned!");
        }

        // 確保 NonNativeKeyboard 存在
        if (NonNativeKeyboard.Instance != null)
        {
            // 設置輸入欄位的事件處理
            nameField.onSelect.AddListener(delegate { ShowKeyboard(nameField, "1"); });
            passwordField.onSelect.AddListener(delegate { ShowKeyboard(passwordField, "2"); });

            // 讀取並顯示保存的文字
            nameSavedText();


            // 設置文字更新事件
            NonNativeKeyboard.Instance.OnTextUpdated += HandleTextUpdated;
        }
        else
        {
            UnityEngine.Debug.LogError("NonNativeKeyboard is not found in the scene!");
        }
    }

    public void nameSavedText()
    {
        // 直接設置帳號欄位的文字
        // 直接設置密碼欄位的文字
        nameField.text = PlayerPrefs.GetString("inputname");
        passwordField.text = PlayerPrefs.GetString("inputpassword");
    }

    void ShowKeyboard(TMP_InputField inputField, string checkText)
    {
        if (NonNativeKeyboard.Instance != null)
        {
            // 設置當前的輸入欄位
            NonNativeKeyboard.Instance.SetCurrentInputField(inputField, checkText);

            // 顯示鍵盤
            NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
        }
        else
        {
            UnityEngine.Debug.LogError("NonNativeKeyboard is not found in the scene!");
        }
    }

    void HandleTextUpdated(string text)
    {
        TMP_InputField currentInputField = NonNativeKeyboard.Instance.GetCurrentInputField();
        if (currentInputField != null)
        {
            currentInputField.text = text;
        }
    }

    void OnDestroy()
    {
        // 保存當前的輸入欄位文字
        if (nameField != null)
        {
            PlayerPrefs.SetString(nameField.name, nameField.text);
        }
        if (passwordField != null)
        {
            PlayerPrefs.SetString(passwordField.name, passwordField.text);
        }
        PlayerPrefs.Save();
    }

    public void CallLogin()
    {
        StartCoroutine(PerformLogin());
    }

    IEnumerator PerformLogin()
    {
        if (isValidEmail)
        {
            // 將 email 和 password 封裝到 LoginData 類別中
            LoginData loginData = new LoginData(nameField.text, passwordField.text);
            // 使用 JsonUtility 轉換為 JSON 字串
            string json = JsonUtility.ToJson(loginData);
            // 顯示 JSON 字串
            UnityEngine.Debug.Log(json);

            using (UnityWebRequest www = UnityWebRequest.Post("https://surecurity.org/api/auth/login", json))
            {
                // 將 JSON 資料轉換為字節數組
                byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(json);

                // 設置上傳處理器，發送原始字節數據
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);

                // 設置下載處理器
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.certificateHandler = new BypassCertificate();
                yield return www.SendWebRequest();
                /*
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    UnityEngine.Debug.LogError("Error: " + www.error);
                }
                else
                {*/
                Dictionary<string, string> headers = www.GetResponseHeaders();
                if (headers.ContainsKey("Set-Cookie"))
                {
                    string cookie = headers["Set-Cookie"];
                    PlayerPrefs.SetString("Cookie", cookie);
                    UnityEngine.Debug.Log("Cookie: " + cookie);
                }

                // 解析伺服器回應的 JSON 字串
                string responseJson = www.downloadHandler.text;
                UnityEngine.Debug.Log("回傳結果"+responseJson);


                // 處理回應
                Loginresponse response = JsonUtility.FromJson<Loginresponse>(responseJson);

                if (response.success)
                {
                    UnityEngine.Debug.Log("Login successful. Token: " + response.token);

                    // 直接將 JSON 字串存入 PlayerPrefs
                    PlayerPrefs.SetString("LoginResponse", responseJson);
                    PlayerPrefs.Save();  // 儲存更改

                    PlayerPrefs.SetString("Email", loginData.email);
                    PlayerPrefs.Save();  // 儲存更改

                    // 儲存 token 或其他資料
                    PlayerPrefs.SetString("AuthToken", response.token);
                    PlayerPrefs.Save();
                    Color currentColor = wrongText.color;
                    currentColor.a = Mathf.Clamp01(0f);
                    wrongText.color = currentColor;

                    // 跳轉到下一個場景
                    UnityEngine.SceneManagement.SceneManager.LoadScene(2);
                }
                else if (!response.success)
                {
                    wrongText.gameObject.SetActive(true);
                    UnityEngine.Debug.LogError("color change");
                }
                else
                {
                    UnityEngine.Debug.LogError("Login failed. Status: " + response.success);
                }
            }
        }
        else
        {
            wrongText.text = "格式錯誤\n未包含@或後面文字";
            wrongText.gameObject.SetActive(true);
        }
    }

    public void GoToRegister()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void VerifyInputs()
    {
        if (nameField.text.Contains("@"))
        {
            int atIndex = nameField.text.IndexOf("@");
            isValidEmail = (atIndex > 0 && atIndex < nameField.text.Length - 1);
        }
        //submitButton.interactable = (isValidEmail);
    }
}
//NonNativeKeyboard.Instance.PresentKeyboard();
