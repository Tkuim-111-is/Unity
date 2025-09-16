using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsbParentIsWho : MonoBehaviour
{
    public GameObject Parent;
    public Transform Child;
    void Update()
    {
        if(Child.parent == Parent)
        {
            Child.gameObject.SetActive(false);
        }
        else
        {
            Child.gameObject.SetActive(true);
        }
    }
}
