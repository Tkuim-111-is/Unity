using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CongratulationEffect : MonoBehaviour
{
    public Material fadeMaterial; // 指派剛剛用的材質
    public GameObject shardPrefab; // 拖你那個"碎片"進來
    public Transform shardParent;  // 建議拖 Canvas 或 shard 的空物件進來
    public int shardCount = 20;
    public float fadeDuration = 1.5f;

    private bool started = false;

    void Start()
    {
        // 你可以選擇在 Start 內自動觸發，或用外部呼叫 StartEffect()
        StartEffect();
    }

    public void StartEffect()
    {
        if (!started)
        {
            started = true;
            StartCoroutine(FadeAndExplode());
        }
    }

    IEnumerator FadeAndExplode()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            float blend = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeMaterial.SetFloat("_Blend", blend);
            t += Time.deltaTime;
            yield return null;
        }

        fadeMaterial.SetFloat("_Blend", 1f);

        // 關掉原圖
        GetComponent<Image>().enabled = false;

        // 產生碎片
        SpawnShards();
    }

    void SpawnShards()
    {
        for (int i = 0; i < shardCount; i++)
        {
            GameObject shard = Instantiate(shardPrefab, transform.position, Quaternion.identity, shardParent);
            Rigidbody2D rb = shard.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                Vector2 upward = Vector2.up * 0.5f; // 混合一點往上力量
                rb.AddForce((randomDir + upward).normalized * 300f);

                StartCoroutine(FloatUp(rb));
            }

            Destroy(shard, 3f); // 過幾秒自動清除
        }
    }

    IEnumerator FloatUp(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.AddForce(Vector2.up * 100f);
    }
}
