using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Server;

public class Inventory
{
    private static Inventory _instance;

    public Dictionary<string, ItemInfo> Items = new Dictionary<string, ItemInfo>();

    public static Inventory Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Inventory();
            return _instance;
        }
    }

    public void Update(string key, int amount)
    {
        if (Items.ContainsKey(key) == false)
            Items.Add(key, new ItemInfo(key, amount));
        else
            Items[key].Amount += amount;
    }

    public void Update(string json)
    {
        Items = new Dictionary<string, ItemInfo>();
        if (json.IsNullOrEmpty() == false)
        {
            Items = JsonConvert.DeserializeObject<Dictionary<string, ItemInfo>>(json);
            if (Items == null)
                Items = new Dictionary<string, ItemInfo>();
        }
    }

    public int GetAmount(string key)
    {
        if (Items.ContainsKey(key))
            return Items[key].Amount;
        return 0;
    }

    public bool Has(string key)
    {
        return Items.ContainsKey(key);
    }
}