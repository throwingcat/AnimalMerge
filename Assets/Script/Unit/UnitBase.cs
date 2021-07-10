using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Define;
using SheetData;
using UnityEngine;
using Violet;
using DG.Tweening;

public class UnitBase : MonoBehaviour
{
    public SpriteRenderer Texture;
    public Rigidbody2D Rigidbody2D;
    public Collider2D Collider2D;

    public GameObject VFXSpawn;
    public GameObject VFXMerge;

    public string GUID;
    public Unit Sheet;

    public eUnitType eUnitType = eUnitType.None;
    
    public virtual UnitBase OnSpawn(string unit_key)
    {
        Sheet = TableManager.Instance.GetData<Unit>(unit_key);
        GUID = System.Guid.NewGuid().ToString();

        if (Rigidbody2D != null)
            Rigidbody2D.gravityScale = 0;
        if (Collider2D != null)
            Collider2D.enabled = false;

        Texture.sprite = GetSprite(unit_key);
        transform.localScale = Vector3.zero;

        if (VFXSpawn != null)
            VFXSpawn.SetActive(true);

        if (VFXMerge != null)
            VFXMerge.SetActive(false);

        var size = Vector3.one * Sheet.size;
        transform.DOScale(size, 0.5f).SetEase(Ease.OutBack).Play();

        eUnitType = eUnitType.Nomral;
        return this;
    }

    public UnitBase SetPosition(Vector3 position)
    {
        transform.localPosition = position;
        return this;
    }

    public UnitBase SetRotation(Vector3 euler)
    {
        transform.localRotation = Quaternion.Euler(euler);
        return this;
    }

    public void Drop()
    {
        if (Rigidbody2D != null)
            Rigidbody2D.gravityScale = 1;

        if (Collider2D != null)
            Collider2D.enabled = true;

        SetActivePhysics(true);
        
        Rigidbody2D.AddTorque(Random.Range(-0.25f, 0.25f));
    }

    protected static Sprite GetSprite(string unit_key)
    {
        var sheet = TableManager.Instance.GetData<Unit>(unit_key);
        string path = string.Format("AnimalMerge/{0}/{1}", sheet.group, sheet.face_texture);
        return Resources.Load<Sprite>(path);
    }

    public void SetActivePhysics(bool isActive)
    {
        if (Rigidbody2D != null)
            Rigidbody2D.constraints =
                isActive ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeAll;
    }

    public void PlayMerge()
    {
        SetActivePhysics(false);
        if (VFXMerge != null)
            VFXMerge.SetActive(true);
    }
}