using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleTracker
{
    public const string COMBO3 = "COMBO3";
    public const string COMBO7 = "COMBO7";
    public const string COMBO10 = "COMBO10";
    public const string REMOVE_BAD_BLOCK = "REMOVE_BAD_BLOCK";
    public const string USE_SKILL = "USE_SKILL";
    public const string ATTACK_DAMAGE = "ATTACK_DAMAGE";
    public const string DEFENCE_DAMAGE = "DEFENCE_DAMAGE";
    public const string DROP_BLOCK = "DROP_BLOCK";
    public const string MAX_COMBO = "MAX_COMBO";
    public const string BATTLE_WIN = "BATTLE_WIN";
    public const string BATTLE_PLAY = "BATTLE_PLAY";

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