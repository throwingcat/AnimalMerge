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
        GET_CHEST_LIST,
    }
    
    public class ReceivePacket
    {
        public string GUID;
        public PacketBase packet;
    }
    
    [MessagePackObject]
    public class PacketBase
    {
        [Key(0)]
        public ePACKET_TYPE PacketType;
        [Key(1)]
        public Hashtable hash = new Hashtable();
    }

    public class ChestInfo
    {
        public string Key;
        public DateTime ReceiveTime;
        public DateTime CreateTime;
    }
}