using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLightTrigger : MonoBehaviour
{
    public Transform player;
    public UnityEngine.UI.Button officeButton;
    public UnityEngine.UI.Button restButton;
    public UnityEngine.UI.Button meetingButton;
    public GameObject changePlaceCanvas;
    public GameObject portal1;
    public GameObject portal2;
    public GameObject portal3;

    void Start()
    {
        changePlaceCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))  
        {
            changePlaceCanvas.SetActive(true);
            officeButton.interactable = true;
            restButton.interactable = true;
            meetingButton.interactable = true;
        }
    }

    // 當 player 離開 BoxCollider 時觸發
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("player")) 
        {
            changePlaceCanvas.SetActive(false);
            officeButton.interactable = false;
            restButton.interactable = false;
            meetingButton.interactable = false;
        }
    }

    public void ChangeToOffice()
    {
        Vector3 targetPosition = portal1.transform.position;
        Vector3 newPlayerPosition = new Vector3(targetPosition.x, player.position.y, targetPosition.z);
        player.position = newPlayerPosition;
    }

    public void ChangeToRest()
    {
        Vector3 targetPosition = portal2.transform.position;
        Vector3 newPlayerPosition = new Vector3(targetPosition.x, player.position.y, targetPosition.z);
        player.position = newPlayerPosition;
    }

    public void ChangeToMeeting()
    {
        Vector3 targetPosition = portal3.transform.position;
        Vector3 newPlayerPosition = new Vector3(targetPosition.x, player.position.y, targetPosition.z);
        player.position = newPlayerPosition;
    }
}

