using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(SphereCollider))]
public class NPC_area_movement : MonoBehaviour
{
    public string PlayerTag = "player"; // ���a Tag
    public string VariableName = "NPC_1_Status"; // �����ܼƦW��
    //---------------NPC1������---------------
    public GameObject NPC1_start;
    public GameObject NPC1_video; // �v��
    public GameObject NPC1_video_end;
    //---------------NPC3������---------------
    public GameObject NPC3_start;
    public GameObject NPC3_video; // �v��
    public GameObject NPC3_video_end;

    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true; // �T�O�O Trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTag))
        {
            Variables.ActiveScene.Set(VariableName, 1);
        }
        if(VariableName == "NPC_1_Status")
        {
            NPC1_start.SetActive(false);
            NPC1_video.SetActive(true);
            StartCoroutine(WaitForNPC1VideoEnd());
        }
        if (VariableName == "NPC_3_Status")
        {
            NPC3_start.SetActive(false);
            NPC3_video.SetActive(true);
            StartCoroutine(WaitForNPC3VideoEnd());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PlayerTag))
        {
            Variables.ActiveScene.Set(VariableName, 0);
        }
    }

    IEnumerator WaitForNPC1VideoEnd()
    {
        yield return new WaitForSeconds(6f);
        NPC1_video.SetActive(false);
        NPC1_video_end.SetActive(true);
    }

    IEnumerator WaitForNPC3VideoEnd()
    {
        yield return new WaitForSeconds(52f);
        NPC3_video.SetActive(false);
        NPC3_video_end.SetActive(true);
    }
}




// Scene ��ı��Ĳ�o�d��
/*private void OnDrawGizmosSelected()
{
    SphereCollider col = GetComponent<SphereCollider>();
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, col.radius);
}*/