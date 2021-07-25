using System.Collections;
using System.Collections.Generic;
using Packet;
using Server;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupChestUnlock : SUIPanel
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

        for (int i = 0; i < Grade.Length; i++)
        {
            Grade[i].SetActive(i < _chest.Grade);
        }
    }

    public void OnClickBackground()
    {
        BackPress();
    }

    public void OnClickUnlock()
    {
        ChestInventory.Instance.Progress(_chest.inDate);
        
        PacketBase packet = new PacketBase();
        packet.PacketType = ePACKET_TYPE.CHEST_PROGRESS;
        packet.hash.Add("inDate", _chest.inDate);
        NetworkManager.Instance.Request(packet, (packet) =>
        {
            BackPress();
        });
            
    }
}