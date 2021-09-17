using System.Collections;
using System.Collections.Generic;
using MessagePack;
using Packet;

namespace Common
{
    [MessagePackObject]
    public class PacketBase
    {
        [Key(0)] public ePACKET_TYPE PacketType;
        [Key(1)] public Dictionary<string,object> hash = new Dictionary<string, object>();

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