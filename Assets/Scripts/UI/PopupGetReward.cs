using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupGetReward : SUIPanel
{
    public PartItemCard PartItemPrefab;
    public List<PartItemCard> cells = new List<PartItemCard>();
    public Text Message;
    public GridLayoutGroup GridLayoutGroup;
    public void Set(ItemInfo item)
    {
        PartItemPrefab.Set(item);
        
        var sheet = item.Key.ToTableData<Item>();
        Message.gameObject.SetActive(true);
        Message.text = string.Format("{0} x {1}", sheet.Name.ToLocalization(), item.Amount);
        GridLayoutGroup.padding.top = 50;
    }
    public void Set(List<ItemInfo> items)
    {
        if (items.Count == 1)
            Set(items[0]);
        else
        {
            GridLayoutGroup.padding.top = 0;
            foreach(var cell in cells)
                cell.gameObject.SetActive(false);
            
            Message.gameObject.SetActive(false);

            int need = items.Count - cells.Count;
            for (int i = 0; i < need; i++)
            {
                var cell = Instantiate(PartItemPrefab, PartItemPrefab.transform.parent);
                cell.transform.SetAsLastSibling();
                cells.Add(cell);
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                
                cells[i].Set(item);
                cells[i].gameObject.SetActive(true);
            }
        }
    }
}
