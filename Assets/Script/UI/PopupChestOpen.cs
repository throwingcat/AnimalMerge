using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupChestOpen : SUIPanel
{
    public Text GoldRange;
    public Text CardQuantity;
    public Text NeedTime;
    public GameObject[] Grade;
    protected override void OnShow()
    {
        base.OnShow();
    }

    private ChestInventory.Chest _chest;
    public void Set(ChestInventory.Chest chest)
    {
        _chest = chest;
        Refresh();
    }

    public void Refresh()
    {
        GoldRange.text = string.Format("{0}~{1}", _chest.GetGoldMin(), _chest.GetGoldMax());
        CardQuantity.text = _chest.GetCardQuantity().ToString();
        NeedTime.text = Utils.ParseSeconds(_chest.Sheet.time);
    }
}
