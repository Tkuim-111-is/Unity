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
        // �ˬd input field �O�_�s�b
        if (nameField == null)
        {
            UnityEngine.Debug.LogError("Name field is not assigned!");
        }

        if (passwordField == null)
        {
            UnityEngine.Debug.LogError("Password field is not assigned!");
        }

        // �T�O NonNativeKeyboard �s�b
        if (NonNativeKeyboard.Instance != null)
        {
            // �]�m��J��쪺�ƥ�B�z
            nameField.onSelect.AddListener(delegate { ShowKeyboard(nameField, "1"); });
            passwordField.onSelect.AddListener(delegate { ShowKeyboard(passwordField, "2"); });

            // Ū������ܫO�s����r
            nameSavedText();


            // �]�m��r��s�ƥ�
            NonNativeKeyboard.Instance.OnTextUpdated += HandleTextUpdated;
        }
        else
        {
            UnityEngine.Debug.LogError("NonNativeKeyboard is not found in the scene!");
        }
    }

    public void nameSavedText()
    {
        // �����]�m�b����쪺��r
        // �����]�m�K�X��쪺��r
        nameField.text = PlayerPrefs.GetString("inputname");
        passwordField.text = PlayerPrefs.GetString("inputpassword");
    }

    void ShowKeyboard(TMP_InputField inputField, string checkText)
    {
        if (NonNativeKeyboard.Instance != null)
        {
            // �]�m��e����J���
            NonNativeKeyboard.Instance.SetCurrentInputField(inputField, checkText);

            // �����L
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
        // �O�s��e����J����r
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
        // �N email �M password �ʸ˨� RegisterData ���O��
        RegisterData registerData = new RegisterData(nameField.text, passwordField.text);
        // �ϥ� JsonUtility �ഫ�� JSON �r��
        string json = JsonUtility.ToJson(registerData);
        // ��� JSON �r��
        UnityEngine.Debug.Log(json);

        using (UnityWebRequest www = UnityWebRequest.Post("https://surecurity.org/api/auth/register", json))
        {
            if (isValidEmail)
            {
                // �N JSON ����ഫ���r�`�Ʋ�
                byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(json);

                // �]�m�W�ǳB�z���A�o�e��l�r�`�ƾ�
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);

                // �]�m�U���B�z��
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.certificateHandler = new BypassCertificate();
                yield return www.SendWebRequest();

                // �ѪR���A���^���� JSON �r��
                string responseJson = www.downloadHandler.text;

                // �B�z�^��
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(responseJson);

                if (response.success == true)
                {
                    UnityEngine.Debug.Log("Register successful.");

                    // �����U�@�ӳ���
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
                else if (response.message == "���q�l�l��w�Q���U")
                {
                    wrongText.text = "���q�l�l��w�Q���U";
                    wrongText.gameObject.SetActive(true);
                    UnityEngine.Debug.LogError("color change");
                }
                else if (response.message == "�ʤ֥��n�H��")
                {
                    wrongText.text = "�п�J�b���αK�X";
                    wrongText.gameObject.SetActive(true);
                }
                else
                {
                    UnityEngine.Debug.LogError("Register failed. Status: " + response.success);
                }
            }
            else
            {
                wrongText.text = "�榡���~\n���]�t@�Ϋ᭱��r";
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

    // �c�y��ƨӪ�l�� email �M password
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