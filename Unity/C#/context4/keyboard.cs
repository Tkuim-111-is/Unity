using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class keyboard : MonoBehaviour
{
    public TMP_Text inputField;       // ��ܥ��X����r
    private string realPassword = ""; // �u����J���K�X

    // === �Ʀr�� ===
    public void word_one()
    {
        AddCharacter("1");
    }

    public void word_two()
    {
        AddCharacter("2");
    }

    public void word_three()
    {
        AddCharacter("3");
    }

    public void word_four()
    {
        AddCharacter("4");
    }

    public void word_five()
    {
        AddCharacter("5");
    }

    public void word_six()
    {
        AddCharacter("6");
    }

    public void word_seven()
    {
        AddCharacter("7");
    }

    public void word_eight()
    {
        AddCharacter("8");
    }

    public void word_nine()
    {
        AddCharacter("9");
    }

    public void word_zero()
    {
        AddCharacter("0");
    }

    // === �Ÿ��� ===
    public void word_symbol()
    {
        AddCharacter("@");
    }

    // === �R���� ===
    public void RemoveLastWord()
    {
        if (realPassword.Length > 0)
        {
            realPassword = realPassword.Substring(0, realPassword.Length - 1);
            inputField.text = new string('*', realPassword.Length);
        }
    }

    // === �s�W�r���å��X ===
    private void AddCharacter(string c)
    {
        realPassword += c;
        inputField.text = new string('*', realPassword.Length-1) + c;
        StartCoroutine(HideLastCharAfterDelay());
    }
    private IEnumerator HideLastCharAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        inputField.text = new string('*', realPassword.Length);
    }

    // === �ݭn�Ψ�u���K�X�ɩI�s ===
    public string GetPassword()
    {
        return realPassword;
    }
}
