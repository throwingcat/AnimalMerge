using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOffsetFlow : MonoBehaviour
{
    public Vector2 Power;
    public Material Material;
    private static readonly int Offset = Shader.PropertyToID("_offset");

    public void OnEnable()
    {
        var mg = GetComponent<MaskableGraphic>();
        if (mg != null)
            Material = mg.material;
    }

    public void Update()
    {
        if (Material != null)
        {
            Vector2 offset = Material.mainTextureOffset;
            offset.x = (offset.x + Power.x * Time.deltaTime) % 1;
            offset.y = (offset.y + Power.y * Time.deltaTime) % 1;
            Material.mainTextureOffset = offset;
        }
    }
}