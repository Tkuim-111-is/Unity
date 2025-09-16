using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class LogoutHandler : MonoBehaviour
{
    private string url = "https://surecurity.org/api/auth/logout";

    public void GoToGame()
    {
        SceneManager.LoadScene(4); //之後要改成操作教學的介面
    }

    public void Logout()
    {
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        string url = "https://surecurity.org/logout"; // 伺服器的登出 API
         UnityWebRequest request = UnityWebRequest.Get(url); 
        string cookie = PlayerPrefs.GetString("Cookie"); 
        request.SetRequestHeader("Cookie", cookie); // 如果需要帶 Cookie
        yield return request.SendWebRequest(); 
        if (request.result == UnityWebRequest.Result.Success)
        { 
            UnityEngine.Debug.Log("Logout successful!"); 
            PlayerPrefs.DeleteKey("Cookie"); 
            SceneManager.LoadScene(0); 
        } 
        else 
        { 
            UnityEngine.Debug.LogError("Logout failed: " + request.error);
        }
    }
}
