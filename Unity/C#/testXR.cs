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
                UnityEngine.Debug.Log("������˸m�G" + device.name + "�A�����G" + device.characteristics);
            }
        }
        else
        {
            UnityEngine.Debug.Log("�S����������� XR �˸m�I");
        }
    }*/
    void Update()
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var device in devices)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Controller) != 0) // �u��ܱ��
            {
                bool triggerPressed = false;
                bool gripPressed = false;

                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
                {
                    UnityEngine.Debug.Log(device.name + " Ĳ�o���s (Trigger) �Q���U�I");
                }

                if (device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed) && gripPressed)
                {
                    UnityEngine.Debug.Log(device.name + " ������s (Grip) �Q���U�I");
                }
            }
        }
    }
}