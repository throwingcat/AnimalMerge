using Common;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PopupChestUnlock : SUIPanel
{
    private ChestInventory.ChestSlot _chestSlot;
    public Text CardQuantity;
    public Text GoldRange;
    public GameObject[] Grade;
    public Text NeedTime;

    protected override void OnShow()
    {
        base.OnShow();
    }

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

        for (var i = 0; i < Grade.Length; i++) Grade[i].SetActive(i < _chestSlot.Grade);
    }

    public void OnClickBackground()
    {
        BackPress();
    }

    public void OnClickUnlock()
    {
        ChestInventory.Instance.Progress(_chestSlot.inDate);

        var packet = new PacketBase();
        packet.PacketType = ePacketType.CHEST_PROGRESS;
        packet.hash.Add("inDate", _chestSlot.inDate);
        NetworkManager.Instance.Request(packet, packet => { BackPress(); });
    }
}