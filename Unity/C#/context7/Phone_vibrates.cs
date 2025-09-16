using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phone_vibrates : MonoBehaviour
{
    [Header("�w�]�Ѽ�")]
    public float duration = 6.7f; 
    public float posAmplitude = 0.02f; // ��m�ݰʴT��(���ة�UI��������Canvas�Y���)
    public float rotAmplitude = 2.0f;  // ����ݰʴT��(��)
    public float frequency = 45f;      // �ݰ��W�v(�V�j�V�u�g�v)
    public bool useUnscaledTime = false;
    public GameObject NPC4_phone;

    Vector3 _origLocalPos;
    Quaternion _origLocalRot;
    bool _isShaking;

    void Start()
    {
        // StartCoroutine(ShakeCoroutine());
    }

    public IEnumerator ShakeCoroutine()
    {
        NPC4_phone.SetActive(true);
        _isShaking = true;

        // �O����l����
        _origLocalPos = transform.localPosition;
        _origLocalRot = transform.localRotation;

        float t = 0f;
        float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // �W�v�������t�ס]���ס^
        float omega = frequency * Mathf.PI * 2f;

        while (t < duration)
        {
            t += dt();
            // �i�[�W�H�X�A�����ݦ^�k��۵M
            float falloff = 1f - Mathf.Clamp01(t / duration);

            // ��m�G�p�T�H���ϥ����V�X
            Vector3 offset =
                new Vector3(
                    (Mathf.PerlinNoise(Time.time * frequency, 0f) - 0.5f),
                    (Mathf.PerlinNoise(0f, Time.time * frequency) - 0.5f),
                    0f
                ) * (posAmplitude * falloff);

            // ����GZ �b���L���k�\�]�����b��W�`���ĪG�^
            float angle = Mathf.Sin(Time.time * omega) * rotAmplitude * falloff;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            transform.localPosition = _origLocalPos + offset;
            transform.localRotation = _origLocalRot * rot;

            yield return null;
        }

        // �٭�
        transform.localPosition = _origLocalPos;
        transform.localRotation = _origLocalRot;

        _isShaking = false;
        NPC4_phone.SetActive(false);
    }
}
