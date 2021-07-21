using System;
using System.Collections.Generic;
using Define;

public class ChestInventory
{
    private static ChestInventory _instance;

    public List<Chest> Chests = new List<Chest>();

    public static ChestInventory Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ChestInventory();
            return _instance;
        }
    }

    public bool Insert(string key)
    {
        if (Chests.Count < EnvironmentValue.CHEST_SLOT_MAX_COUNT)
        {
            var chest = new Chest();
            chest.Key = key;
            chest.StartTime = GameManager.GetTime();
            chest.isChanged = true;

            Chests.Add(chest);
            return true;
        }
        return false;
    }

    public void Remove(string indate)
    {
        for (var i = 0; i < Chests.Count; i++)
            if (Chests[i].inDate == indate)
            {
                Chests[i].inDate = "";
                Chests[i].Key = "";
                Chests[i].isChanged = true;
            }
    }

    public bool isContains(string inDate)
    {
        for (int i = 0; i < Chests.Count; i++)
        {
            if (Chests[i].inDate == inDate)
                return true;
        }

        return false;
    }

    public class Chest
    {
        public string inDate;
        public bool isChanged;
        public string Key;
        public bool isProgress = false;
        public DateTime StartTime;
    }
}