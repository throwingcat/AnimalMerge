using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class SimpleRewardView : MonoBehaviour
{
    public Image Icon;
    public Text Amount;

    public void Set(ItemInfo reward)
    {
        var item = reward.Key.ToTableData<Item>();
        if (Icon != null)
            Icon.sprite = item.Texture.ToSprite();
        if (Amount != null)
            Amount.text = reward.ToString();
    }
}