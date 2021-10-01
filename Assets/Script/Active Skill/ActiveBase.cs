using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;

public class ActiveBase
{
    public List<UnitBase> BadUnits => Core.BadUnits;
    public List<UnitBase> UnitsInField => Core.UnitsInField;


    public PanelIngame PanelIngame => Core.IsPlayer ? Core.PanelIngame : null;
    public Canvas Canvas => Core.Canvas;
    public GameCore Core;
    public int Point;
    public float Progress => Point / (float) EnvironmentValue.SKILL_CHARGE_MAX_VALUE;

    public bool isEnable => EnvironmentValue.SKILL_CHARGE_MAX_VALUE <= Point;

    public ActiveBase(GameCore core)
    {
        Core = core;
    }

    public virtual void Charge(int value)
    {
        Point += value;
    }

    public void Run()
    {
        if (Point < EnvironmentValue.SKILL_CHARGE_MAX_VALUE) return;
        if (RunProcess())
        {
            PlayerBattleTracker.Update(PlayerTracker.USE_SKILL, 1);
            Point = 0;
        }
    }

    protected virtual bool RunProcess()
    {
        return true;
    }
}