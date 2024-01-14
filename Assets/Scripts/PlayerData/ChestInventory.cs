using System;
using Define;
using SheetData;
using UnityEngine.Serialization;

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
        if (ChestSlots == null)
            ChestSlots = new ChestSlot[EnvironmentValue.CHEST_SLOT_MAX_COUNT];
        foreach (var slot in ChestSlots)
        {
            if (slot == null) continue;
            if (slot.key.IsNullOrEmpty())
                return slot;
        }

        return null;
    }

    public void Insert(string key)
    {
        var slot = GetEmptySlot();
        if (slot != null)
        {
            slot.key = key;
            slot.isProgress = false;
            slot.grade = 0;
            slot.startTime = new DateTime();
            slot.getTime = GameManager.GetTime();

            slot.isChanged = true;
        }
    }

    public void Upgrade(string inDate)
    {
        foreach (var chest in ChestSlots)
            if (chest.guid == inDate)
            {
                chest.grade++;
                chest.isChanged = true;
            }
    }

    public void Progress(string guid)
    {
        foreach (var chest in ChestSlots)
        {
            if (chest.guid == guid)
            {
                chest.startTime = GameManager.GetTime();
                chest.isProgress = true;
                chest.isChanged = true;
                break;
            }
        }
    }

    public void Remove(string inDate)
    {
        for (var i = 0; i < ChestSlots.Length; i++)
            if (ChestSlots[i].guid == inDate)
            {
                ChestSlots[i].key = "";
                ChestSlots[i].isChanged = true;
            }
    }

    public bool isContains(string inDate)
    {
        for (var i = 0; i < ChestSlots.Length; i++)
        {
            if (ChestSlots[i] == null) continue;
            if (ChestSlots[i].guid == inDate)
                return true;
        }

        return false;
    }

    public ChestSlot Get(string inDate)
    {
        for (var i = 0; i < ChestSlots.Length; i++)
            if (ChestSlots[i].guid == inDate)
                return ChestSlots[i];

        return null;
    }

    [Serializable]
    public class ChestSlot
    {
        public string guid;
        public bool isChanged;
        public int index;
        public string key;
        public bool isProgress;
        public int grade;
        public DateTime getTime;
        public DateTime startTime;

        public Chest Sheet => key.ToTableData<Chest>();

        public int GetGoldMin()
        {
            return (int)(Sheet.gold_min + Sheet.gold_min * 0.5f * grade);
        }

        public int GetGoldMax()
        {
            return (int)(Sheet.gold_max + Sheet.gold_max * 0.5f * grade);
        }

        public int GetRewardAmount()
        {
            return (int)(Sheet.amount + Sheet.amount * 0.5f * grade);
        }
    }
}