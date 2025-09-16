using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class afterloginmenu : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteKey("WIFI");
        PlayerPrefs.DeleteKey("document_holder");
        PlayerPrefs.DeleteKey("USB");
        PlayerPrefs.DeleteKey("ItemID");
        PlayerPrefs.SetInt("Wrong",0);
    }
    public void GoToLearning()
    {
        PlayerPrefs.SetInt("ItemID", 5);
        SceneManager.LoadScene(4);
    }
    public void GoToSet()
    {
        SceneManager.LoadScene(3);
    }
}