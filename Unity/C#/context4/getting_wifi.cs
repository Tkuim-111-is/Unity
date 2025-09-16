using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class getting_wifi : MonoBehaviour
{
    public Image targetImage; // �६������s�u���p
    public Sprite Wrong_Wifi; // ���~�������६
    public Sprite Correct_Wifi; // ���T�K�X���६
    public Sprite Wrong_password; //���~�K�X����
    public GameObject wifi_remind; // �u�h�s��������
    public GameObject wifi_connect; // �������
    public GameObject wifi_entering_password; // ��J�K�X���a��
    public GameObject keyboard; // ��J�K�X�Ϊ���L
    public Image wifi_entering_password_picture; // ��J�K�X���I����
    public TMP_Text inputField; // �K�X��J��
    public GameObject wifi_checking; // �s�����T�{
    public GameObject fail_picture; // ���ѹϤ�
    public GameObject congratulation_picture; // ���\�Ϥ�
    public GameObject Step4; // line�q��
    public AudioSource audioSource;
    public AudioClip computerSound; // �q���q������
    public AudioClip LineSound; // line�q������
    
    public AudioClip SuccessSound; // ���\����
    public VoiceToGPT_4 voiceToGPT_4;
    public keyboard Keyboard;
    public GameObject Wrong_bottom; // ���~wifi�����s
    

    private bool wifi_lock = true;
    private int wifi_check_num; // 0�O��������(�L�K�X)�A1�O�諸����(���K�X)
    private string password = "284@36307262";
    private Coroutine progressRoutine; // ���B(���F����X����e��)
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

    // wifi�Ϥ��ƭȧ��(�P�_�s���O�w���Τ��w��������)
    public int WifiCheckNum
    {
        get => wifi_check_num;
        set => wifi_check_num = value;
    }

    // �I���u�h�s��������
    public void wifi_remind_to_wifi_connect()
    {
        wifi_remind.SetActive(false);
        wifi_connect.SetActive(true);
    }

    // ��찲����
    public void wifi_connect_to_wrong_wifi()
    {
        wifi_connect.SetActive(false);
        wifi_checking.SetActive(true);
        wifi_check_num = 0;
    }

    // �T�w�ϥΥ��e��ܪ�����
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
        yield return new WaitForSeconds(1f); // ����@��A���q��
        audioSource.PlayOneShot(LineSound);
        Step4.SetActive(true);
    }

    public void wifi_check_if_no()
    {
        wifi_checking.SetActive(false);
        wifi_connect.SetActive(true);
    }

    // ���u����
    public void wifi_connect_to_right_wifi()
    {
        keyboard.SetActive(true);
        wifi_entering_password.SetActive(true);
    }

    // �K�X��J���A�n�T�{�K�X
    public void right_wifi_finish_password()
    {
        string input_password = Keyboard.GetPassword();
        // ��J�K�X���T
        if(input_password == password)
        {
            wifi_entering_password.SetActive(false);
            wifi_connect.SetActive(false);
            wifi_checking.SetActive(true);
            wifi_check_num = 1;
        }
        // ��J�K�X���~
        else
        {
            wifi_entering_password_picture.sprite = Wrong_password;
        }
    }

    // �K�X�����F�A���Ӻ���
    public void do_not_want_to_print_password()
    {
        keyboard.SetActive(false);
        inputField.text = "";
        wifi_entering_password.SetActive(false);
    }

    // ���}�ɮץX�{���Ϥ�
    public void openPDF()
    {
        if(wifi_check_num == 0) // �p�G�����O���w����
        {
            fail_picture.SetActive(true);
            StartCoroutine(voiceToGPT_4.Choose_wrong_wifi());
            Wrong_bottom.SetActive(false);
        }
        else // �p�G�����O�w����
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

    // ���\�q���᩵��4����٤j�����~��M�䪫��
    /*private IEnumerator Change_back_to_Choose_Object()
    {
        yield return new WaitForSeconds(4f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(4);
    }*/
}
