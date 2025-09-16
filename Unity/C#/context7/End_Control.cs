using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class End_Control : MonoBehaviour
{
    void Update()
    {
        bool xDown = OVRInput.GetDown(OVRInput.RawButton.X);               // ���� X
        bool yDown = OVRInput.GetDown(OVRInput.RawButton.Y);               // ���� Y
        bool indexDown = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);   // ���⭹����� (���U�{��)
        bool gripDown = OVRInput.GetDown(OVRInput.RawButton.LHandTrigger);    // ���⤤��/������

        // �Y�u�n X / Y / ������� �䤤���@���U�N�������G
        if (xDown || yDown || indexDown || gripDown)
        {
            PlayerPrefs.SetInt("ItemID", 6);
            SceneManager.LoadScene(4);
        }
    }
}
