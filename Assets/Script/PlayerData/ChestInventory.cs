using System;
using System.Collections.Generic;
using System.Xml.Schema;
using Define;

public class ChestInventory
{
    private static ChestInventory _instance;

    public ChestSlot[] ChestSlots;

    public static ChestInventory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ChestInventory();
                _instance.ChestSlots = new ChestSlot[EnvironmentValue.CHEST_SLOT_MAX_COUNT];
            }

            return _instance;
        }
    }

    public ChestSlot GetEmptySlot()
    {
        foreach (var slot in ChestSlots)
        {
            if (slot.Key.IsNullOrEmpty())
                return slot;
        }

        return null;
    }

    public void Insert(string key)
    {
        var slot = GetEmptySlot();
        if (slot != null)
        {
            slot.Key = key;
            slot.isProgress = false;
            slot.Grade = 0;
            slot.StartTime = new DateTime();
            slot.GetTime = GameManager.GetTime();
            
            slot.isChanged = true;
        }
    }

    public void Upgrade(string inDate)
    {
        foreach (var chest in ChestSlots)
        {
            if (chest.inDate == inDate)
            {
                chest.Grade++;
                chest.isChanged = true;
            }
        }
    }

    public void Progress(string inDate)
    {
        foreach (var chest in ChestSlots)
        {
            if (chest.inDate == inDate)
            {
                chest.StartTime = GameManager.GetTime();
                chest.isProgress = true;
                chest.isChanged = true;
                break;
            }
        }
    }

    public void Remove(string inDate)
    {
        for (var i = 0; i < ChestSlots.Length; i++)
            if (ChestSlots[i].inDate == inDate)
            {
                ChestSlots[i].Key = "";
                ChestSlots[i].isChanged = true;
            }
    }

    public bool isContains(string inDate)
    {
        for (int i = 0; i < ChestSlots.Length; i++)
        {
            if (ChestSlots[i] == null) continue;
            if (ChestSlots[i].inDate == inDate)
                return true;
        }

        return false;
    }

    public ChestSlot Get(string inDate)
    {
        for (int i = 0; i < ChestSlots.Length; i++)
        {
            if (ChestSlots[i].inDate == inDate)
                return ChestSlots[i];
        }

        return null;
    }

    public class ChestSlot
    {
        public string inDate;
        public bool isChanged;
        public string Key;
        public bool isProgress = false;
        public int Grade = 0;
        public DateTime StartTime;
        public DateTime GetTime;

        public SheetData.Chest Sheet => Key.ToTableData<SheetData.Chest>();

        public int GetGoldMin()
        {
            return (int) (Sheet.gold_min * ((Grade + 1.5f) + 1f));
        }

        public int GetGoldMax()
        {
            return (int) (Sheet.gold_max * ((Grade + 1.5f) + 1f));
        }

        public int GetCardQuantity()
        {
            return (int) (Sheet.amount * ((Grade + 1.5f) + 1f));
        }
    }
}