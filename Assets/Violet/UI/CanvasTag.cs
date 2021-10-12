using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasTag : MonoBehaviour
{
    public enum eCanvasTag
    {
        Main,
        Background,
        Dynamic,
        None,
    }

    public eCanvasTag Tag = eCanvasTag.None;
    public Canvas Canvas;

    void Reset()
    {
        Canvas = transform.GetComponent<Canvas>();
    }
}
