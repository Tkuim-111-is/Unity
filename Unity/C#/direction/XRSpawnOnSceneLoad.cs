using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRSpawnOnSceneLoad : MonoBehaviour
{
    [SerializeField] GameObject spawnPoint;  // 直接指定 GameObject，不用 Tag

    [Tooltip("只對齊水平朝向(保留玩家抬頭/低頭)")]
    public bool alignYawOnly = true;

    IEnumerator Start()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (spawnPoint == null)
        {
            Debug.LogWarning("沒有指定 SpawnPoint，玩家不會移動");
            yield break;
        }

        // 傳送玩家
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
