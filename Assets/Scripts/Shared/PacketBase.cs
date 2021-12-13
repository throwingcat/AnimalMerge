using System.Collections.Generic;
using MessagePack;

namespace Common
{
    public enum ePacketType
    {
        REPORT_GAME_RESULT,
        CHEST_PROGRESS,
        CHEST_COMPLETE,
        UNIT_LEVEL_UP,
        QUEST_COMPLETE,
        QUEST_REFRESH,
        DAILY_QUEST_REWARD,
        CHANGE_HERO,
        PURCHASE_PREMIUM_PASS,
        RECEIVE_PASS_REWARD,
        RECEIVE_PLAYER_LEVEL_REWARD
    }

    [MessagePackObject]
    public class PacketBase
    {
        [Key(1)] public Dictionary<string, object> hash = new Dictionary<string, object>();
        [Key(0)] public ePacketType PacketType;

        public void SetError(string reason = "unknown")
        {
            if (hash.ContainsKey("error") == false)
                hash.Add("error", true);
        }

        public bool isSuccess()
        {
            if (hash.ContainsKey("success"))
                return bool.Parse(hash["success"].ToString());
            return true;
        }
    }

    [MessagePackObject]
    public class PacketReward : PacketBase
    {
        [Key(2)] public List<ItemInfo> Rewards = new List<ItemInfo>();
    }
}