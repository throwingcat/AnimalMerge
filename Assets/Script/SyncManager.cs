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
            _syncPacket.Add(ePacketType.Ready,
                MessagePackSerializer.Serialize(new Ready
                {
                    ReadyTime = value
                }));
        }, false);

        From.MyStackDamage.Subscribe(value =>
        {
            _syncPacket.Add(ePacketType.UpdateStackDamage,
                MessagePackSerializer.Serialize(new UpdateStackDamage
                {
                    StackDamage = value
                }));
        }, false);

        From.AttackDamage.Subscribe(value =>
        {
            _syncPacket.Add(ePacketType.AttackDamage,
                MessagePackSerializer.Serialize(new AttackDamage
                {
                    Damage = value
                }));
        }, false);

        From.AttackComboValue.Subscribe(value =>
        {
            _syncPacket.Add(ePacketType.UpdateAttackCombo,
                MessagePackSerializer.Serialize(new UpdateAttackCombo
                {
                    Combo = value
                }));
        }, false);

        From.isGameOver.Subscribe(value =>
        {
            _syncPacket.Add(ePacketType.GameResult,
                MessagePackSerializer.Serialize(new GameResult
                {
                    isGameOver = value,
                    GameOverTime = From.GameOverTime
                }));
        }, false);
    }

    public void SetTo(GameCore to)
    {
        To = to;
    }

    public void Request(ePacketType type, byte[] bytes)
    {
        _syncPacket.Add(type, bytes);
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
            u.UnitKey = (sbyte) unit.Sheet.index;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);

            pUpdateUnit.UnitDatas.Add(u);
        }

        _syncPacket.Add(ePacketType.UpdateUnit, MessagePackSerializer.Serialize(pUpdateUnit));

        From.AttackDamage.Clear();
        From.AttackComboValue.Clear();

        OnSyncCapture?.Invoke(_syncPacket);

        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        var bytes = MessagePackSerializer.Serialize(_syncPacket, lz4Options);
        Debug.LogWarning("Packet Size : " +bytes.Length);
        
        //싱글 플레이의 경우 From < - > To 끼리 바로 통신
        if (GameManager.Instance.isSinglePlay)
        {
            To.SyncManager.Receive(_syncPacket);
            _syncPacket.Bytes.Clear();
            return;
        }

        //매치 서버로 송신
        if (Backend.Match.IsMatchServerConnect() && Backend.Match.IsInGameServerConnect())
        {
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
        [Key(0)] public Dictionary<ePacketType, byte[]> Bytes = new Dictionary<ePacketType, byte[]>();

        public void Add(ePacketType type, byte[] bytes)
        {
            var isContains = false;
            if (Bytes.ContainsKey(type))
                Bytes[type] = bytes;
            else
                Bytes.Add(type, bytes);
            // foreach (var p in Packets)
            // {
            //     if (p.PacketType == packet.PacketType)
            //     {
            //         isContains = true;
            //         switch (p)
            //         {
            //             case PlayerInfo playerInfo:
            //                 Debug.Log(playerInfo.Name);
            //                 break;
            //             case AttackDamage attackDamage:
            //                 p.UpdateValue(attackDamage.Damage);
            //                 break;
            //             case UpdateAttackCombo updateAttackCombo:
            //                 p.UpdateValue(updateAttackCombo.Combo);
            //                 break;
            //             case UpdateStackDamage updateStackDamage:
            //                 p.UpdateValue(updateStackDamage.StackDamage);
            //                 break;
            //         }
            //     }
            // }
            //
            // if (isContains == false)
            //     Packets.Add(packet);
        }
    }
    [MessagePackObject]
    public class PlayerInfo 
    {
        [Key(1)] public string HeroKey;
        [Key(2)] public int MMR;
        [Key(3)] public string Name;
    }

    [MessagePackObject]
    public class Ready 
    {
        [Key(1)] public DateTime ReadyTime;
    }

    [MessagePackObject]
    public class UpdateUnit 
    {
        [Key(1)] public List<UnitData> UnitDatas = new List<UnitData>();

        public List<UnitData> Convert()
        {
            List<UnitData> result = new List<UnitData>();
            foreach (var bytes in UnitDatas)
            {
                //var unit = MessagePackSerializer.Deserialize<UnitData>(bytes, lz4Options);
                result.Add(bytes);
            }

            return result;
        }
    }

    [MessagePackObject]
    public class AttackDamage
    {
        [Key(1)] public int Damage;
    }

    [MessagePackObject]
    public class UpdateAttackCombo
    {
        [Key(1)] public int Combo;
    }

    [MessagePackObject]
    public class UpdateStackDamage
    {
        [Key(1)] public int StackDamage;
    }

    [MessagePackObject]
    public class GameResult
    {
        [Key(1)] public DateTime GameOverTime;
        [Key(2)] public bool isGameOver;
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