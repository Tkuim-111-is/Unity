using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    public Image targetImage; // 背景圖片
    public Sprite Wrong_Wifi; // 錯誤桌面的圖片
    public TMP_Text percentText;
    public GameObject Step5;
    public GameObject Step6;
    public GameObject confirmPanel_1; // 確認視窗1(一按取消跑條開始跑)
    public GameObject confirmPanel_2; // 確認視窗2(跑條跑到一半)
    public GameObject run_tail; // 跑條
    public GameObject pdf; // pdf檔案
    public getting_wifi wifiScript; // 掛載getting_wifi的物件

    private float currentValue = 0f;
    private Coroutine progressRoutine;

    void OnEnable()
    {
        SetProgress(0f);
        Step6.SetActive(false);
        confirmPanel_1.SetActive(false);
        confirmPanel_2.SetActive(false);
    }

    public void Step5_to_Step6()
    {
        if(wifiScript.WifiCheckNum == 0)
        {
            Step6.SetActive(true);
            run_tail.SetActive(true);
            progressRoutine = StartCoroutine(Wrong_wifi_run_tail());
        }
        else
        {
            Step6.SetActive(true);
            confirmPanel_1.SetActive(true);
            run_tail.SetActive(false);
        }
            
    }

    // 第一個提示選擇正確
    public void first_hint_press_yes()
    {
        wifiScript.WifiCheckNum = 0;
        targetImage.sprite = Wrong_Wifi;
        confirmPanel_1.SetActive(false);
        run_tail.SetActive(true);
        progressRoutine = StartCoroutine(Wrong_wifi_run_tail());
    }

    // 第一個提示選擇取消
    public void OnDownloadButtonClicked()
    {
        confirmPanel_1.SetActive(false);
        if (progressRoutine == null)
        {
            run_tail.SetActive(true);
            progressRoutine = StartCoroutine(PhaseOne()); // 第1階段
        }
    }

    // 第二個提示選擇正確
    public void second_hint_press_yes()
    {
        wifiScript.WifiCheckNum = 0;
        targetImage.sprite = Wrong_Wifi;
        confirmPanel_2.SetActive(false);
        run_tail.SetActive(true);
        progressRoutine = StartCoroutine(Wrong_wifi_run_tail());
    }

    // 第二個提示選擇取消
    public void OnConfirmButtonClicked()
    {
        confirmPanel_2.SetActive(false);
        if (progressRoutine == null)
        {
            run_tail.SetActive(true);
            progressRoutine = StartCoroutine(PhaseTwo()); // 第2階段
        }
    }

    private IEnumerator PhaseOne()
    {
        float time = 0f;
        float duration = 5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Lerp(0f, 0.01f, time / duration);
            SetProgress(progress);
            yield return null;
        }

        SetProgress(0.01f); // 保證到 1%
        progressRoutine = null;

        // 顯示確認視窗
        run_tail.SetActive(false);
        confirmPanel_2.SetActive(true);
    }

    private IEnumerator PhaseTwo()
    {
        float time = 0f;
        float duration = 5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Lerp(0.01f, 1f, time / duration);
            SetProgress(progress);
            yield return null;
        }

        SetProgress(1f); // 最終完成
        pdf.SetActive(true);
        progressRoutine = null;
    }

    // 第二個提示選擇取消
    public void WrongWifiRunTail()
    {
        run_tail.SetActive(true);
        progressRoutine = StartCoroutine(Wrong_wifi_run_tail());
    }

    private IEnumerator Wrong_wifi_run_tail()
    {
        float time = 0f;
        float duration = 10f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Lerp(0.01f, 1f, time / duration);
            SetProgress(progress);
            yield return null;
        }

        SetProgress(1f);
        pdf.SetActive(true);
        progressRoutine = null;
    }

    private void SetProgress(float value)
    {
        currentValue = Mathf.Clamp01(value);
        fillImage.fillAmount = currentValue;

        if (percentText != null)
        {
            percentText.text = Mathf.RoundToInt(currentValue * 100f) + "%";
        }
    }
}
