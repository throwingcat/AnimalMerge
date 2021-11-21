using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPlayer : MonoBehaviour
{
    private float _duration;
    private Action _onFinish;

    private ulong _invokeID = 0;
    public void Play(float duration, Action onFinish)
    {
        gameObject.SetActive(false);

        if(_invokeID != 0)
            GameManager.DelayInvokeCancel(_invokeID);
        
        gameObject.SetActive(true);
        
        _invokeID = GameManager.DelayInvoke(() =>
        {
            gameObject.SetActive(false);
            onFinish?.Invoke();
        }, duration);
    }
}
