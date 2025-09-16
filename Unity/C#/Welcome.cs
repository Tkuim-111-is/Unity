using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Welcome : MonoBehaviour
{
    public TextMeshProUGUI welcomeText;
    void Start()
    {
        string username = PlayerPrefs.GetString("Username", "Guest");
        welcomeText.text = "Åwªïµn¤J¡A" + username;
    }
}
