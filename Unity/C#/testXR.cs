using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

public class testXR : MonoBehaviour
{
    /*void Update()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        if (devices.Count > 0)
        {
            foreach (var device in devices)
            {
                UnityEngine.Debug.Log("偵測到裝置：" + device.name + "，類型：" + device.characteristics);
            }
        }
        else
        {
            UnityEngine.Debug.Log("沒有偵測到任何 XR 裝置！");
        }
    }*/
    void Update()
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var device in devices)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Controller) != 0) // 只顯示控制器
            {
                bool triggerPressed = false;
                bool gripPressed = false;

                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                {
                    UnityEngine.Debug.Log(device.name + " 觸發按鈕 (Trigger) 被按下！");
                }

                if (device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed) && gripPressed)
                {
                    UnityEngine.Debug.Log(device.name + " 握把按鈕 (Grip) 被按下！");
                }
            }
        }
    }
}