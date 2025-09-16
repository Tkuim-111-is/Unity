using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
    public Material fadeMaterial;
    public GameObject shardPrefab;
    public Transform shardParent;
    public int shardCount = 30;
    public float fadeDuration = 1.5f;
    public VoiceToGPT VoiceToGPT;

    private bool triggered = false;

    public void StartFade()
    {
        if (!triggered)
        {
            triggered = true;
            StartCoroutine(FadeAndExplode());
        }
    }

    IEnumerator FadeAndExplode()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            float blend = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeMaterial.SetFloat("_Blend", blend);
            t += Time.deltaTime;
            yield return null;
        }

        fadeMaterial.SetFloat("_Blend", 1f);
        GetComponent<Image>().enabled = false;

        SpawnShards();
        StartCoroutine(VoiceToGPT.Context6_Ending());
    }

    void SpawnShards()
    {
        for (int i = 0; i < shardCount; i++)
        {
            GameObject shard = Instantiate(shardPrefab, transform.position, Quaternion.identity, shardParent);

            shard.transform.localScale = Vector3.one * 0.2f;

            //Destroy(shard, 3.5f); // 自動清除
        }
    }
}
