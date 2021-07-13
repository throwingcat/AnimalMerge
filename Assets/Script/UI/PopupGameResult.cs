using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Violet;

public class PopupGameResult : SUIPanel
{
    public GameObject Victory;
    public GameObject Defeat;
    public void SetResult(bool isWin)
    {
        Victory.SetActive(isWin);
        Defeat.SetActive(!isWin);
    }
}
