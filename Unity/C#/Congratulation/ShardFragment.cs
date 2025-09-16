using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardFragment : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;
    private float time;
    private float switchTime;

    [Header("調整參數")]
    public float explosionForce = 600f;
    public float floatSpeed = 20f;
    public float floatLerpSpeed = 1.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        direction = Random.insideUnitCircle.normalized;
        speed = Random.Range(0.5f, 1.2f) * explosionForce;
        switchTime = Random.Range(0.3f, 0.6f);
        rb.AddForce(direction * speed);
        rb.gravityScale = 0;
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time > switchTime)
        {
            rb.velocity += Vector2.up * floatSpeed * Time.deltaTime;
        }
    }
}
