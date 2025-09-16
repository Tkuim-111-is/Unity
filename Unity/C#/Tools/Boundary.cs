using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public GameObject wallObject;  
    public camaramove movementScript;

    private bool isCollidingWithWall = false; 

    void Update()
    {
        if (Vector3.Distance(transform.position, wallObject.transform.position) < 1f)
        {
            if (!isCollidingWithWall)
            {
                isCollidingWithWall = true;
                if (movementScript != null)
                {
                    movementScript.enabled = false; 
                    Debug.Log("撞到牆了");
                }
            }
        }
        else
        {
            if (isCollidingWithWall)
            {
                isCollidingWithWall = false;
                if (movementScript != null)
                {
                    movementScript.enabled = true; 
                    Debug.Log("離開牆了");
                }
            }
        }
    }
}
