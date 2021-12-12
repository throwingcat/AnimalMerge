using System;
using SheetData;
using UnityEngine.UI;
using Violet;

public class PopupUnitUpgrade : SUIPanel
{
    public Text After;
    public Text Before;
    public PartItemCard Unit;

    protected override void OnShow()
    {
        base.OnShow();
    }

    public void Set(UnitInventory.Unit unit)
    {
        Unit.Set(unit);
        var sheet = unit.Key.ToTableData<Unit>();

        string level = string.Format("level_format".ToLocalization(), unit.Level);
        string name = sheet.name.ToLocalization();
        Unit.SetName(string.Format("{0} {1}", level, name));
        
        var before = Utils.GetUnitDamage(sheet.score, unit.Level - 1);
        var after = Utils.GetUnitDamage(sheet.score, unit.Level);

        before = Math.Truncate(before * 10) / 10;
        after = Math.Truncate(after * 10) / 10;
        Before.text = string.Format("atk_format".ToLocalization(), before);
        After.text = string.Format("atk_format_color".ToLocalization(), after);
    }
}