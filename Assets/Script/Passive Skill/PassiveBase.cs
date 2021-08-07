using System;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBase
{
    public List<UnitBase> BadUnits => Core.BadUnits;
    public PanelIngame PanelIngame => Core.IsPlayer ? Core.PanelIngame : null;
    public Canvas Canvas => Core.Canvas;
    
    public float CoolTime = 20f;
    public float CoolTimeRemain;
    public bool isEnable => CoolTimeRemain <= 0f;
    public GameCore Core;

    public float CoolTimeProgress => CoolTimeRemain / CoolTime;
    public PassiveBase(GameCore core)
    {
        Core = core;
    }

    public virtual void OnUpdate(float delta)
    {
        CoolTimeRemain -= delta;
        if (CoolTimeRemain <= 0f)
            CoolTimeRemain = 0f;
    }
    
    public virtual void Run(Action onComplete)
    {
    }
}
