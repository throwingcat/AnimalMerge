using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isEqulityResolution = false;
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;
        float scaleHeight = ((float) Screen.width / Screen.height) / ((float) 9 / 16);
        float scaleWidth = 1f / scaleHeight;
        
        if (isEqulityResolution)
        {
            if (scaleWidth < scaleHeight)
                scaleHeight = scaleWidth;
            else
                scaleWidth = scaleHeight;
        }
        
        if (scaleHeight < 1)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;
        }



        camera.rect = rect;
    }
}
