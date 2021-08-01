using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using Violet;

public class SyncUnitBase : UnitBase
{
    public override UnitBase OnSpawn(string unit_key,System.Action<UnitBase, UnitBase> collisionEvent)
    {
        Sheet = TableManager.Instance.GetData<Unit>(unit_key);
        GUID = System.Guid.NewGuid().ToString();
        Texture.sprite = GetSprite(unit_key);
        transform.localScale =  Vector3.one * Sheet.size;
        return this;
    }
}
