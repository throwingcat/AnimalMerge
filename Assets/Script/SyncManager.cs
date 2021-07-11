using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BackEnd;
using BackEnd.Tcp;
using Define;
using MessagePack;
using Newtonsoft.Json;
using UnityEngine;
using Violet;

public class SyncManager
{
    private static SyncManager _instance;

    public static SyncManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SyncManager();
            return _instance;
        }
    }

    [MessagePackObject]
    public class UnitData
    {
        [Key(0)]
        public string UnitKey;
        [Key(1)]
        public SVector3 UnitPosition;
        [Key(2)]
        public SVector3 UnitRotation;
    }
    [MessagePackObject]
    public class SyncPacket
    {
        [Key(0)]
        public List<UnitData> UnitsDatas = new List<UnitData>();
        [Key(1)]
        public int BadBlockValue = 0;
    }

    [System.Serializable ,MessagePackObject]
    public class SVector3
    {
        [Key(0)]
        public float x;
        [Key(1)]
        public float y;
        [Key(2)]
        public float z;

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

    public System.Action<SyncPacket> OnSyncCapture;
    public System.Action<SyncPacket> OnSyncReceive;

    public SyncPacket Capture()
    {
        SyncPacket packet = new SyncPacket();

        var units = GameCore.Instance.UnitsInField;
        units.AddRange(GameCore.Instance.BadUnits);
        foreach (var unit in units)
        {
            UnitData u = new UnitData();
            u.UnitKey = unit.Sheet.key;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);

            packet.UnitsDatas.Add(u);
        }

        packet.BadBlockValue = GameCore.Instance.AttackBadBlockValue;
        GameCore.Instance.AttackBadBlockValue = 0;
        
        OnSyncCapture?.Invoke(packet);

        //매치 서버로 송신
        if(Backend.Match.IsMatchServerConnect() && Backend.Match.IsInGameServerConnect())
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
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
        var packet = MessagePackSerializer.Deserialize<SyncPacket>(args.BinaryUserData,lz4Options);

        if (args.From.NickName != Backend.UserNickName)
            Receive(packet);
    }
}