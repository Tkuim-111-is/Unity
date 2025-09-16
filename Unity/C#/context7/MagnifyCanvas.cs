using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagnifyCanvas : MonoBehaviour
{
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public Sprite NPC1_picture;
    public Sprite NPC3_picture;
    public Sprite NPC4_picture;
    public Sprite NPC5_picture;
    public GameObject Player_canvas;
    public Image Player_picture;

    void Start()
    {
        Player_canvas.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
            {
                if (Player_canvas.activeSelf)
                {
                    Player_canvas.SetActive(false);
                }
                else
                {
                    if (hit.collider.CompareTag("Canvas"))
                    {
                        Player_canvas.SetActive(true);
                        if (hit.collider.name == "NPC1_video")
                        {
                            Player_picture.sprite = NPC1_picture;
                        }
                        else if (hit.collider.name == "NPC3_video")
                        {
                            Player_picture.sprite = NPC3_picture;
                        }
                        else if (hit.collider.name == "NPC4_video")
                        {
                            Player_picture.sprite = NPC4_picture;
                        }
                        else if (hit.collider.name == "NPC5_video")
                        {
                            Player_picture.sprite = NPC5_picture;
                        }
                    }
                }
            }
        }
    }
}
