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

    public ulong GUID;
    public Unit Sheet;
    public UnitInventory.Unit Info;

    public eUnitType eUnitType = eUnitType.None;
    public eUnitDropState eUnitDropState = eUnitDropState.Ready;
    private ulong _dropInvokeGUID;

    private Action<UnitBase, UnitBase> _collisionEvent;

    private GameCore ParentCore;

    public virtual UnitBase OnSpawn(string unit_key, Action<UnitBase, UnitBase> collisionEvent, GameCore Core)
    {
        _collisionEvent = collisionEvent;
        ParentCore = Core;

        Sheet = TableManager.Instance.GetData<Unit>(unit_key);
        Info = UnitInventory.Instance.GetUnit(unit_key);
        if (Info == null)
            Info = new UnitInventory.Unit()
            {
                Key = unit_key,
                Exp = 0,
                Level = 1,
            };
        GUID = GameManager.Guid.NewGuid();
        eUnitDropState = eUnitDropState.Ready;

        if (Rigidbody2D != null)
            Rigidbody2D.gravityScale = 0;

        Texture.sprite = GetSprite(unit_key);
        transform.localScale = Vector3.zero;

        if (Core != null && Core.IsPlayer)
        {
            if (Sheet.isBadBlock)
            {
                if (VFXSpawn != null)
                    VFXSpawn.SetActive(true);
            }
            else if (VFXSpawn != null)
                VFXSpawn.SetActive(false);

            if (VFXMerge != null)
                VFXMerge.SetActive(false);
        }
        else
        {
            if (VFXSpawn != null)
                VFXSpawn.SetActive(false);
            if (VFXMerge != null)
                VFXMerge.SetActive(false);
        }

        var size = Vector3.one * (float) (EnvironmentValue.UNIT_BASE_SIZE * Sheet.size);
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

    public void Drop(bool isAddForce = false)
    {
        Utils.SetLayer("Unit", gameObject);

        eUnitDropState = eUnitDropState.Falling;
        if (Rigidbody2D != null)
            Rigidbody2D.gravityScale = 2;

        if (Collider2D != null)
            Collider2D.enabled = true;

        SetActivePhysics(true);

        if (isAddForce)
            AddTorque(0.3f);

        _dropInvokeGUID = GameManager.DelayInvoke(() =>
        {
            if (eUnitDropState == eUnitDropState.Falling)
                eUnitDropState = eUnitDropState.Complete;
        }, 2f);
    }

    public void AddForce(Vector2 power)
    {
        Rigidbody2D.AddRelativeForce(power);
    }

    public void AddTorque(float power)
    {
        Rigidbody2D.AddTorque(Random.Range(-power, power), ForceMode2D.Force);
    }

    protected static Sprite GetSprite(string unit_key)
    {
        var sheet = TableManager.Instance.GetData<Unit>(unit_key);
        string atlas = sheet.Master != null ? sheet.Master.atlas : "Common";
        return sheet.face_texture.ToSprite(atlas);
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
        if (ParentCore != null && ParentCore.IsPlayer)
        {
            if (VFXMerge != null)
                VFXMerge.SetActive(true);
        }
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