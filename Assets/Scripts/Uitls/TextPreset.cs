using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Coffee.UIEffects.UIShadow))]
public class TextPreset : UnityEngine.UI.Text
{
    #if UNITY_EDITOR
    void Reset()
    {
        base.Reset();
        font = Resources.Load<Font>("Font/Maplestory Bold");
        Coffee.UIEffects.UIShadow shadow = GetComponent<Coffee.UIEffects.UIShadow>();
        shadow.style = Coffee.UIEffects.ShadowStyle.Outline8;
        shadow.effectDistance = new Vector2(2,2);
        transform.localScale = Vector3.one * 0.5f;
    }
    #endif
}
