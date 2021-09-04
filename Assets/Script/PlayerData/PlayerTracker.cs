using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerTracker
{
    private static PlayerTracker _instance;

    public static PlayerTracker Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PlayerTracker();
            return _instance;
        }
    }

    public Dictionary<string, int> Tracker = new Dictionary<string, int>();

    public bool Contains(string key)
    {
        return Get(key) != 0;
    }

    public int Get(string key)
    {
        if (Tracker.ContainsKey(key))
            return Tracker[key];
        return 0;
    }

    public int Set(string key, int value)
    {
        if (Tracker.ContainsKey(key) == false)
            Tracker.Add(key, 0);
        Tracker[key] += value;
        return Tracker[key];
    }

    public void OnUpdate(string json)
    {
        Tracker = new Dictionary<string, int>();
        if (json.IsNullOrEmpty() == false)
        {
            Tracker = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
            if (Tracker == null)
                Tracker = new Dictionary<string, int>();
        }
    }
}