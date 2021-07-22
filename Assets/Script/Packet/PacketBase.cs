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
    }
    
    [MessagePackObject]
    public class PacketBase
    {
        [Key(0)] public ePACKET_TYPE PacketType;
        [Key(1)] public Hashtable hash = new Hashtable();
    }
}