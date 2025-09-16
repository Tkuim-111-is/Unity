using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRmenu : MonoBehaviour
{
    public Transform headTransform; 
    public GameObject canvas;   
    //public Button backButton;

    public float canvasDistance = 10f; 
    public Vector2 canvasSize = new Vector2(4000, 3000); 

    private RectTransform canvasRect;
    private bool isCanvasVisible = false;

    void Start()
    {
        canvasRect = canvas.GetComponent<RectTransform>();

        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One)) 
        {
            if (isCanvasVisible)
                HideCanvas();
            else
                ShowCanvas();
        }
    }

    void ShowCanvas()
    {
        if (canvas != null)
        {
            canvas.SetActive(true);

            Vector3 forward = headTransform.forward;
            Vector3 position = headTransform.position + forward * canvasDistance;
            canvas.transform.position = position;

            canvas.transform.rotation = Quaternion.LookRotation(position - headTransform.position);

            canvasRect.sizeDelta = canvasSize;

            isCanvasVisible = true;
        }
    }

    void HideCanvas()
    {
        if (canvas != null)
        {
            canvas.SetActive(false);
            isCanvasVisible = false;
        }

        
    }
}
