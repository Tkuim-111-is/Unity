using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class getting_wifi : MonoBehaviour
{
    public Image targetImage; // 桌布跟網路連線狀況
    public Sprite Wrong_Wifi; // 錯誤網路的桌布
    public Sprite Correct_Wifi; // 正確密碼的桌布
    public Sprite Wrong_password; //錯誤密碼的圖
    public GameObject wifi_remind; // 滾去連網路提示
    public GameObject wifi_connect; // 網路選擇
    public GameObject wifi_entering_password; // 輸入密碼的地方
    public GameObject keyboard; // 輸入密碼用的鍵盤
    public Image wifi_entering_password_picture; // 輸入密碼的背景圖
    public TMP_Text inputField; // 密碼輸入框
    public GameObject wifi_checking; // 連網路確認
    public GameObject fail_picture; // 失敗圖片
    public GameObject congratulation_picture; // 成功圖片
    public GameObject Step4; // line通知
    public AudioSource audioSource;
    public AudioClip computerSound; // 電腦通知音效
    public AudioClip LineSound; // line通知音效
    
    public AudioClip SuccessSound; // 成功音效
    public VoiceToGPT_4 voiceToGPT_4;
    public keyboard Keyboard;
    public GameObject Wrong_bottom; // 錯誤wifi的按鈕
    

    private bool wifi_lock = true;
    private int wifi_check_num; // 0是錯的網路(無密碼)，1是對的網路(有密碼)
    private string password = "284@36307262";
    private Coroutine progressRoutine; // 異步(為了延遲幾秒切畫面)
    void Start()
    {
        wifi_remind.SetActive(false);
        wifi_connect.SetActive(false);
        wifi_entering_password.SetActive(false);
        keyboard.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(wifi_lock && (int)Variables.ActiveScene.Get("NPC_1_Status") == -2)
        {
            wifi_lock = false;
            audioSource.PlayOneShot(computerSound);
            wifi_remind.SetActive(true);
        }
    }

    // wifi圖片數值改動(判斷連的是安全或不安全的網路)
    public int WifiCheckNum
    {
        get => wifi_check_num;
        set => wifi_check_num = value;
    }

    // 點擊滾去連網路提示
    public void wifi_remind_to_wifi_connect()
    {
        wifi_remind.SetActive(false);
        wifi_connect.SetActive(true);
    }

    // 選到假網路
    public void wifi_connect_to_wrong_wifi()
    {
        wifi_connect.SetActive(false);
        wifi_checking.SetActive(true);
        wifi_check_num = 0;
    }

    // 確定使用先前選擇的網路
    public void WifiCheckIfYes()
    {
        StartCoroutine(wifi_check_if_yes());
    }

    public IEnumerator wifi_check_if_yes()
    {
        wifi_checking.SetActive(false);
        if(wifi_check_num == 0)
        {
            targetImage.sprite = Wrong_Wifi;
        }
        else
        {
            inputField.text = "";
            keyboard.SetActive(false);
            wifi_entering_password.SetActive(false);
            targetImage.sprite = Correct_Wifi;
        }
        yield return new WaitForSeconds(1f); // 延遲一秒再跳通知
        audioSource.PlayOneShot(LineSound);
        Step4.SetActive(true);
    }

    public void wifi_check_if_no()
    {
        wifi_checking.SetActive(false);
        wifi_connect.SetActive(true);
    }

    // 選到真網路
    public void wifi_connect_to_right_wifi()
    {
        keyboard.SetActive(true);
        wifi_entering_password.SetActive(true);
    }

    // 密碼輸入完，要確認密碼
    public void right_wifi_finish_password()
    {
        string input_password = Keyboard.GetPassword();
        // 輸入密碼正確
        if(input_password == password)
        {
            wifi_entering_password.SetActive(false);
            wifi_connect.SetActive(false);
            wifi_checking.SetActive(true);
            wifi_check_num = 1;
        }
        // 輸入密碼錯誤
        else
        {
            wifi_entering_password_picture.sprite = Wrong_password;
        }
    }

    // 密碼不打了，換個網路
    public void do_not_want_to_print_password()
    {
        keyboard.SetActive(false);
        inputField.text = "";
        wifi_entering_password.SetActive(false);
    }

    // 打開檔案出現的圖片
    public void openPDF()
    {
        if(wifi_check_num == 0) // 如果網路是不安全的
        {
            fail_picture.SetActive(true);
            StartCoroutine(voiceToGPT_4.Choose_wrong_wifi());
            Wrong_bottom.SetActive(false);
        }
        else // 如果網路是安全的
        {
            congratulation_picture.SetActive(true);
            StartCoroutine(voiceToGPT_4.Correct_PDF());
        }
    }

    public void do_again()
    {
        audioSource.PlayOneShot(computerSound);
        wifi_remind.SetActive(true);
        fail_picture.SetActive(false);
    }

    // 成功通關後延遲4秒切還大情境繼續尋找物件
    /*private IEnumerator Change_back_to_Choose_Object()
    {
        yield return new WaitForSeconds(4f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(4);
    }*/
}
