using System.Collections;
using System.Collections.Generic;
using Define;
using SheetData;
using UnityEngine;
using Violet;

public class SyncUnitBase : UnitBase
{
    public override UnitBase OnSpawn(string unit_key, System.Action<UnitBase, UnitBase> collisionEvent, GameCore Core)
    {
        Sheet = TableManager.Instance.GetData<Unit>(unit_key);
        GUID = GameManager.Guid.NewGuid();
        Texture.sprite = GetSprite(unit_key);
        transform.localScale = Vector3.one * (EnvironmentValue.UNIT_SPRITE_BASE_SIZE * Sheet.size);
        return this;
    }
}