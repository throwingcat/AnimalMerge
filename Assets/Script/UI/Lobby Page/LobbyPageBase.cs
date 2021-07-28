using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LobbyPageBase : MonoBehaviour
{
    public int Index = 0;
    public CanvasGroup CanvasGroup;
    public GameObject Root;
    
    public virtual void OnShow()
    {
    }

    public virtual void OnUpdate(float delta)
    {
    }

    public virtual void OnHide()
    {
    }
}