using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class AutoUIElementFinder : MonoBehaviour
{
    public VRUIButtonSelector buttonSelector; // �s�� VRUIButtonSelector

    void Start()
    {
        if (buttonSelector == null)
        {
            buttonSelector = GetComponent<VRUIButtonSelector>();
        }

        if (buttonSelector != null)
        {
            FindUIElements();
        }
        else
        {
            UnityEngine.Debug.LogError("VRUIButtonSelector �����[�쪫��W�I");
        }
    }

    void FindUIElements()
    {
        // ���Ҧ� Selectable UI�]Button, TMP_InputField, InputField�^
        Selectable[] allUI = FindObjectsOfType<Selectable>();

        // �M�Ų{�����C��
        buttonSelector.uiElements.Clear();

        foreach (Selectable ui in allUI)
        {
            if (ui is Button || ui is TMP_InputField || ui is InputField)
            {
                buttonSelector.uiElements.Add(ui);
            }
        }

        UnityEngine.Debug.Log($"�o�{ {buttonSelector.uiElements.Count} �� UI ����å[�J�ܦC��C");

        // �w�]����Ĥ@��
        if (buttonSelector.uiElements.Count > 0)
        {
            buttonSelector.HighlightElement(0);
        }
    }
}
