using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : MonoBehaviour
{
    [Header("指定紙張（留空=用本物件）")]
    public Transform paper;

    [Header("參數")]
    public float duration = 5f;       // 總時長
    public float downDistance = 0.18f; // 往下沉的距離（依模型調）

    [Header("機器聲")]
    public GameObject machinesound;
    public AudioSource machineLoop; // 放一段馬達/輥輪聲（Loop 勾起來也行）

    /*void Start()
    {
        if (paper == null) paper = transform;     // 沒指定就用自己
        StartCoroutine(Run());
    }*/

    public IEnumerator Run()
    {
        machinesound.SetActive(true);
        Vector3 start = paper.localPosition;
        Vector3 end = start + Vector3.down * downDistance;

        if (machineLoop) machineLoop.Play();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            paper.localPosition = Vector3.Lerp(start, end, u);
            yield return null;
        }

        if (machineLoop) machineLoop.Stop();

        // 看不到就好：要隱藏或刪除都行
        paper.gameObject.SetActive(false); // 或 Destroy(paper.gameObject);
    }
}
