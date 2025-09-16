using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start_Button : MonoBehaviour
{
    public GameObject Step4;
    public GameObject Step5;

    void OnEnable()
    {
        Step4.SetActive(false);
        Step5.SetActive(false);
    }
    public void Step4_to_Step5()
    {
        Step4.SetActive(false);
        Step5.SetActive(true);
    }
}
