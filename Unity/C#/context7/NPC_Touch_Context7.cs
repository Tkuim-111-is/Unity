using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC_Touch_Context7 : MonoBehaviour
{
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public VoiceToGPT_7 voiceToGPT_7;
    public GameObject NPC1_coordinate;
    public GameObject NPC2_coordinate;
    public GameObject NPC3_coordinate;
    public GameObject NPC4_coordinate;
    public GameObject NPC5_coordinate;

    public Transform NPC1_place;
    public Transform NPC2_place;
    public Transform NPC3_place;
    public Transform NPC4_place;
    public Transform NPC5_place;
    public Transform player;

    private bool callNPC1 = true;
    private bool callNPC2 = true;
    private bool callNPC3 = true;
    private bool callNPC4 = true;
    private bool callNPC5 = true;
    // Update is called once per frame
    void Update()
    {
        NPC_action();
    }
    void NPC_action()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag("NPC") && OVRInput.GetDown(OVRInput.Button.One))
            {
                const float eps = 0.01f;  // ���u�e�ԫ�
                if (callNPC1 && hit.collider.name == "NPC_1")
                {
                    Vector3 local = NPC1_place.InverseTransformPoint(player.position);
                    local.y = 0f; // �u�b�����W�P�_
                    if (local.x < -eps) // ����
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_1_Status", 21, "��ڦ��ƻ�ƶ�?", 1));
                    }
                    else// �k��
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_1_Status", 22, "��ڦ��ƻ�ƶ�?", 1));
                    }
                    NPC1_coordinate.SetActive(false);
                    callNPC1 = false;
                }
                else if (callNPC2 && hit.collider.name == "NPC_2")
                {
                    Vector3 local = NPC2_place.InverseTransformPoint(player.position);
                    local.y = 0f; // �u�b�����W�P�_
                    if (local.x < -eps) // ����
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_2_Status", 21, "��ڦ��ƻ�ƶ�?", 2));
                    }
                    else
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_2_Status", 22, "��ڦ��ƻ�ƶ�?", 2));
                    }
                    NPC2_coordinate.SetActive(false);
                    callNPC2 = false;
                }
                else if (callNPC3 && hit.collider.name == "NPC_3")
                {
                    Vector3 local = NPC3_place.InverseTransformPoint(player.position);
                    local.y = 0f; // �u�b�����W�P�_
                    if (local.x < -eps) // ����
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_3_Status", 21, "��ڦ��ƻ�ƶ�?", 3));
                    }
                    else
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_3_Status", 22, "��ڦ��ƻ�ƶ�?", 3)); ;
                    }
                    NPC3_coordinate.SetActive(false);
                    callNPC3 = false;
                }
                else if (callNPC4 && hit.collider.name == "NPC_4")
                {
                    Vector3 local = NPC4_place.InverseTransformPoint(player.position);
                    local.y = 0f; // �u�b�����W�P�_
                    if (local.x < -eps) // ����
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_4_Status", 21, "�o�Ӭ��ʭn����򥻸�Ƥ~��a�H", 4));
                    }
                    else
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_4_Status", 22, "�o�Ӭ��ʭn����򥻸�Ƥ~��a�H", 4));
                    }
                    NPC4_coordinate.SetActive(false);
                    callNPC4 = false;
                }
                else if (callNPC5 && hit.collider.name == "NPC_5")
                {
                    Vector3 local = NPC5_place.InverseTransformPoint(player.position);
                    local.y = 0f; // �u�b�����W�P�_
                    if (local.x < -eps) // ����
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_5_Status", 21, "�o�O�Ȥ�ǨӪ��F��ܡH�ݰ_�өǩǪ��K�K", 5));
                    }
                    else
                    {
                        StartCoroutine(voiceToGPT_7.Turn_Left_or_Right("NPC_5_Status", 22, "�o�O�Ȥ�ǨӪ��F��ܡH�ݰ_�өǩǪ��K�K", 5));
                    }
                    NPC5_coordinate.SetActive(false);
                    callNPC5 = false;
                }
            }
        }

        if (!callNPC1 && (int)Variables.ActiveScene.Get("NPC_1_Status") == -2)
        {
            NPC1_coordinate.SetActive(true);
            callNPC1 = true;
            voiceToGPT_7.npc_trigger = false;
        }

        if (!callNPC2 && (int)Variables.ActiveScene.Get("NPC_2_Status") == -2)
        {
            NPC2_coordinate.SetActive(true);
            callNPC2 = true;
            voiceToGPT_7.npc_trigger = false;
        }

        if (!callNPC3 && (int)Variables.ActiveScene.Get("NPC_3_Status") == -2)
        {
            NPC3_coordinate.SetActive(true);
            callNPC3 = true;
            voiceToGPT_7.npc_trigger = false;
        }

        if (!callNPC4 && (int)Variables.ActiveScene.Get("NPC_4_Status") == -2)
        {
            NPC4_coordinate.SetActive(true);
            callNPC4 = true;
            voiceToGPT_7.npc_trigger = false;
        }

        if (!callNPC5 && (int)Variables.ActiveScene.Get("NPC_5_Status") == -2)
        {
            NPC5_coordinate.SetActive(true);
            callNPC5 = true;
            voiceToGPT_7.npc_trigger = false;
        }
    }
}
