using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Packet
{
    public enum ePACKET_TYPE
    {
        REPORT_GAME_RESULT,
        CHEST_PROGRESS,
        CHEST_COMPLETE,
        UNIT_LEVEL_UP,
        QUEST_COMPLETE,
        QUEST_REFRESH,
        DAILY_QUEST_REWARD,
    }

    [MessagePackObject]
    public class PacketBase
    {
        [Key(0)] public ePACKET_TYPE PacketType;
        [Key(1)] public Hashtable hash = new Hashtable();

        public bool isSuccess()
        {
            if (hash.ContainsKey("success"))
                return bool.Parse(hash["success"].ToString());
            return false;
        }
    }

    [MessagePackObject]
    public class PacketReward : PacketBase
    {
        [Key(2)] public List<ItemInfo> Rewards = new List<ItemInfo>();
    }
}