using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Define;
using SheetData;
using UnityEngine;
using Violet;
using DG.Tweening;
using Random = UnityEngine.Random;

public class UnitBase : MonoBehaviour
{
    public SpriteRenderer Texture;
    public Rigidbody2D Rigidbody2D;
    public Collider2D Collider2D;

    public GameObject VFXSpawn;
    public GameObject VFXMerge;

    public string GUID;
    public Unit Sheet;
    public UnitInventory.Unit Info;

    public eUnitType eUnitType = eUnitType.None;
    public eUnitDropState eUnitDropState = eUnitDropState.Ready;
    private string _dropInvokeGUID = "";

    private Action<UnitBase, UnitBase> _collisionEvent;

    public virtual UnitBase OnSpawn(string unit_key, Action<UnitBase, UnitBase> collisionEvent)
    {
        _collisionEvent = collisionEvent;

        Sheet = TableManager.Instance.GetData<Unit>(unit_key);
        Info = UnitInventory.Instance.GetUnit(unit_key);
        if (Info == null)
            Info = new UnitInventory.Unit()
            {
                Key = unit_key,
                Exp = 0,
                Level = 1,
            };
        GUID = System.Guid.NewGuid().ToString();
        eUnitDropState = eUnitDropState.Ready;

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
        eUnitDropState = eUnitDropState.Falling;
        if (Rigidbody2D != null)
            Rigidbody2D.gravityScale = 1;

        if (Collider2D != null)
            Collider2D.enabled = true;

        SetActivePhysics(true);

        AddTorque(0.03f);

        _dropInvokeGUID = GameManager.DelayInvoke(() =>
        {
            if (eUnitDropState == eUnitDropState.Falling)
                eUnitDropState = eUnitDropState.Complete;
        }, 2f);
    }

    public void AddTorque(float power)
    {
        Rigidbody2D.AddTorque(Random.Range(-power, power),ForceMode2D.Impulse);
    }

    protected static Sprite GetSprite(string unit_key)
    {
        var sheet = TableManager.Instance.GetData<Unit>(unit_key);
        return sheet.face_texture.ToSprite();
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

    public void OnCollisionEnter2D(Collision2D other)
    {
        var target = other.gameObject.GetComponent<UnitBase>();
        if (target != null)
        {
            if (eUnitType == eUnitType.Nomral && target.eUnitType == eUnitType.Nomral)
                _collisionEvent?.Invoke(this, target);
        }
    }

    public void OnRemove()
    {
        GameManager.DelayInvokeCancel(_dropInvokeGUID);
    }
}