using System;
using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;
using Violet;

public class SyncManager
{
    private static SyncManager _instance;

    public static SyncManager Instance
    {
        get
        {
            if(_instance==null)
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
            return new Vector3(x,y,z);
        }
    }

    

    public System.Action<SyncPacket> OnSyncCapture;
    public System.Action<SyncPacket> OnSyncReceive;
    
    public SyncPacket Capture()
    {
        SyncPacket packet = new SyncPacket();
        
        var units = GameCore.Instance.UnitsInField;
        foreach (var unit in units)
        {
            SyncPacket.UnitData u =new SyncPacket.UnitData();
            u.UnitKey = unit.Sheet.key;
            u.UnitPosition = new SVector3(unit.transform.localPosition);
            u.UnitRotation = new SVector3(unit.transform.localRotation.eulerAngles);
            
            packet.UnitsDatas.Add(u);
        }

        OnSyncCapture?.Invoke(packet);

        return packet;
    }

    public void Receive(SyncPacket packet)
    {
        OnSyncReceive?.Invoke(packet);
    }
    
    
}
