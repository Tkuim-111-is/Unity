using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class login_stuck : MonoBehaviour
{
    public Vector3 fixedPosition = new Vector3(1014f, 2418.118f, 254f);
    void Start()
    {
        transform.position = fixedPosition;
    }
    void Update()
    {
        transform.position = fixedPosition;
    }
}
