using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Define;
using UnityEngine;

public class UnitInventory
{
    private static UnitInventory _instance;

    public static UnitInventory Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UnitInventory();
            return _instance;
        }
    }

    public Dictionary<string,List<Unit>> Units = new Dictionary<string, List<Unit>>();
    public Dictionary<string,bool> ChangedGroup = new Dictionary<string, bool>();
    
    public void Insert(SheetData.Unit sheet)
    {
        if (Units.ContainsKey(sheet.group) == false)
        {
            Units.Add(sheet.group,new List<Unit>());
        }

        bool isContains = false;
        foreach (var unit in Units[sheet.group])
        {
            if (unit.Key == sheet.key)
            {
                isContains = true;
                break;
            }
        }

        if (isContains == false)
        {
            Units[sheet.group].Add(new Unit()
            {
                Key = sheet.key,
                Level = 1,
                Exp = 0,
            });

            SetChangedGroup(sheet.group);
        }
    }

    private void SetChangedGroup(string group)
    {
        if (ChangedGroup.ContainsKey(group) == false)
            ChangedGroup[group] = false;
        ChangedGroup[group] = true;
    }

    public void GainEXP(string key,int value)
    {
        var sheet = key.ToTableData<SheetData.Unit>();

        if (Units.ContainsKey(sheet.group))
        {
            foreach (var unit in Units[sheet.group])
            {
                if (unit.Key == key)
                {
                    unit.Exp += value;
                    SetChangedGroup(sheet.group);
                }
            }
        }
    }

    public Unit Get(string key)
    {
        var sheet = key.ToTableData<SheetData.Unit>();

        if (Units.ContainsKey(sheet.group))
        {
            foreach (var unit in Units[sheet.group])
            {
                if (unit.Key == key)
                    return unit;
            }
        }

        return null;
    }
    public class Unit
    {
        public string Key;
        public int Level;
        public int Exp;
    }
}