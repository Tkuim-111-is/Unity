using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : MonoBehaviour
{
    [Header("���w�ȱi�]�d��=�Υ�����^")]
    public Transform paper;

    [Header("�Ѽ�")]
    public float duration = 5f;       // �`�ɪ�
    public float downDistance = 0.18f; // ���U�I���Z���]�̼ҫ��ա^

    [Header("�����n")]
    public GameObject machinesound;
    public AudioSource machineLoop; // ��@�q���F/�@���n�]Loop �İ_�Ӥ]��^

    /*void Start()
    {
        if (paper == null) paper = transform;     // �S���w�N�Φۤv
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

        // �ݤ���N�n�G�n���éΧR������
        paper.gameObject.SetActive(false); // �� Destroy(paper.gameObject);
    }
}
