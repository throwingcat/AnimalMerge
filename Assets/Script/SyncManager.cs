using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.Tcp;
using MessagePack;
using UnityEngine;

public class SyncManager
{
    public Action<SyncPacket> OnSyncCapture;
    public Action<SyncPacket> OnSyncReceive;

    public GameCore From;
    public GameCore To;

    public SyncManager(GameCore from)
    {
        From = from;
    }

    public void SetTo(GameCore to)
    {
        To = to;
    }

    public SyncPacket Capture()
    {
        var packet = new SyncPacket();

        var units = new List<UnitBase>();
        units.AddRange(From.UnitsInField);
        units.AddRange(From.BadUnits);
        foreach (var unit in units)
        {
            var u = new UnitData();
            u.UnitKey = unit.Sheet.key;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);

            packet.UnitsDatas.Add(u);
        }

        packet.AttackDamage = From.AttackBadBlockValue;
        packet.AttackCombo = From.AttackComboValue;
        packet.StackDamage = From.MyBadBlockValue;
        From.AttackBadBlockValue = 0;
        From.AttackComboValue = 0;

        if (From.isGameOver)
        {
            packet.isGameOver = true;
            packet.GameOverTime = From.GameOverTime;
        }

        packet.isGameFinish = From.isGameFinish;

        OnSyncCapture?.Invoke(packet);

        //싱글 플레이의 경우 From < - > To 끼리 바로 통신
        if (GameManager.Instance.isSinglePlay)
        {
            To.SyncManager.Receive(packet);
            return packet;
        }

        //매치 서버로 송신
        if (Backend.Match.IsMatchServerConnect() && Backend.Match.IsInGameServerConnect())
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bytes = MessagePackSerializer.Serialize(packet, lz4Options);
            Backend.Match.SendDataToInGameRoom(bytes);
        }

        return packet;
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
        [Key(0)] public List<UnitData> UnitsDatas = new List<UnitData>();

        [Key(1)] public int AttackDamage;

        [Key(2)] public int AttackCombo;

        [Key(3)] public int StackDamage;

        [Key(4)] public bool isGameOver;

        [Key(5)] public DateTime GameOverTime;

        [Key(6)] public bool isGameFinish;

        [Key(7)] public string WinPlayer = "";
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