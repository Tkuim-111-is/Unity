using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class AutoUIElementFinder : MonoBehaviour
{
    public VRUIButtonSelector buttonSelector; // 連結 VRUIButtonSelector

    void Start()
    {
        if (buttonSelector == null)
        {
            buttonSelector = GetComponent<VRUIButtonSelector>();
        }

        if (buttonSelector != null)
        {
            FindUIElements();
        }
        else
        {
            UnityEngine.Debug.LogError("VRUIButtonSelector 未附加到物件上！");
        }
    }

    void FindUIElements()
    {
        // 找到所有 Selectable UI（Button, TMP_InputField, InputField）
        Selectable[] allUI = FindObjectsOfType<Selectable>();

        // 清空現有的列表
        buttonSelector.uiElements.Clear();

        foreach (Selectable ui in allUI)
        {
            if (ui is Button || ui is TMP_InputField || ui is InputField)
            {
                buttonSelector.uiElements.Add(ui);
            }
        }

        UnityEngine.Debug.Log($"發現 {buttonSelector.uiElements.Count} 個 UI 元件並加入至列表。");

        // 預設選取第一個
        if (buttonSelector.uiElements.Count > 0)
        {
            buttonSelector.HighlightElement(0);
        }
    }
}
