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
        CHANGE_HERO,
        PURCHASE_PREMIUM_PASS,
        RECEIVE_PASS_REWARD,
        RECEIVE_PLAYER_LEVEL_REWARD,
    }
}