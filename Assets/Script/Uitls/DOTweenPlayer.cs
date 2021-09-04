using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DOTweenPlayer : MonoBehaviour
{
    public DOTweenAnimation[] Tweens = null;

    #if UNITY_EDITOR
    
    #endif
    void Reset()
    {
        Tweens = GetComponentsInChildren<DOTweenAnimation>();
    }

    private void OnEnable()
    {
        SetEnable(false);
    }

    public void SetEnable(bool isEnable)
    {
        foreach (var tween in Tweens)
        {
            if (isEnable)
            {
                tween.DORewind();
                tween.DORestart();
            }
            else
            {
                tween.DOComplete();
                tween.DORewind();
            }
            tween.enabled = isEnable;
        }
    }

    public void PlayBackward()
    {
        foreach (var tween in Tweens)
        {
            tween.DOComplete();
            tween.DOPlayBackwards();
        }
    }
}
