using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace SyncPacketCollection
{
    public enum ePacketType
    {
        None = 0,
        PlayerInfo,
        Ready,
        UpdateUnit,
        AttackDamage,
        UpdateAttackCombo,
        UpdateStackDamage,
        GameResult
    }

    [MessagePackObject]
    public class SyncPacketBase
    {
        [Key(0)] public int PacketType;
    }

    [MessagePackObject]
    public class PlayerInfo : SyncPacketBase
    {
        [Key(1)] public string HeroKey;
        [Key(2)] public int MMR;
        [Key(3)] public string Name;

        public PlayerInfo()
        {
            PacketType = (int) ePacketType.PlayerInfo;
        }
    }

    [MessagePackObject]
    public class Ready : SyncPacketBase
    {
        [Key(1)] public DateTime ReadyTime;

        public Ready()
        {
            PacketType = (int) ePacketType.Ready;
        }
    }

    [MessagePackObject]
    public class UpdateUnit : SyncPacketBase
    {
        [Key(1)] public List<UnitData> UnitDatas = new List<UnitData>();

        public UpdateUnit()
        {
            PacketType = (int) ePacketType.UpdateUnit;
        }

        public List<UnitData> Convert()
        {
            var result = new List<UnitData>();
            foreach (var bytes in UnitDatas)
                //var unit = MessagePackSerializer.Deserialize<UnitData>(bytes, lz4Options);
                result.Add(bytes);

            return result;
        }
    }

    [MessagePackObject]
    public class UnitData
    {
        [Key(0)] public sbyte UnitKey;
        [Key(1)] public SVector3 UnitPosition;
        [Key(2)] public SVector3 UnitRotation;
    }

    [MessagePackObject]
    public class AttackDamage : SyncPacketBase
    {
        [Key(1)] public int Damage;

        public AttackDamage()
        {
            PacketType = (int) ePacketType.AttackDamage;
        }
    }

    [MessagePackObject]
    public class UpdateAttackCombo : SyncPacketBase
    {
        [Key(1)] public int Combo;

        public UpdateAttackCombo()
        {
            PacketType = (int) ePacketType.UpdateAttackCombo;
        }
    }

    [MessagePackObject]
    public class UpdateStackDamage : SyncPacketBase
    {
        [Key(1)] public int StackDamage;

        public UpdateStackDamage()
        {
            PacketType = (int) ePacketType.UpdateStackDamage;
        }
    }

    [MessagePackObject]
    public class GameResult : SyncPacketBase
    {
        [Key(1)] public DateTime GameOverTime;
        [Key(2)] public bool isGameOver;

        public GameResult()
        {
            PacketType = (int) ePacketType.GameResult;
        }
    }

    [Serializable]
    [MessagePackObject]
    public class SVector3
    {
        [Key(0)] public short x;
        [Key(1)] public short y;
        [Key(2)] public short z;

        public SVector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public SVector3(Vector3 vec)
        {
            x = (short) (vec.x * 10);
            y = (short) (vec.y * 10);
            z = (short) (vec.z * 10);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x / 10f, y / 10f, z / 10f);
        }
    }
}