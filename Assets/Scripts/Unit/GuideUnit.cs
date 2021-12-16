using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;

public class GuideUnit : MonoBehaviour
{
    public SpriteRenderer Face;
    public SpriteRenderer GuideLine;

    public void Set(string unit_key)
    {
        var unit = unit_key.ToTableData<Unit>();
        Face.sprite = unit.face_texture.ToSprite(unit.Master.atlas);
    }


}
