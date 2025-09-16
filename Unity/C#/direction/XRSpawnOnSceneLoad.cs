using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRSpawnOnSceneLoad : MonoBehaviour
{
    [SerializeField] GameObject spawnPoint;  // �������w GameObject�A���� Tag

    [Tooltip("�u��������¦V(�O�d���a���Y/�C�Y)")]
    public bool alignYawOnly = true;

    IEnumerator Start()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (spawnPoint == null)
        {
            Debug.LogWarning("�S�����w SpawnPoint�A���a���|����");
            yield break;
        }

        // �ǰe���a
        transform.position = spawnPoint.transform.position;

        if (alignYawOnly)
        {
            var yRot = spawnPoint.transform.eulerAngles.y;
            var rot = transform.eulerAngles;
            rot.y = yRot;
            transform.eulerAngles = rot;
        }
        else
        {
            transform.rotation = spawnPoint.transform.rotation;
        }
    }
}
