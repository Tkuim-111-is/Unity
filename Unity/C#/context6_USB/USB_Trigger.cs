using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USB_Trigger : MonoBehaviour
{
    public USB_Controller controller;
    void OnTriggerEnter(Collider other){
        if (other.CompareTag("Check")){
            controller.Explode();
        }
    }
}
