using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupLogin : SUIPanel
{
    public InputField InputField;
    public GameObject BtnLoginEnable;
    public GameObject BtnLoginDisable;

    public System.Action<string> OnFinish;
    public void OnEndEditText()
    {
        if (InputField.text.IsNullOrEmpty())
        {
            InputField.text = "닉네임을 입력하세요";
            
            BtnLoginEnable.SetActive(false);
            BtnLoginDisable.SetActive(true);
        }
        else
        {
            BtnLoginEnable.SetActive(true);
            BtnLoginDisable.SetActive(false);            
        }
    }

    public void OnClickConnection()
    {
        OnFinish?.Invoke(InputField.text);
    }
}
