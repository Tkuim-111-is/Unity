using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActive_office : MonoBehaviour
{
    public List<GameObject> computers = new List<GameObject>();

    public GameObject wifi;
    public GameObject document_holder;
    public GameObject usb;
    public AudioSource AudioSource;
    // public GameObject teleporter;

    private int Document_holder;
    private int WIFI;
    private int USB;

    void Awake()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("computer."))
            {
                computers.Add(obj);
            }
        }
    }

    void Start()
    {
        WIFI = PlayerPrefs.GetInt("WIFI", -1);
        Document_holder = PlayerPrefs.GetInt("document_holder", -1);
        USB = PlayerPrefs.GetInt("USB", -1);

        if (WIFI == 1) wifi.tag = "Untagged";
        if (Document_holder == 1) document_holder.tag = "Untagged";
        if (USB == 1) usb.tag = "Untagged";

        if (WIFI == 1 && Document_holder == 1 && USB == 1)
        {
            AudioSource.Play();
            foreach (GameObject computer in computers)
            {
                computer.tag = "Check";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
