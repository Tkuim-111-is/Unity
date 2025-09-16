using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("�����󪺰ߤ@ID�]�i�ΦW�٩Φۭq�^")]
    public string itemId;

    [Header("�~�[�欰")]
    public bool setUntaggedWhenCollected = true;
    public bool hideWhenCollected = false;

    [Header("�������ɡA�O�_�� Tag �٭쬰 defaultTag")]
    public bool resetTagIfNotCollected = true;
    public string defaultTag = "Check";

    void Start()
    {
        // �C������J�γ��������^�Ӯɦ۰ʮM�Ϊ��A
        if (ItemStateManager.Instance)
            ItemStateManager.Instance.ApplyToCollectible(this);
    }
}
