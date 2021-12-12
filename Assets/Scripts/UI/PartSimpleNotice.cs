using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PartSimpleNotice : MonoSingleton<PartSimpleNotice>
{
    public DOTweenPlayer TweenPlayer;
    public Text Message;
    private ulong _fadeID;
    private ulong _completeID;
    public static void Show(string text)
    {
        GameManager.DelayInvokeCancel(Instance._fadeID);
        GameManager.DelayInvokeCancel(Instance._completeID);
        Instance.gameObject.SetActive(false);
        Instance.Message.text = text;
        Instance.gameObject.SetActive(true);
        Instance.TweenPlayer.SetEnable(true);
        
        Instance._fadeID = GameManager.DelayInvoke(() =>
        {
            Instance.TweenPlayer.PlayBackward();
        }, 1.75f);
        Instance._completeID =GameManager.DelayInvoke(() =>
        {
            Instance.gameObject.SetActive(false);
        }, 2f);
        
        
    }
}
