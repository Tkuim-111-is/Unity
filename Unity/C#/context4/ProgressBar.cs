using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    public Image targetImage; // �I���Ϥ�
    public Sprite Wrong_Wifi; // ���~�ୱ���Ϥ�
    public TMP_Text percentText;
    public GameObject Step5;
    public GameObject Step6;
    public GameObject confirmPanel_1; // �T�{����1(�@�������]���}�l�])
    public GameObject confirmPanel_2; // �T�{����2(�]���]��@�b)
    public GameObject run_tail; // �]��
    public GameObject pdf; // pdf�ɮ�
    public getting_wifi wifiScript; // ����getting_wifi������

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

    // �Ĥ@�Ӵ��ܿ�ܥ��T
    public void first_hint_press_yes()
    {
        wifiScript.WifiCheckNum = 0;
        targetImage.sprite = Wrong_Wifi;
        confirmPanel_1.SetActive(false);
        run_tail.SetActive(true);
        progressRoutine = StartCoroutine(Wrong_wifi_run_tail());
    }

    // �Ĥ@�Ӵ��ܿ�ܨ���
    public void OnDownloadButtonClicked()
    {
        confirmPanel_1.SetActive(false);
        if (progressRoutine == null)
        {
            run_tail.SetActive(true);
            progressRoutine = StartCoroutine(PhaseOne()); // ��1���q
        }
    }

    // �ĤG�Ӵ��ܿ�ܥ��T
    public void second_hint_press_yes()
    {
        wifiScript.WifiCheckNum = 0;
        targetImage.sprite = Wrong_Wifi;
        confirmPanel_2.SetActive(false);
        run_tail.SetActive(true);
        progressRoutine = StartCoroutine(Wrong_wifi_run_tail());
    }

    // �ĤG�Ӵ��ܿ�ܨ���
    public void OnConfirmButtonClicked()
    {
        confirmPanel_2.SetActive(false);
        if (progressRoutine == null)
        {
            run_tail.SetActive(true);
            progressRoutine = StartCoroutine(PhaseTwo()); // ��2���q
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

        SetProgress(0.01f); // �O�Ҩ� 1%
        progressRoutine = null;

        // ��ܽT�{����
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

        SetProgress(1f); // �̲ק���
        pdf.SetActive(true);
        progressRoutine = null;
    }

    // �ĤG�Ӵ��ܿ�ܨ���
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
