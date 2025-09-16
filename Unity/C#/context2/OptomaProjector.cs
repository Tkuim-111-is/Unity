using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptomaProjector : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public GameObject Screen; //��v���e��
    public GameObject Screen_Black; //��v�����W�e��(�N���q�w�Q����)
    public Check_Finish_Cleaning Check_Finish_Cleaning;
    public GameObject Check_Canvas; // �T�{�e��
    public GameObject Congratulation; // ���⮥�ߵe��

    private bool door_open_bool = true;

    void Start()
    {
        Check_Finish_Cleaning.enabled = false;
        Check_Canvas.SetActive(false);
        Congratulation.SetActive(false);
        Screen_Black.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.name == "OptomaProjector" && OVRInput.GetDown(OVRInput.Button.One))
            {
                Screen.SetActive(false);
                if (Screen_Black.activeSelf)
                {
                    Screen_Black.SetActive(false);
                }
            }
            if (hit.collider.CompareTag("Check") && OVRInput.GetDown(OVRInput.Button.One))
            {
                if (hit.collider.name == "laptop")
                {
                    if (Screen.activeSelf)
                    {
                        Screen.SetActive(false);
                        Screen_Black.SetActive(true);
                    }
                }
            }
            if (door_open_bool && hit.collider.CompareTag("Door") && OVRInput.GetDown(OVRInput.Button.One))
            {
                door_open_bool = false;
                Variables.ActiveScene.Set("Meeting_Door_Status", 1);
                Check_Finish_Cleaning.enabled = true;
            }
        }
    }
}
