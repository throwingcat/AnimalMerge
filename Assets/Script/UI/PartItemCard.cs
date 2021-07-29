using System;
using Define;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class PartItemCard : MonoBehaviour
{
    public GameObject Root;
    
    public Text Amount;
    public CanvasGroup CardDescriptionGroup;
    public CanvasGroup CardImageGroup;
    public Text Exp;
    public SlicedFilledImage ExpGauge;
    public Image Frame;
    public Text Grade;
    public Text Group;
    public Image Icon;
    public Text Level;
    public Text Name;
    public Image Piece;
    public GameObject LevelUp;

    public UnitInventory.Unit Unit; 
    
    public void Set(ItemInfo itemInfo)
    {
        switch (itemInfo.Type)
        {
            case eItemType.Currency:
            {
                var sheet = itemInfo.Key.ToTableData<Item>();
                SetTexutre(sheet.Texture);
                SetName(sheet.Name.ToLocalization());
                SetGroup(sheet.Type.ToLocalization());
                Exp.gameObject.SetActive(false);
                ExpGauge.gameObject.SetActive(false);
            }
                break;
            case eItemType.Card:
            {
                var unit = UnitInventory.Instance.GetUnit(itemInfo.Key);
                Set(unit);
            }
                break;
        }
    }

    public void Set(UnitInventory.Unit unit)
    {
        Unit = unit;
        
        var sheet = unit.Key.ToTableData<Unit>();
        SetTexutre(sheet.face_texture);
        SetName(sheet.name.ToLocalization());
        SetGroup(string.Format("group_{0}", sheet.group.ToLower().ToLocalization()));
        SetLevel(unit.Level);
        SetGrade(sheet.index);

        var next_level_sheet = (unit.Level + 1).ToString().ToTableData<UnitLevel>();
        if (next_level_sheet != null)
            SetExp(unit.Exp, next_level_sheet.exp);
        else
            SetMaxLevel();

        SetPiece(sheet.piece_texture);
    }

    public void SetTexutre(string texutre)
    {
        if (Icon != null)
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

        if (max <= current)
            SetActiveLevelUp(true);
        else
            SetActiveLevelUp(false);
    }

    public void SetPiece(string piece)
    {
        if (Piece != null) Piece.sprite = piece.ToSprite();
    }

    public void SetAmount(int amount)
    {
        if (Amount != null)
        {
            if (Amount.gameObject.activeSelf == false)
                Amount.gameObject.SetActive(true);
            Amount.text = string.Format("X{0}", amount);
        }
    }

    public void SetGrade(int grade)
    {
        if (Grade != null)
        {
            if (Grade.gameObject.activeSelf == false)
                Grade.gameObject.SetActive(true);
            Grade.text = grade.ToString();
        }
    }

    public void SetActiveLevelUp(bool isActive)
    {
        if (LevelUp != null)
            LevelUp.SetActive(isActive);
    }

    public void SetMaxLevel()
    {
        if (Exp != null)
        {
            if (Exp.gameObject.activeSelf == false)
                Exp.gameObject.SetActive(true);
            Exp.text = "MAX";
        }

        if (ExpGauge != null)
        {
            if (ExpGauge.gameObject.activeSelf == false)
                ExpGauge.gameObject.SetActive(true);
            ExpGauge.fillAmount = 1f;
        }

        SetActiveLevelUp(false);
    }

    
    private Action<UnitInventory.Unit> _onClickUnit;

    public void SetClickEvent(Action<UnitInventory.Unit> onClick)
    {
        _onClickUnit = onClick;
    }

    public void OnPress()
    {
        Root.transform.ButtonPressPlay();
    }

    public void OnRelease()
    {
        Root.transform.ButtonReleasePlay();
    }
    public void OnClick()
    {
        _onClickUnit?.Invoke(Unit);
    }
}