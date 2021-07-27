using System;
using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;
using UnityEngine.UI;

public class PartItemCard : MonoBehaviour
{
    public CanvasGroup CardImageGroup;
    public CanvasGroup CardDescriptionGroup;
    public GameObject Root;
    public Image Frame;
    public Image Icon;
    public Text Level;
    public Text Exp;
    public Text Group;
    public Text Name;
    public Text Amount;
    public SlicedFilledImage ExpGauge;


    public void Set(ItemInfo itemInfo)
    {
        switch (itemInfo.Type)
        {
            case eItemType.Currency:
            {
                var sheet = itemInfo.Key.ToTableData<SheetData.Item>();
                SetTexutre(sheet.Texture);
                SetName(sheet.Name.ToLocalization());
                SetGroup(sheet.Type.ToLocalization());
                Exp.gameObject.SetActive(false);
                ExpGauge.gameObject.SetActive(false);
            }
                break;
            case eItemType.Card:
            {
                var sheet = itemInfo.Key.ToTableData<SheetData.Unit>();
                SetTexutre(sheet.face_texture);
                SetName(sheet.name.ToLocalization());
                SetGroup(string.Format("group_{0}",sheet.group.ToLower().ToLocalization()));

                var unit = UnitInventory.Instance.Get(sheet.key);
                SetLevel(unit.Level);
                
                var next_level_sheet = (unit.Level + 1).ToString().ToTableData<SheetData.UnitLevel>();
                if (next_level_sheet != null)
                    SetExp(unit.Exp, next_level_sheet.exp);
                else
                    SetExp(1, 1);
            }
                break;
        }
    }
    public void SetTexutre(string texutre)
    {
        Icon.sprite = texutre.ToSprite();
    }

    public void SetName(string name)
    {
        if (Name != null)
            Name.text = name;
    }

    public void SetGroup(string group)
    {
        if (Group != null)
            Group.text = group;
    }

    public void SetLevel(int level)
    {
        if (Level != null)
        {
            if (Level.gameObject.activeSelf == false)
                Level.gameObject.SetActive(true);
            Level.text = string.Format("{0} {1}", "Level".ToLocalization(), level);
        }
    }

    public void SetExp(int current, int max)
    {
        if (Exp != null)
        {
            if (Exp.gameObject.activeSelf == false)
                Exp.gameObject.SetActive(true);
            Exp.text = string.Format("{0}/{1}", current, max);
        }

        if (ExpGauge != null)
        {
            if (ExpGauge.gameObject.activeSelf == false)
                ExpGauge.gameObject.SetActive(true);
            ExpGauge.fillAmount = current / (float) max;
        }
    }

    public void SetAmount(int amount)
    {
        if (Amount != null)
        {
            if(Amount.gameObject.activeSelf == false)
                Amount.gameObject.SetActive(true);
            Amount.text = string.Format("X{0}", amount);
        }
    }
}