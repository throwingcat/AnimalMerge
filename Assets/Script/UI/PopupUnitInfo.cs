using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using Violet;

public class PopupUnitInfo : SUIPanel
{
    public PartItemCard Unit;
    public Text Name;
    public Text Group;
    public Text Description;
    public Text Damage;
    public Text UpgradeDamage;
    public Text Price;
    public GameObject UpgradeButton;
    public GameObject UpgradeComplete;

    public void Set(UnitInventory.Unit unit)
    {
        Unit.Set(unit);
        var sheet = unit.Key.ToTableData<Unit>();
        var group = sheet.group.ToTableData<UnitGroup>();
        Name.text = sheet.name.ToLocalization();
        Description.text = string.Format("{0} 의 배경설명", sheet.name.ToLocalization()); 
        Group.text = group.name.ToLocalization();

        float current = Utils.GetUnitDamage(sheet.score, unit.Level);
        Damage.text = current.ToString("N1");

        if (unit.IsMaxLevel())
        {
            UpgradeComplete.SetActive(true);
            UpgradeDamage.gameObject.SetActive(false);
            UpgradeButton.SetActive(false);
        }
        else
        {
            UpgradeComplete.SetActive(false);
            UpgradeDamage.gameObject.SetActive(true);
            UpgradeButton.SetActive(true);
            
            float next = Utils.GetUnitDamage(sheet.score, unit.Level + 1);
            UpgradeDamage.text = string.Format("+{0:N1}", (next - current)); 
            Price.text = "1,000";
        }
    }

    public void OnClickUpgrade()
    {
        
    }

    public void OnClickBackground()
    {
        BackPress();
    }
}
