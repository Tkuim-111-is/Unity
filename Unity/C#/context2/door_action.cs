using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class door_action : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip nock_door; // �V���n

    private Coroutine nockCoroutine; // �V�������B

    void Start()
    {
        nockCoroutine = StartCoroutine(LoopNockDoor());
    }

    // �`���V���n�A�b�}���ᰱ��V��
    IEnumerator LoopNockDoor()
    {
        while (true)
        {
            // �p�G���w�g���}
            if ((int)Variables.ActiveScene.Get("Meeting_Door_Status") == 1)
            {
                break; // ���X while �j��A���� Coroutine
            }

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(nock_door);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // �p�G�٦b���� while �j��Q����A�j����V���n
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
// (int) Variables.ActiveScene.Get("NPC_1_Status") == 1
