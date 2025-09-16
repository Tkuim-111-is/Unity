using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CongratulationEffect : MonoBehaviour
{
    public Material fadeMaterial; // �������Ϊ�����
    public GameObject shardPrefab; // ��A����"�H��"�i��
    public Transform shardParent;  // ��ĳ�� Canvas �� shard ���Ū���i��
    public int shardCount = 20;
    public float fadeDuration = 1.5f;

    private bool started = false;

    void Start()
    {
        // �A�i�H��ܦb Start ���۰�Ĳ�o�A�ΥΥ~���I�s StartEffect()
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

        // �������
        GetComponent<Image>().enabled = false;

        // ���͸H��
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
                Vector2 upward = Vector2.up * 0.5f; // �V�X�@�I���W�O�q
                rb.AddForce((randomDir + upward).normalized * 300f);

                StartCoroutine(FloatUp(rb));
            }

            Destroy(shard, 3f); // �L�X��۰ʲM��
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
