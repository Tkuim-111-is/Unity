using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Background_Information : MonoBehaviour
{
    public Transform player;
    public OVRRayHelper rayHelper;
    public LayerMask objectLayer;
    public TextMeshProUGUI infoText;

    private int itemId;
    private int wrong;
    void Start()
    {
        itemId = PlayerPrefs.GetInt("ItemID", -1);
        wrong = PlayerPrefs.GetInt("Wrong");
        if (itemId == 5) // ����7
        {
            infoText.text = "�A�{�b�b�줽�Ǩ������u�̪��u�@���p�A�����G���ǤH���w���Ӽ��x\n�Ъȥ��L�̪����~�I";
        }
        else if (itemId == 6) // ����4
        {
            infoText.text = "�A�b�@���U�y�����ܩ@��\n�P�Ƭ�M�ǪF��n�A�U��";
        }
        else if (itemId == 7) // ����6
        {
            if (wrong <= 0)
            {
                infoText.text = "�A���P�ƾߨ�F�@�ӨӸ��������H����\n���U�L�æ^���L�����D";
            }
            else
            {
                infoText.text = "�A���P�ƾߨ�F�@�ӨӸ��������H����\n�ä��ݩ�A";
            }
        }
        else if(itemId == 8) // ����2
        {
            infoText.text = "�|ĳ�赲���A�A�ݭn�M�z�|ĳ�ǡA�קK���|���n��T�C";
        }
    }

    void Update()
    {
        GetInScene();
    }

    public void GetInScene()
    {
        Ray ray = new Ray(rayHelper.transform.position, rayHelper.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, objectLayer))
        {
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                SceneManager.LoadScene(itemId);
            }
        }
    }
}
