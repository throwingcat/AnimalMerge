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

    public enum eItemType
    {
        Currency,
        Card,
        Hero,
    }
    
    public class EnvironmentValue
    {
        public static float UNIT_BASE_SIZE = 1f;
        public static float WORLD_RATIO = 0.00521f;
        
        public static float UNIT_SPAWN_DELAY = 1f;
        
        public static float COMBO_DURATION = 2f;
        public static float GAME_OVER_TIME_OUT = 5f;
        
        public static float SYNC_CAPTURE_DELAY = 1f;

        public static float DAMAGE_PER_SECOND = 0.05f;

        public static float RECOVERY_BAD_BLOCK_TIMER = 1f;
        public static float RECOVERY_GAMEOVER_TIMER = 1f;
        #region BadBlock
        
        public static float BAD_BLOCK_TIMER_MAX = 5f;
        public static float BAD_BLOCK_TIMER_MIN = 5f;
        public static float BAD_BLOCK_TIMER_PER_SECOND = 5f;
        public static int BAD_BLOCK_HORIZONTAL_MAX_COUNT = 6;
        public static int BAD_BLOCK_DROP_COUNT_MIN = 6;
        public static int BAD_BLOCK_DROP_COUNT_MAX = 36;
        public static float BAD_BLOCK_INCREASE_DROP_COUNT_PER_SECOND = 0.5f;
        public static float BAD_BLOCK_AREA_WIDTH = 800;
        public static float BAD_BLOCK_SPAWN_Y = 1050;
        public static float BAD_BLOCK_VERTICAL_OFFSET = 150;
        #endregion
        
        #region AI

        public static float AI_INPUT_DELAY_MAX = 3;
        public static float AI_INPUT_DELAY_MIN = 0;
        public static int AI_INPUT_DELAY_LOW_MMR = 1000;
        public static int AI_INPUT_DELAY_HIGH_MMR = 1050;
        
        #endregion
        
        #region Skill
        public static float SHAKE_SKILL_VERTICAL_POWER = 100f;
        public static float SHAKE_SKILL_HORIZONTAL_POWER = 1.0f;
        public static float SHAKE_SKILL_TORQUE_MAX_POWER = 2f;
        public static float SHAKE_SKILL_TORQUE_MIN_POWER = 1f;
        public static int SKILL_CHARGE_MAX_VALUE = 3000;
        #endregion
        
        #region Lobby

        public static int CHEST_SLOT_MAX_COUNT = 4;
        public static int CHEST_GRADE_MAX = 3;
        
        #endregion

    }

    public class Key
    {
        #region Currency

        public const string ITEM_COIN = "Coin";
        #endregion
        #region Pool Category
        public const string IngamePoolCategory = "ingame";
        public const string UIVFXPoolCategory = "ui_vfx";
        #endregion
        
        #region VFX
        public const string VFX_MERGE_ATTACK_TRAIL = "VFX@AttackTrail";
        public const string VFX_MERGE_ATTACK_BOMB = "VFX@AttackBomb";
        public const string VFX_MERGE_ATTACK_TRAIL_Red = "VFX@AttackTrail_Red";
        public const string VFX_MERGE_ATTACK_BOMB_Red = "VFX@AttackBomb_Red";
        #endregion
        
        #region Simple Timer

        public const string SIMPLE_TIMER_RUNNING_SKILL = "running_skill";

        #endregion
    }
}

