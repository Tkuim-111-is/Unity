using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Check_Finish_Cleaning : MonoBehaviour
{
    public Transform player; // ���a
    public OVRRayHelper rayHelper; // ���a���k��
    public LayerMask objectLayer;  // �k��W���p�g
    public GameObject Check_Canvas; // �T�{�e��
    public GameObject Document1; // ��W���1
    public GameObject Document2; // ��W���2
    public GameObject Document3; // �d�W���
    public GameObject Laptop; // ���q
    public VoiceToGPT_Try VoiceToGPT_Try; // �y���s���ʧ@�޿�}��
    public camaramove camaramove; // ���ʪ��}��
    public GameObject Congratulation; // ���⮥�ߵe��
    public AudioSource audioSource;
    public AudioClip SuccessSound; // ���\����
    public AudioClip ChineseSound; // �O�O�O�O�O�O�O~
    public AudioClip FinishSound; // �������ܵ�����
    public GameObject door; // �Ӧ������ɧګ��s
    private bool isShowingCheckCanvas = false;
    public historyinformation hf;//���v�����}��
    public End_control_context2 End_Control;
    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (!isShowingCheckCanvas && Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag("Door") && OVRInput.GetDown(OVRInput.Button.One))
            {
                door.GetComponent<BoxCollider>().enabled = false;
                Check_Canvas.SetActive(true);
                isShowingCheckCanvas = true;
            }
        }
    }

    public void Finish_Cleaning()
    {
        door.GetComponent<BoxCollider>().enabled = true;
        Check_Canvas.SetActive(false);
        isShowingCheckCanvas = false;
        Variables.ActiveScene.Set("Meeting_Door_Status", 1);
        if(Document1.activeSelf || Document2.activeSelf)
        {
            // �����ͮ�y��
            StartCoroutine(VoiceToGPT_Try.npc1_is_not_clean("�A�s��W����Ƴ��S���A�����H�N��ڭ̶i��", "�p�G�ݭn�����A�i�H�}����ڭ�"));
        }
        else if (Laptop.activeSelf)
        {
            camaramove.enabled = false; // ��w���ʡA�����A��
            player.position = new Vector3(-18.66639f, 7.05f, 52.88611f);
            player.rotation = Quaternion.Euler(0f, 104f, 0f);
            StartCoroutine(VoiceToGPT_Try.Did_not_finish_cleaning(14, "�A�����q�ù��S���A���������L���̬ݨ�\n���n�A�s��"));
        }
        else if (Document3.activeSelf)
        {
            camaramove.enabled = false;
            player.position = new Vector3(-18.66639f, 7.05f, 52.88611f);
            player.rotation = Quaternion.Euler(0f, 104f, 0f);
            StartCoroutine(VoiceToGPT_Try.Did_not_finish_cleaning(13, "�A������ٯd�b�|ĳ�Ǥ��A���������L���̶i��\n���n�A�s��"));
        }
        else
        {
            player.position = new Vector3(-18.66639f, 7.05f, 52.88611f);
            player.rotation = Quaternion.Euler(0f, 104f, 0f);
            Variables.ActiveScene.Set("NPC_1_Status", 9);
            StartCoroutine(Finish_movement());
        }
        Debug.Log("����F");
    }

    // ��NPC1���槹1_9���ʧ@
    private IEnumerator Finish_movement()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(VoiceToGPT_Try.running_time);
        string time_record = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        int time_seconds = (int)timeSpan.TotalSeconds; //�`���
        Debug.Log("�`�@�ΤF" + time_record + "��");
        while ((int)Variables.ActiveScene.Get("NPC_1_Status") != -1)
        {
            yield return null;
        }
        Congratulation.SetActive(true);
        // audioSource.PlayOneShot(SuccessSound);
        StartCoroutine(Change_back_to_Choose_Object());
        hf.PostHistoryinformation(VoiceToGPT_Try.context_error, 2,time_seconds);
        Debug.Log("���F"+ VoiceToGPT_Try.context_error+"��");
    }

    // ����4��ǰe�^�j����
    private IEnumerator Change_back_to_Choose_Object()
    {
        audioSource.PlayOneShot(ChineseSound);
        yield return new WaitForSeconds(ChineseSound.length);
        audioSource.PlayOneShot(FinishSound);
        yield return new WaitForSeconds(FinishSound.length);
        // yield return new WaitForSeconds(4f);
        // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        End_Control.enabled = true;
    }

    public void Not_Yet()
    {
        door.GetComponent<BoxCollider>().enabled = true;
        Check_Canvas.SetActive(false);
        isShowingCheckCanvas = false;
        Debug.Log("����F�ACanvas ���A�G" + Check_Canvas.activeSelf);
    }
}
