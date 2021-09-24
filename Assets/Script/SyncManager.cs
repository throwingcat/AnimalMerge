using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.Tcp;
using MessagePack;
using UnityEngine;

public class SyncManager
{
    public enum ePacketType
    {
        PlayerInfo,
        Ready,
        UnitUpdate,
        AttackDamage,
        UpdateAttackCombo,
        UpdateStackDamage,
        GameResult,
        None
    }

    private readonly SyncPacket _syncPacket = new SyncPacket();

    public GameCore From;
    public Action<SyncPacket> OnSyncCapture;
    public Action<SyncPacket> OnSyncReceive;
    public GameCore To;

    public SyncManager(GameCore from)
    {
        From = from;
        From.MyReadyTime.Subscribe(value =>
        {
            _syncPacket.Add(new Ready
            {
                ReadyTime = value
            });
        }, false);

        From.MyStackDamage.Subscribe(value =>
        {
            _syncPacket.Add(new UpdateStackDamage
            {
                StackDamage = value
            });
        }, false);

        From.AttackDamage.Subscribe(value =>
        {
            _syncPacket.Add(new AttackDamage
            {
                Damage = value
            });
        }, false);

        From.AttackComboValue.Subscribe(value =>
        {
            _syncPacket.Add(new UpdateAttackCombo
            {
                Combo = value
            });
        }, false);

        From.isGameOver.Subscribe(value =>
        {
            _syncPacket.Add(new GameResult
            {
                isGameOver = value,
                GameOverTime = From.GameOverTime
            });
        }, false);
    }

    public void SetTo(GameCore to)
    {
        To = to;
    }

    public void Request(PacketBase packet)
    {
        _syncPacket.Add(packet);
    }
    
    public void Capture()
    {
        //유닛 목록 업데이트
        var pUpdateUnit = new UpdateUnit();
        var units = new List<UnitBase>();
        units.AddRange(From.UnitsInField);
        units.AddRange(From.BadUnits);
        foreach (var unit in units)
        {
            var u = new UnitData();
            u.UnitKey = unit.Sheet.key;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);

            pUpdateUnit.UnitsDatas.Add(u);
        }

        _syncPacket.Packets.Add(pUpdateUnit);

        From.AttackDamage.Clear();
        From.AttackComboValue.Clear();

        OnSyncCapture?.Invoke(_syncPacket);

        //싱글 플레이의 경우 From < - > To 끼리 바로 통신
        if (GameManager.Instance.isSinglePlay)
        {
            To.SyncManager.Receive(_syncPacket);
            _syncPacket.Packets.Clear();
            return;
        }

        //매치 서버로 송신
        if (Backend.Match.IsMatchServerConnect() && Backend.Match.IsInGameServerConnect())
        {
            var bytes = MessagePackSerializer.Serialize(_syncPacket);
            _syncPacket.Packets.Clear();
            Backend.Match.SendDataToInGameRoom(bytes);
        }
    }

    public void Receive(SyncPacket packet)
    {
        OnSyncReceive?.Invoke(packet);
    }

    public void OnReceiveMatchRelay(MatchRelayEventArgs args)
    {
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        var packet = MessagePackSerializer.Deserialize<SyncPacket>(args.BinaryUserData, lz4Options);

        if (args.From.NickName != Backend.UserNickName)
            Receive(packet);
    }

    [MessagePackObject]
    public class UnitData
    {
        [Key(0)] public string UnitKey;

        [Key(1)] public SVector3 UnitPosition;

        [Key(2)] public SVector3 UnitRotation;
    }

    [MessagePackObject]
    public class SyncPacket
    {
        [Key(0)] public List<PacketBase> Packets = new List<PacketBase>();

        public void Add(PacketBase packet)
        {
            var isContains = false;
            foreach (var p in Packets)
                if (p.PacketType == packet.PacketType)
                {
                    isContains = true;
                    switch (p.PacketType)
                    {
                        case ePacketType.Ready:
                            break;
                        case ePacketType.UnitUpdate:
                            break;
                        case ePacketType.AttackDamage:
                            p.UpdateValue(((AttackDamage) packet).Damage);
                            break;
                        case ePacketType.UpdateAttackCombo:
                            p.UpdateValue(((UpdateAttackCombo) packet).Combo);
                            break;
                        case ePacketType.UpdateStackDamage:
                            p.UpdateValue(((UpdateStackDamage) packet).StackDamage);
                            break;
                        case ePacketType.GameResult:
                            break;
                        case ePacketType.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

            if (isContains == false)
                Packets.Add(packet);
        }
    }

    [MessagePackObject]
    public class PacketBase
    {
        [Key(0)] public ePacketType PacketType = ePacketType.None;

        public virtual void UpdateValue(int value)
        {
        }
    }

    [MessagePackObject]
    public class PlayerInfo : PacketBase
    {
        [Key(3)] public string HeroKey;
        [Key(2)] public int MMR;
        [Key(1)] public string Name;

        public PlayerInfo()
        {
            PacketType = ePacketType.PlayerInfo;
        }
    }

    [MessagePackObject]
    public class Ready : PacketBase
    {
        [Key(1)] public DateTime ReadyTime;

        public Ready()
        {
            PacketType = ePacketType.Ready;
        }
    }

    [MessagePackObject]
    public class UpdateUnit : PacketBase
    {
        [Key(1)] public List<UnitData> UnitsDatas = new List<UnitData>();

        public UpdateUnit()
        {
            PacketType = ePacketType.UnitUpdate;
        }
    }

    [MessagePackObject]
    public class AttackDamage : PacketBase
    {
        [Key(1)] public int Damage;

        public AttackDamage()
        {
            PacketType = ePacketType.AttackDamage;
        }

        public override void UpdateValue(int value)
        {
            base.UpdateValue(value);
            Damage = value;
        }
    }

    public class UpdateAttackCombo : PacketBase
    {
        [Key(1)] public int Combo;

        public UpdateAttackCombo()
        {
            PacketType = ePacketType.UpdateAttackCombo;
        }

        public override void UpdateValue(int value)
        {
            base.UpdateValue(value);
            Combo = value;
        }
    }

    public class UpdateStackDamage : PacketBase
    {
        [Key(1)] public int StackDamage;

        public UpdateStackDamage()
        {
            PacketType = ePacketType.UpdateStackDamage;
        }

        public override void UpdateValue(int value)
        {
            base.UpdateValue(value);
            StackDamage = value;
        }
    }

    [MessagePackObject]
    public class GameResult : PacketBase
    {
        [Key(2)] public DateTime GameOverTime;
        [Key(1)] public bool isGameOver;

        public GameResult()
        {
            PacketType = ePacketType.GameResult;
        }
    }

    [Serializable]
    [MessagePackObject]
    public class SVector3
    {
        [Key(0)] public float x;

        [Key(1)] public float y;

        [Key(2)] public float z;

        public SVector3()
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }

        public SVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}