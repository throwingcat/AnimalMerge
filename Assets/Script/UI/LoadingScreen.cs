using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Violet;

public class LoadingScreen : MonoBehaviour
{
    public CanvasGroup CanvasGroup;

    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            CanvasGroup.alpha = 0f;
            gameObject.SetActive(true);

            CanvasGroup.DOFade(1f, 1f).Play();
        }

        if (isActive == false)
        {
            CanvasGroup.DOFade(0f, 1f).Play().OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}