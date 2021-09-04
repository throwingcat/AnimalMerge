using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Violet;

public class PopupHeroUnlock : SUIPanel
{
    public GameObject Close;

    protected override void OnShow()
    {
        base.OnShow();

        Close.SetActive(false);
        IgnoreBackPress = true;
        GameManager.DelayInvoke(() =>
        {
            IgnoreBackPress = false;
            Close.SetActive(true);
        },2f);
    }
}
