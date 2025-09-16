using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Background_Information : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public TextMeshProUGUI infoText;

    private int itemId;
    private int wrong;
    void Start()
    {
        itemId = PlayerPrefs.GetInt("ItemID", -1);
        wrong = PlayerPrefs.GetInt("Wrong");
        if (itemId == 5) // 情境7
        {
            infoText.text = "你現在在辦公室巡視員工們的工作情況，但似乎有些人對資安不太熟悉\n請糾正他們的錯誤點";
        }
        else if (itemId == 6) // 情境4
        {
            infoText.text = "你在咖啡廳悠閒的喝咖啡\n同事突然傳東西要你下載";
        }
        else if (itemId == 7) // 情境6
        {
            if (wrong <= 0)
            {
                infoText.text = "你的同事撿到了一個來路不明的隨身碟\n幫助他並回答他的問題";
            }
            else
            {
                infoText.text = "你的同事撿到了一個來路不明的隨身碟\n並不屬於你";
            }
        }
        else if(itemId == 8) // 情境2
        {
            infoText.text = "會議剛結束，你需要清理會議室，避免洩漏重要資訊。";
        }
    }

    void Update()
    {
        GetInScene();
    }

    public void GetInScene()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                SceneManager.LoadScene(itemId);
            }
        }
    }
}
