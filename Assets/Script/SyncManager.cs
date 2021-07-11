using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BackEnd;
using BackEnd.Tcp;
using Define;
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

    public class SyncPacket
    {
        public class UnitData
        {
            public string UnitKey;
            public SVector3 UnitPosition;
            public SVector3 UnitRotation;
        }

        public List<UnitData> UnitsDatas = new List<UnitData>();
        public int BadBlockValue = 0;
    }

    [System.Serializable]
    public class SVector3
    {
        public float x, y, z;

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
            SyncPacket.UnitData u = new SyncPacket.UnitData();
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
            var message = JsonConvert.SerializeObject(packet, Formatting.None);
            var bytes = Encoding.UTF8.GetBytes(message);
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
        var message = Encoding.UTF8.GetString(args.BinaryUserData);
        var packet = JsonConvert.DeserializeObject<SyncPacket>(message);

        if (args.From.NickName != Backend.UserNickName)
            Receive(packet);
    }
}