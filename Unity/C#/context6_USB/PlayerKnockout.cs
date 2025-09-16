using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerKnockout : MonoBehaviour
{
    public Image fadeImage;
    public AudioSource AudioSource;
    public AudioClip HitSound;
    public float fadeDuration = 1f;   // 黑頻持續時間
    public float flyDuration = 1f;    // 飛行時間
    public float arcHeight = 2f;
    public float flyDistance = 5f;
    public float groundY = 0f;

    private bool isFading = false;

    void Update()
    {
        if (!isFading && (int)Variables.ActiveScene.Get("NPC_1_Status") == 4)
        {
            AudioSource.PlayOneShot(HitSound);
            StartCoroutine(KnockoutSequence());
        }
    }

    IEnumerator KnockoutSequence()
    {
        isFading = true;

        Vector3 startPos = transform.position;
        Vector3 pushDir = -transform.forward;

        Vector3 endPos = startPos + pushDir * flyDistance;
        endPos.y = groundY;

        Vector3 controlPoint = startPos + pushDir * (flyDistance / 2f) + Vector3.up * arcHeight;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(transform.eulerAngles + new Vector3(90f, 0, 0));

        Color startColor = fadeImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1);

        float elapsed = 0f;
        float maxDuration = Mathf.Max(flyDuration, fadeDuration);

        while (elapsed < maxDuration)
        {
            float tFly = Mathf.Clamp01(elapsed / flyDuration);
            float tFade = Mathf.Clamp01(elapsed / fadeDuration);

            // 飛行位置
            Vector3 a = Vector3.Lerp(startPos, controlPoint, tFly);
            Vector3 b = Vector3.Lerp(controlPoint, endPos, tFly);
            transform.position = Vector3.Lerp(a, b, tFly);

            // 同時旋轉
            transform.rotation = Quaternion.Slerp(startRot, endRot, tFly);

            // 同時黑屏淡出
            fadeImage.color = Color.Lerp(startColor, endColor, tFade);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;
        fadeImage.color = endColor;

        yield return new WaitForSeconds(1f);
        PlayerPrefs.SetInt("ItemID", 7);
        PlayerPrefs.SetInt("Wrong", PlayerPrefs.GetInt("Wrong")+1);
        SceneManager.LoadScene(4);
    }
}
