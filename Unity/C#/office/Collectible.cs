using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("此物件的唯一ID（可用名稱或自訂）")]
    public string itemId;

    [Header("外觀行為")]
    public bool setUntaggedWhenCollected = true;
    public bool hideWhenCollected = false;

    [Header("未收集時，是否把 Tag 還原為 defaultTag")]
    public bool resetTagIfNotCollected = true;
    public string defaultTag = "Check";

    void Start()
    {
        // 遊戲剛載入或場景切換回來時自動套用狀態
        if (ItemStateManager.Instance)
            ItemStateManager.Instance.ApplyToCollectible(this);
    }
}
