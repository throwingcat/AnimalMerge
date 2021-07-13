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

    public enum eUnitType
    {
        Nomral,
        Bad,
        None,
    }

    public enum eUnitDropState
    {
        Ready,
        Falling,
        Complete,
    }
    
    public class EnvironmentValue
    {
        public static float UNIT_SPAWN_DELAY = 1f;
        public static float MERGE_DELAY = 0.15f;
        public static float UNIT_SPRITE_BASE_SIZE = 300f;
        public static float WORLD_RATIO = 0.00521f;

        public static float SYNC_CAPTURE_DELAY = 1f;
        public static float BAD_BLOCK_TIMER = 5f;
        
        public static float COMBO_DURATION = 2f;

        public static float GAME_OVER_TIME_OUT = 5f;
    }

    public class Key
    {
        #region Pool Category
        public const string IngamePoolCategory = "ingame";
        public const string UIVFXPoolCategory = "ui_vfx";
        #endregion
        
        #region VFX
        public const string VFX_MERGE_ATTACK_TRAIL = "VFX@AttackTrail";
        public const string VFX_MERGE_ATTACK_BOMB = "VFX@AttackBomb";
        #endregion
    }
}

