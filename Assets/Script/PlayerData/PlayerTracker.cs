using System.Collections.Generic;
using Newtonsoft.Json;
using Server;
using UnityEngine;

public class PlayerTracker
{
    public const string COMBO3 = "COMBO3";
    public const string COMBO5 = "COMBO5";
    public const string COMBO7 = "COMBO7";
    public const string COMBO10 = "COMBO10";
    public const string REMOVE_BAD_BLOCK = "REMOVE_BAD_BLOCK";
    public const string USE_SKILL = "USE_SKILL";
    public const string ATTACK_DAMAGE = "ATTACK_DAMAGE";
    public const string DEFENCE_DAMAGE = "DEFENCE_DAMAGE";
    public const string DROP_BLOCK = "DROP_BLOCK";
    public const string MAX_COMBO = "MAX_COMBO";
    public const string BATTLE_WIN = "BATTLE_WIN";
    public const string BATTLE_LOSE = "BATTLE_LOSE";
    public const string BATTLE_PLAY = "BATTLE_PLAY";
    public const string MERGE_COUNT = "MERGE_COUNT";
    public const string USE_GOLD = "USE_GOLD";
    public const string USE_JEWEL = "USE_JEWEL";
    public const string QUEST_CLEAR = "QUEST_CLEAR";
    public const string PLAY_AD = "PLAY_AD";
    public const string GET_HERO_CAT = "GET_HERO_CAT";
    public const string GET_HERO_TAKO = "GET_HERO_TAKO";
    public const string UPGRADE_ANY = "UPGRADE_ANY";

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

    public Dictionary<string, ulong> Tracker = new Dictionary<string, ulong>();

    public bool Contains(string key)
    {
        return Get(key) != 0;
    }

    public ulong Get(string key)
    {
        if (Tracker.ContainsKey(key))
            return Tracker[key];
        return 0;
    }

    public ulong Report(string key, ulong value)
    {
        if (Tracker.ContainsKey(key) == false)
            Tracker.Add(key, 0);
        Tracker[key] += value;
        
        AnimalMergeServer.Instance.ReportAchievement(Tracker,null);
        AnimalMergeServer.Instance.ReportQuest(Tracker,null);
        
        
        return Tracker[key];
    }

    public void OnUpdate(string json)
    {
        Tracker = new Dictionary<string, ulong>();
        if (json.IsNullOrEmpty() == false)
        {
            Tracker = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(json);
            if (Tracker == null)
                Tracker = new Dictionary<string, ulong>();
        }
    }
}