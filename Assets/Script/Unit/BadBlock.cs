using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;

public class BadBlock : UnitBase
{
    public override UnitBase OnSpawn(string unit_key)
    {
        var unit = base.OnSpawn(unit_key);
        unit.eUnitType = eUnitType.Bad;
        return unit;
    }
}
