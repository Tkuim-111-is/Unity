using System.Collections;
using UnityEngine;

public class PlayerPushBack : MonoBehaviour
{
    public float pushDistance = 1.0f;
    public float pushDuration = 0.3f; // 推開所花的時間

    private bool isPushing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && !isPushing)
        {
            Vector3 pushDir = -transform.forward;
            pushDir.y = 0;
            pushDir.Normalize();

            Vector3 targetPosition = transform.position + pushDir * pushDistance;
            StartCoroutine(SmoothPushBack(targetPosition));
        }
    }

    private IEnumerator SmoothPushBack(Vector3 targetPos)
    {
        isPushing = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / pushDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isPushing = false;
    }
}
