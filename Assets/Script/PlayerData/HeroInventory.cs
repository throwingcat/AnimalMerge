using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInventory
{
    private static HeroInventory _instance;

    public static HeroInventory Instance
    {
        get
        {
            if (_instance == null)
                _instance = new HeroInventory();
            return _instance;
        }
    }
    
}