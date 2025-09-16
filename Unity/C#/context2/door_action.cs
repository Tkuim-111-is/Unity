using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class door_action : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip nock_door; // 敲門聲

    private Coroutine nockCoroutine; // 敲門的異步

    void Start()
    {
        nockCoroutine = StartCoroutine(LoopNockDoor());
    }

    // 循環敲門聲，在開門後停止敲門
    IEnumerator LoopNockDoor()
    {
        while (true)
        {
            // 如果門已經打開
            if ((int)Variables.ActiveScene.Get("Meeting_Door_Status") == 1)
            {
                break; // 跳出 while 迴圈，結束 Coroutine
            }

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(nock_door);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // 如果還在播但 while 迴圈被停止，強制停止播放敲門聲
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
// (int) Variables.ActiveScene.Get("NPC_1_Status") == 1
