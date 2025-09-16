using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class keyboard : MonoBehaviour
{
    public TMP_Text inputField;       // 顯示打碼的文字
    private string realPassword = ""; // 真正輸入的密碼

    // === 數字鍵 ===
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

    // === 符號鍵 ===
    public void word_symbol()
    {
        AddCharacter("@");
    }

    // === 刪除鍵 ===
    public void RemoveLastWord()
    {
        if (realPassword.Length > 0)
        {
            realPassword = realPassword.Substring(0, realPassword.Length - 1);
            inputField.text = new string('*', realPassword.Length);
        }
    }

    // === 新增字元並打碼 ===
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

    // === 需要用到真正密碼時呼叫 ===
    public string GetPassword()
    {
        return realPassword;
    }
}
