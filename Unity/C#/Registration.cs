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

public class Registration : MonoBehaviour
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

    public void CallRegister()
    {
        StartCoroutine(PerformRegister());
    }

    IEnumerator PerformRegister()
    {
        // 將 email 和 password 封裝到 RegisterData 類別中
        RegisterData registerData = new RegisterData(nameField.text, passwordField.text);
        // 使用 JsonUtility 轉換為 JSON 字串
        string json = JsonUtility.ToJson(registerData);
        // 顯示 JSON 字串
        UnityEngine.Debug.Log(json);

        using (UnityWebRequest www = UnityWebRequest.Post("https://surecurity.org/api/auth/register", json))
        {
            if (isValidEmail)
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

                // 解析伺服器回應的 JSON 字串
                string responseJson = www.downloadHandler.text;

                // 處理回應
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(responseJson);

                if (response.success == true)
                {
                    UnityEngine.Debug.Log("Register successful.");

                    // 跳轉到下一個場景
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
                else if (response.message == "此電子郵件已被註冊")
                {
                    wrongText.text = "此電子郵件已被註冊";
                    wrongText.gameObject.SetActive(true);
                    UnityEngine.Debug.LogError("color change");
                }
                else if (response.message == "缺少必要信息")
                {
                    wrongText.text = "請輸入帳號及密碼";
                    wrongText.gameObject.SetActive(true);
                }
                else
                {
                    UnityEngine.Debug.LogError("Register failed. Status: " + response.success);
                }
            }
            else
            {
                wrongText.text = "格式錯誤\n未包含@或後面文字";
                wrongText.gameObject.SetActive(true);
            }
        }
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


    public void GoToLogin()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}

[System.Serializable]
public class RegisterData
{
    public string email;
    public string password;

    // 構造函數來初始化 email 和 password
    public RegisterData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}

[System.Serializable]
public class RegisterResponse
{
    public Boolean success;
    public String message;
}