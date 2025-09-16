using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeContext : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public string targetTag = "Check";

    void Update()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (hit.collider.CompareTag(targetTag) && OVRInput.GetDown(OVRInput.Button.One))
            {
                if (hit.collider.name == "document_holder.002")
                {
                    PlayerPrefs.SetInt("ItemID", 6);
                    PlayerPrefs.SetInt("document_holder", 1);
                }
                else if (hit.collider.name == "wifi.001")
                {
                    PlayerPrefs.SetInt("ItemID", 7);
                    PlayerPrefs.SetInt("WIFI", 1);
                }
                else if (hit.collider.name == "USB")
                {
                    PlayerPrefs.SetInt("ItemID", 8);
                    PlayerPrefs.SetInt("USB", 1);
                }
                else if (hit.collider.name.Contains("computer"))
                {
                    PlayerPrefs.SetInt("ItemID", 9);
                }
                    PlayerPrefs.Save();
                SceneManager.LoadScene(5);
            }
        }
    }
}