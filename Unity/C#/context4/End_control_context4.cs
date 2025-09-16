using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class End_control_context4 : MonoBehaviour
{
    void Update()
    {
        bool xDown = OVRInput.GetDown(OVRInput.RawButton.X);               // 左手 X
        bool yDown = OVRInput.GetDown(OVRInput.RawButton.Y);               // 左手 Y
        bool indexDown = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);   // 左手食指扳機 (按下臨界)
        bool gripDown = OVRInput.GetDown(OVRInput.RawButton.LHandTrigger);    // 左手中指/握把扳機

        // 若只要 X / Y / 食指扳機 其中任一按下就切場景：
        if (xDown || yDown || indexDown || gripDown)
        {
            PlayerPrefs.SetInt("ItemID", 7);
            SceneManager.LoadScene(4);
        }
    }
}
