using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Define;
using SheetData;
using UnityEngine;
using Violet;

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

    public Dictionary<string, List<Unit>> Units = new Dictionary<string, List<Unit>>();
    public Dictionary<string, bool> ChangedGroup = new Dictionary<string, bool>();

    public void Insert(SheetData.Unit sheet)
    {
        if (Units.ContainsKey(sheet.master) == false)
        {
            Units.Add(sheet.master, new List<Unit>());
        }

        bool isContains = false;
        foreach (var unit in Units[sheet.master])
        {
            if (unit.Key == sheet.key)
            {
                isContains = true;
                break;
            }
        }

        if (isContains == false)
        {
            Units[sheet.master].Add(new Unit()
            {
                Key = sheet.key,
                Level = 1,
                Exp = 0,
            });

            SetChangedGroup(sheet.master);
        }
    }

    private void SetChangedGroup(string group)
    {
        if (ChangedGroup.ContainsKey(group) == false)
            ChangedGroup[group] = false;
        ChangedGroup[group] = true;
    }

    public void GainEXP(string key, int value)
    {
        var sheet = key.ToTableData<SheetData.Unit>();

        if (Units.ContainsKey(sheet.master))
        {
            foreach (var unit in Units[sheet.master])
            {
                if (unit.Key == key)
                {
                    unit.Exp += value;
                    SetChangedGroup(sheet.master);
                }
            }
        }
    }

    public Unit GetUnit(string key)
    {
        var sheet = key.ToTableData<SheetData.Unit>();

        if (Units.ContainsKey(sheet.master))
        {
            foreach (var unit in Units[sheet.master])
            {
                if (unit.Key == key)
                    return unit;
            }
        }

        return null;
    }

    public List<Unit> GetGroup(string master)
    {
        if (Units.ContainsKey(master) == false)
        {
            var table = TableManager.Instance.GetTable<SheetData.Unit>();
            foreach (var row in table)
            {
                var unit = row.Value as SheetData.Unit;

                if (unit.master == master)
                {
                    if (Units.ContainsKey(master) == false)
                        Units.Add(master, new List<Unit>());
                    Units[master].Add(new Unit()
                    {
                        Key = unit.key,
                        Level = 1,
                        Exp = 0,
                    });
                }
            }
        }

        return Units[master];
    }

    public bool LevelUp(string key)
    {
        var unit = GetUnit(key);
        if (unit.IsMaxLevel() == false && unit.AvaliableLevelUp())
        {
            unit.Level++;
            var sheet = key.ToTableData<SheetData.Unit>();
            SetChangedGroup(sheet.master);
            return true;
        }

        return false;
    }

    public class Unit
    {
        public string Key;
        public int Level;
        public int Exp;

        //?????? ??????????????? ?????????
        public int GetCurrentLevelEXP()
        {
            //?????? ?????? ?????? ?????? ????????? ????????? ??????
            int total = 0;
            for (int i = 1; i <= Level; i++)
            {
                var sheet = i.ToString().ToTableData<UnitLevel>();
                total += sheet.exp;
            }

            return Exp - total;
        }

        //?????? ??????????????? ????????? ?????? ?????????
        public int GetCurrentLevelUpExp()
        {
            var sheet = (Level + 1).ToString().ToTableData<UnitLevel>();
            if (sheet != null)
                return sheet.exp;
            return -1;
        }

        //???????????? ??????
        public bool IsMaxLevel()
        {
            UnitLevel sheet = Level.ToString().ToTableData<UnitLevel>();
            return sheet.max;
        }

        //????????? ?????? ?????? ??????
        public bool AvaliableLevelUp()
        {
            if (IsMaxLevel()) return false;
            var sheet = (Level + 1).ToString().ToTableData<UnitLevel>();
            return sheet.total <= Exp;
        }
    }
}