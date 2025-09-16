using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
using UnityEngine.EventSystems;
using System.Diagnostics;

public class VRUIButtonSelector : MonoBehaviour
{
    public List<Selectable> uiElements; // 可選擇的 UI 元件（Button 或 InputField）
    private int selectedIndex = 0; // 當前選擇的 UI 索引
    private bool canNavigate = true; // 防止連續觸發

    void Start()
    {
        HighlightElement(selectedIndex);
    }

    void Update()
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);

        foreach (var device in devices)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                Vector2 thumbstickValue;
                bool triggerPressed = false;

                // 方向鍵控制 UI 元件選擇
                if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickValue))
                {
                    if (canNavigate)
                    {
                        if (thumbstickValue.y > 0.5f) // 向上
                        {
                            StartCoroutine(NavigateWithDelay(-1)); // 選擇上一個
                        }
                        else if (thumbstickValue.y < -0.5f) // 向下
                        {
                            StartCoroutine(NavigateWithDelay(1)); // 選擇下一個
                        }
                    }
                }

                // Trigger 按鈕點擊當前選擇的 UI 元件
                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                {
                    ClickSelectedElement();
                }
            }
        }
    }

    // 協程：延遲切換 UI
    IEnumerator NavigateWithDelay(int direction)
    {
        canNavigate = false; // 禁止連續輸入

        if (direction == -1)
        {
            SelectPreviousElement();
        }
        else if (direction == 1)
        {
            SelectNextElement();
        }

        yield return new WaitForSeconds(0.2f); // 休眠 0.2 秒，防止連續觸發
        canNavigate = true; // 允許下一次輸入
    }

    // 選擇上一個 UI 元件
    void SelectPreviousElement()
    {
        selectedIndex = (selectedIndex - 1 + uiElements.Count) % uiElements.Count;
        HighlightElement(selectedIndex);
    }

    // 選擇下一個 UI 元件
    void SelectNextElement()
    {
        selectedIndex = (selectedIndex + 1) % uiElements.Count;
        HighlightElement(selectedIndex);
    }

    // 點擊當前選擇的 UI 元件
    void ClickSelectedElement()
    {
        Selectable selectedElement = uiElements[selectedIndex];

        if (selectedElement != null)
        {
            UnityEngine.Debug.Log("點擊 UI：" + selectedElement.name);

            if (selectedElement is Button)
            {
                ((Button)selectedElement).onClick.Invoke();
            }
            else if (selectedElement is TMP_InputField)
            {
                TMP_InputField inputField = (TMP_InputField)selectedElement;
                inputField.Select();
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            }
            else if (selectedElement is InputField)
            {
                InputField inputField = (InputField)selectedElement;
                inputField.Select();
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            }
        }
    }

    // 高亮顯示當前選擇的 UI 元件
    public void HighlightElement(int index)
    {
        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i] is Button)
            {
                Button button = (Button)uiElements[i];
                ColorBlock cb = button.colors;
                cb.normalColor = (i == index) ? Color.yellow : Color.white;
                button.colors = cb;
            }
            else if (uiElements[i] is TMP_InputField || uiElements[i] is InputField)
            {
                UnityEngine.UI.Image inputFieldImage = uiElements[i].GetComponent<UnityEngine.UI.Image>();
                if (inputFieldImage)
                {
                    inputFieldImage.color = (i == index) ? Color.yellow : Color.white;
                }
            }
        }
    }
}
