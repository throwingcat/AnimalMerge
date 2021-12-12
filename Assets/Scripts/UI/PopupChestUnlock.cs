using System.Collections;
using System.Collections.Generic;
using Common;
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

    private ChestInventory.ChestSlot _chestSlot;
    public void Set(ChestInventory.ChestSlot chestSlot)
    {
        _chestSlot = chestSlot;
        Refresh();
    }

    public void Refresh()
    {
        GoldRange.text = string.Format("{0}~{1}", _chestSlot.GetGoldMin(), _chestSlot.GetGoldMax());
        CardQuantity.text = _chestSlot.GetRewardAmount().ToString();
        NeedTime.text = Utils.ParseSeconds(_chestSlot.Sheet.time);

        for (int i = 0; i < Grade.Length; i++)
        {
            Grade[i].SetActive(i < _chestSlot.Grade);
        }
    }

    public void OnClickBackground()
    {
        BackPress();
    }

    public void OnClickUnlock()
    {
        ChestInventory.Instance.Progress(_chestSlot.inDate);
        
        PacketBase packet = new PacketBase();
        packet.PacketType = ePACKET_TYPE.CHEST_PROGRESS;
        packet.hash.Add("inDate", _chestSlot.inDate);
        NetworkManager.Instance.Request(packet, (packet) =>
        {
            BackPress();
        });
            
    }
}
