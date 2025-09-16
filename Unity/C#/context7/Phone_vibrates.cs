using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phone_vibrates : MonoBehaviour
{
    [Header("預設參數")]
    public float duration = 6.7f; 
    public float posAmplitude = 0.02f; // 位置抖動幅度(公尺或UI像素換成Canvas縮放後)
    public float rotAmplitude = 2.0f;  // 旋轉抖動幅度(度)
    public float frequency = 45f;      // 抖動頻率(越大越「狂」)
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

        // 記錄原始姿勢
        _origLocalPos = transform.localPosition;
        _origLocalRot = transform.localRotation;

        float t = 0f;
        float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // 頻率換成角速度（弧度）
        float omega = frequency * Mathf.PI * 2f;

        while (t < duration)
        {
            t += dt();
            // 可加上淡出，讓尾端回歸更自然
            float falloff = 1f - Mathf.Clamp01(t / duration);

            // 位置：小幅隨機＋正弦混合
            Vector3 offset =
                new Vector3(
                    (Mathf.PerlinNoise(Time.time * frequency, 0f) - 0.5f),
                    (Mathf.PerlinNoise(0f, Time.time * frequency) - 0.5f),
                    0f
                ) * (posAmplitude * falloff);

            // 旋轉：Z 軸輕微左右擺（手機放在桌上常見效果）
            float angle = Mathf.Sin(Time.time * omega) * rotAmplitude * falloff;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            transform.localPosition = _origLocalPos + offset;
            transform.localRotation = _origLocalRot * rot;

            yield return null;
        }

        // 還原
        transform.localPosition = _origLocalPos;
        transform.localRotation = _origLocalRot;

        _isShaking = false;
        NPC4_phone.SetActive(false);
    }
}
