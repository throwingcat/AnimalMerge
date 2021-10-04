using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Violet;

public class LoadingCurtain : MonoSingleton<LoadingCurtain>
{
    private int _stack = 0;
    public void SetActive(bool isActive)
    {
        if (isActive)
            _stack++;
        else
            _stack--;
        
        gameObject.SetActive(0 < _stack);
    }
}
