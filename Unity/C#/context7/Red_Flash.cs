using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Red_Flash : MonoBehaviour
{
    public AudioSource Sound;
    public Light redLight;          // 指定要閃爍的燈
    public float flashSpeed = 0.5f; // 每次閃爍間隔
    public float duration = 5f;     // 總閃爍秒數

    private bool flashing = false;

    public void StartFlashing()
    {
        if (!flashing && redLight != null)
            StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        Sound.Play();
        flashing = true;

        // 原本狀態
        bool originalEnabled = redLight.enabled;
        Color originalColor = redLight.color;
        float originalIntensity = redLight.intensity;

        // 調整成紅色燈（如果你要全紅閃爍）
        redLight.color = Color.red;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            redLight.enabled = !redLight.enabled;
            yield return new WaitForSeconds(flashSpeed);
            elapsed += flashSpeed;
        }

        // 還原狀態
        redLight.enabled = originalEnabled;
        redLight.color = originalColor;
        redLight.intensity = originalIntensity;

        flashing = false;
    }
}
