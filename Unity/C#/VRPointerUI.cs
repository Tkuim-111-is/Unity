using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class VRPointerUI : MonoBehaviour
{
    public OVRRayHelper rayHelper; // 使用 OVRRayHelper 替代 LineRenderer
    public LayerMask uiLayer; // 只偵測 UI 的 Layer
    public Sprite Line_message_picture;  // 情境4的Line訊息圖片
    public Sprite Line_download_picture; // 情境4的Line下載圖片

    private Selectable currentTarget;
    private Dictionary<Selectable, Color> originalColors = new Dictionary<Selectable, Color>();
    private bool aButtonPressed = false; // 當前幀 A 鍵是否按下
    private bool isButtonSleep = false; // 是否正在睡眠中

    void Update()
    {
        DetectUIElement();  // 偵測 UI 物件
        HandleAButtonClick();  // 處理 A 按鈕點擊事件
    }

    // **使用 OVRRayHelper 來偵測 UI**
    void DetectUIElement()
    {
        if (rayHelper == null) return;

        // 從 OVRRayHelper 取得射線資訊
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, uiLayer))
        {
            Selectable uiElement = hit.collider.GetComponent<Selectable>();

            if (uiElement != null)
            {
                HighlightElement(uiElement);
                currentTarget = uiElement;
            }
            else
            {
                TMP_InputField inputField = hit.collider.GetComponent<TMP_InputField>();
                if (inputField != null)
                {
                    HighlightElement(inputField);
                    currentTarget = inputField;
                }
            }
        }
        else
        {
            ClearHighlight();
            currentTarget = null;
        }
    }

    void HandleAButtonClick()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);

        foreach (var device in devices)
        {
            // 檢查 A 鍵是否被按下
            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool aButtonPressedThisFrame) && aButtonPressedThisFrame)
            {
                if (!aButtonPressed && !isButtonSleep)  // 確保在未處於睡眠狀態下才執行點擊
                {
                    ClickElement();
                    StartCoroutine(ButtonSleep(0.5f)); // 延遲 0.5 秒後讓按鈕恢復可點擊
                    aButtonPressed = true;  // 記錄當前幀 A 鍵已經被按下
                }
            }
            else
            {
                aButtonPressed = false;  // 如果 A 鍵被釋放，重置按鈕狀態
            }
        }
    }

    void ClickElement()
    {
        if (currentTarget == null) return;

        if (currentTarget is Button button)
        {
            button.onClick.Invoke();
        }
        else if (currentTarget is TMP_InputField inputField)
        {
            inputField.Select();
            inputField.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }
    }

    void HighlightElement(Selectable element)
    {
        ClearHighlight();

        if (element is Button button)
        {
            // 檢查是否為你指定的那個按鈕，例如名稱為 "檔案下載"
            if (button.gameObject.name == "檔案下載")
            {
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    // 儲存原始圖片
                    if (!originalColors.ContainsKey(button))
                    {
                        originalColors[button] = image.color;
                    }

                    image.sprite = Line_download_picture; // 更換 Source Image
                }
            }
            else
            {
                // 一般情況下改變顏色
                ColorBlock cb = button.colors;
                originalColors[button] = cb.normalColor;
                cb.normalColor = Color.yellow;
                button.colors = cb;
            }
        }
        else if (element is TMP_InputField inputField)
        {
            Image inputFieldImage = inputField.GetComponent<Image>();
            if (inputFieldImage)
            {
                originalColors[inputField] = inputFieldImage.color;
                inputFieldImage.color = Color.yellow;
            }
        }
    }

    void ClearHighlight()
    {
        if (currentTarget == null) return;

        if (currentTarget is Button button)
        {
            if (button.gameObject.name == "檔案下載")
            {
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    // 還原成原本的圖片
                    image.sprite = Line_message_picture; // 你可以另外存原始圖片，如果需要
                }
            }
            else if (originalColors.TryGetValue(button, out Color originalColor))
            {
                ColorBlock cb = button.colors;
                cb.normalColor = originalColor;
                button.colors = cb;
            }
        }
    }

    // Button Sleep: 停止按鈕點擊 1 秒
    IEnumerator ButtonSleep(float sleepTime)
    {
        isButtonSleep = true;  // 按鈕進入睡眠狀態
        yield return new WaitForSeconds(sleepTime); // 等待指定時間
        isButtonSleep = false;  // 恢復按鈕可點擊狀態
    }
}
