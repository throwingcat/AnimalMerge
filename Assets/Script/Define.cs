using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Define
{
    public enum eGAME_STATE
    {
        Intro,
        Lobby,
        Battle,
    }

    public enum eLanguage
    {
        Korean,
        English,
    }

    public class EnvironmentValue
    {
        public static float UNIT_SPAWN_DELAY = 1f;
        public static float MERGE_DELAY = 0.15f;
        public static float UNIT_SPRITE_BASE_SIZE = 300f;
        public static float WORLD_RATIO = 0.00521f;

    }
}

