using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using Violet;

public class UnitBase : MonoBehaviour
{
    public SpriteRenderer Texture;
    public Rigidbody2D Rigidbody2D;
    public CircleCollider2D CircleCollider2D;

    public string GUID;
    public Unit Sheet;

    public void OnSpawn(string unit_key, Vector3 position)
    {
        Sheet = TableManager.Instance.GetData<Unit>(unit_key);
        GUID = System.Guid.NewGuid().ToString();

        transform.localPosition = position;
        Rigidbody2D.gravityScale = 0;
        CircleCollider2D.enabled = false;

        Texture.sprite = GetSprite(unit_key);
        transform.localScale = Vector3.one * Sheet.size;
    }

    public void Drop()
    {
        Rigidbody2D.gravityScale = 1;
        CircleCollider2D.enabled = true;
    }

    private static Sprite GetSprite(string unit_key)
    {
        var sheet = TableManager.Instance.GetData<Unit>(unit_key);
        string path = string.Format("Units/{0}/{1}", sheet.group, sheet.face_texture);
        return Resources.Load<Sprite>(path);
    }
}