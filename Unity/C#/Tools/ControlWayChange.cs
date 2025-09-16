using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlWayChange : MonoBehaviour
{
    public camaramove_walk movementControl; 
    public camaramove controllerControl; 

    public void ChangeControl()
    {
        if(movementControl.enabled)
        {
            movementControl.enabled = false;
            controllerControl.enabled = true;
        }
        else
        {
            movementControl.enabled = true;
            controllerControl.enabled = false;
        }
    }
}
