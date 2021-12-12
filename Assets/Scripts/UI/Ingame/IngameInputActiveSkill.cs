using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameInputActiveSkill : MonoBehaviour
{
    public RectTransform RectTransform;
    public RectTransform ProgressImage;

    public SlicedFilledImage Progress;
    private Vector2 _touchBegin;
    public bool isActive = false;
    public float ActiveProgress = 0f;
    
    public void On(Vector2 touchBegin,Vector2 current)
    {
        float threshold = Screen.height * 0.05f;
        if (isActive == false)
        {
            if (threshold < (current.y - touchBegin.y))
            {
                _touchBegin = touchBegin;
                isActive = true;
                
                //Utils.ScreenToCanvas( UIManager.Instance.GetCanvas(CanvasTag.eCanvasTag.Main),_touchBegin,ref RectTransform);
                gameObject.SetActive(true);   
                ActiveProgress = 0f;
                Progress.fillAmount = 0f;
                ProgressImage.sizeDelta = Vector2.one * MIN_SIZE;
            }
        }
    }

    public void Off()
    {
        gameObject.SetActive(false);
        isActive = false;
        _touchBegin = Vector2.zero;
        ActiveProgress = 0f;
        Progress.fillAmount = 0f;
    }

    private float RANGE = 350f;
    private float MAX_SIZE = 1600;
    private float MIN_SIZE = 400;
    public void Update()
    {
        if (isActive)
        {
            var current = Utils.GetTouchPoint();
            var value = current.y - _touchBegin.y;
            ActiveProgress =  value / RANGE;
            Progress.fillAmount = ActiveProgress;
            ProgressImage.sizeDelta = Vector2.one * Mathf.Lerp(MIN_SIZE, MAX_SIZE, ActiveProgress);
        }
    }
}
