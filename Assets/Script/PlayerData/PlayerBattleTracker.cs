using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleTracker
{
    private static PlayerBattleTracker _instance;

    public static PlayerBattleTracker Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PlayerBattleTracker();
            return _instance;
        }
    }

    public static Dictionary<string, int> Tracker = new Dictionary<string, int>();

    public static void Update(string key, int value)
    {
        if (Tracker.ContainsKey(key) == false)
            Tracker.Add(key, 0);
        Tracker[key] += value;
    }

    public static void UpdateMax(string key, int value)
    {
        if (Tracker.ContainsKey(key) == false)
            Tracker.Add(key, 0);
        if(Tracker[key] < value)
            Tracker[key] = value;
    }

    public static void Clear()
    {
        Tracker.Clear();
    }
}