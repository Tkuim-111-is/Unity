using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Red_Flash : MonoBehaviour
{
    public AudioSource Sound;
    public Light redLight;          // ���w�n�{�{���O
    public float flashSpeed = 0.5f; // �C���{�{���j
    public float duration = 5f;     // �`�{�{���

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

        // �쥻���A
        bool originalEnabled = redLight.enabled;
        Color originalColor = redLight.color;
        float originalIntensity = redLight.intensity;

        // �վ㦨����O�]�p�G�A�n�����{�{�^
        redLight.color = Color.red;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            redLight.enabled = !redLight.enabled;
            yield return new WaitForSeconds(flashSpeed);
            elapsed += flashSpeed;
        }

        // �٭쪬�A
        redLight.enabled = originalEnabled;
        redLight.color = originalColor;
        redLight.intensity = originalIntensity;

        flashing = false;
    }
}
