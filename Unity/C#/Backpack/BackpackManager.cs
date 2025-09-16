using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackManager : MonoBehaviour
{
    public static BackpackManager Instance;

    public Transform uiContentParent;     // ContentPanel
    public GameObject itemIconPrefab;     // ItemIcon prefab
    public Transform attachPoint;         // ��W�Ū���
    public GameObject backpackUI;         // �I�] UI ���O�]�Ψ������^
    public bool justEquippedViaUI = false;

    private List<GameObject> items = new List<GameObject>();
    private Dictionary<GameObject, GameObject> itemToIcon = new Dictionary<GameObject, GameObject>();
    private GameObject currentEquippedItem = null;

    void Awake()
    {
        Instance = this;
    }

    public void AddItem(GameObject item)
    {
        if (items.Contains(item)) return;

        items.Add(item);
        item.SetActive(false);

        GameObject icon = Instantiate(itemIconPrefab, uiContentParent, false);

        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        Image iconImage = icon.transform.Find("IconImage").GetComponent<Image>();
        iconImage.sprite = item.GetComponent<ItemData>().icon;

        Button btn = icon.GetComponent<Button>();
        btn.onClick.AddListener(() => EquipItem(item));

        itemToIcon[item] = icon;
    }

    public void EquipItem(GameObject item)
    {
        if (currentEquippedItem != null)
            return; // �w�g���˳ơA�����\�P�ɨ��

        currentEquippedItem = item;
        item.SetActive(true);
        item.transform.SetParent(attachPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        if (itemToIcon.ContainsKey(item))
        {
            itemToIcon[item].SetActive(false); // ���� UI �ϥ�
        }

        Object_Interaction interaction = FindObjectOfType<Object_Interaction>();
        if (interaction != null)
        {
            interaction.isBackpackOpen = false;
        }
        backpackUI.SetActive(false); // ���K�����I�] UI
        justEquippedViaUI = true;
    }

    public void UnequipCurrentItem()
    {
        if (currentEquippedItem == null)
            return;

        GameObject item = currentEquippedItem;
        currentEquippedItem = null;

        item.SetActive(false);
        item.transform.SetParent(null);

        if (itemToIcon.ContainsKey(item))
        {
            itemToIcon[item].SetActive(true); // �A��ܦ^ UI �ϥ�
        }
    }
}
