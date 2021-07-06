using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class IngameBadBlock : MonoBehaviour
{
    public Image Image;

    public void Set(Unit unit)
    {
        Image.sprite = unit.face_texture.ToSprite();
        Image.SetNativeSize();
    }
}
