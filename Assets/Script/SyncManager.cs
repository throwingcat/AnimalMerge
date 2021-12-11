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
        UpdateUnit,
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

    public void Request(SyncPacketBase syncPacket)
    {
        _syncPacket.Add(syncPacket);
    }

    public void Capture()
    {
        //유닛 목록 업데이트
        var pUpdateUnit = new UpdateUnit();
        var units = new List<UnitBase>();
        units.AddRange(From.UnitsInField);
        units.AddRange(From.BadUnits);
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        foreach (var unit in units)
        {
            var u = new UnitData();
            u.UnitKey = (sbyte) unit.Sheet.index;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);

            pUpdateUnit.UnitDatas.Add(MessagePackSerializer.Serialize(u, lz4Options));
        }

        _syncPacket.Packets.Add(pUpdateUnit);

        From.AttackDamage.Clear();
        From.AttackComboValue.Clear();

        OnSyncCapture?.Invoke(_syncPacket);

        _syncPacket.Bytes.Clear();
        foreach (var packet in _syncPacket.Packets)
        {
            switch (packet.PacketType)
            {
                case ePacketType.PlayerInfo:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as PlayerInfo));
                    break;
                case ePacketType.Ready:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as Ready));
                    break;
                case ePacketType.UpdateUnit:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as UpdateUnit));
                    break;
                case ePacketType.AttackDamage:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as AttackDamage));
                    break;
                case ePacketType.UpdateAttackCombo:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as UpdateAttackCombo));
                    break;
                case ePacketType.UpdateStackDamage:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as UpdateStackDamage));
                    break;
                case ePacketType.GameResult:
                    _syncPacket.Bytes.Add(MessagePackSerializer.Serialize(packet as GameResult));
                    break;
            }
        }

        //싱글 플레이의 경우 From < - > To 끼리 바로 통신
        if (GameManager.Instance.isSinglePlay)
        {
            To.SyncManager.Receive(_syncPacket);
            _syncPacket.Packets.Clear();
            _syncPacket.Bytes.Clear();
            return;
        }

        //매치 서버로 송신
        if (Backend.Match.IsMatchServerConnect() && Backend.Match.IsInGameServerConnect())
        {
            var bytes = MessagePackSerializer.Serialize(_syncPacket, lz4Options);
            Debug.Log(string.Format("패킷 전송량 : {0}", bytes.Length));
            _syncPacket.Packets.Clear();
            _syncPacket.Bytes.Clear();
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
        [Key(0)] public sbyte UnitKey;
        [Key(1)] public SVector3 UnitPosition;
        [Key(2)] public SVector3 UnitRotation;
    }

    [MessagePackObject]
    public class SyncPacket
    {
        [Key(0)] public List<byte[]> Bytes = new List<byte[]>();
        [IgnoreMember] public List<SyncPacketBase> Packets = new List<SyncPacketBase>();

        public void Add<T>(T packet) where T : SyncPacketBase
        {
            var isContains = false;
            foreach (var p in Packets)
            {
                if (p.PacketType == packet.PacketType)
                {
                    isContains = true;
                    switch (p)
                    {
                        case PlayerInfo playerInfo:
                            Debug.Log(playerInfo.Name);
                            break;
                        case AttackDamage attackDamage:
                            p.UpdateValue(attackDamage.Damage);
                            break;
                        case UpdateAttackCombo updateAttackCombo:
                            p.UpdateValue(updateAttackCombo.Combo);
                            break;
                        case UpdateStackDamage updateStackDamage:
                            p.UpdateValue(updateStackDamage.StackDamage);
                            break;
                    }
                }
            }

            if (isContains == false)
                Packets.Add(packet);
        }
    }

    [MessagePackObject]
    public class SyncPacketBase
    {
        [Key(0)] public ePacketType PacketType = ePacketType.None;

        public virtual void UpdateValue(int value)
        {
        }
    }

    [MessagePackObject]
    public class PlayerInfo : SyncPacketBase
    {
        [Key(1)] public string HeroKey;
        [Key(2)] public int MMR;
        [Key(3)] public string Name;

        public PlayerInfo()
        {
            PacketType = ePacketType.PlayerInfo;
        }
    }

    [MessagePackObject]
    public class Ready : SyncPacketBase
    {
        [Key(1)] public DateTime ReadyTime;

        public Ready()
        {
            PacketType = ePacketType.Ready;
        }
    }

    [MessagePackObject]
    public class UpdateUnit : SyncPacketBase
    {
        [Key(1)] public List<byte[]> UnitDatas = new List<byte[]>();

        public UpdateUnit()
        {
            PacketType = ePacketType.UpdateUnit;
        }

        public List<UnitData> Convert()
        {
            List<UnitData> result = new List<UnitData>();
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            foreach (var bytes in UnitDatas)
            {
                var unit = MessagePackSerializer.Deserialize<UnitData>(bytes, lz4Options);
                result.Add(unit);
            }

            return result;
        }
    }

    [MessagePackObject]
    public class AttackDamage : SyncPacketBase
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

    public class UpdateAttackCombo : SyncPacketBase
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

    public class UpdateStackDamage : SyncPacketBase
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
    public class GameResult : SyncPacketBase
    {
        [Key(1)] public DateTime GameOverTime;
        [Key(2)] public bool isGameOver;

        public GameResult()
        {
            PacketType = ePacketType.GameResult;
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
            // xyz = (ulong)(vec.x);
            // xyz = (ulong)(xyz + vec.y * 1000);
            // xyz = (ulong)(xyz + vec.z * 1000000);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x / 10f, y / 10f, z / 10f);
            //return new Vector3(xyz % 1000, xyz % 1000000 / 1000, xyz % 1000000000 / 1000000);
        }
    }
}